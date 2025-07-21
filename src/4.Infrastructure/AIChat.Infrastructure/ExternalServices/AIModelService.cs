using AIChat.Domain.Services;
using AIChat.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;

namespace AIChat.Infrastructure.ExternalServices;

/// <summary>
/// AI模型服务实现 - 使用Microsoft Semantic Kernel
/// </summary>
public class AIModelService : IAIModelService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIModelService> _logger;
    private readonly Dictionary<string, AIModelConfig> _modelConfigs;
    private readonly HttpClient _httpClient;

    public AIModelService(IConfiguration configuration, ILogger<AIModelService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
        _modelConfigs = LoadModelConfigurations();
    }

    /// <summary>
    /// 发送普通消息(非流式)
    /// </summary>
    public async Task<ChatResponse> SendMessageAsync(string message, string modelId, List<string>? conversationHistory = null)
    {
        var config = await GetModelConfigAsync(modelId);
        if (config == null)
        {
            throw new InvalidOperationException($"Model {modelId} not found or not configured");
        }

        try
        {
            // 对于SiliconFlow，直接调用API
            if (config.Provider.ToLower() == "siliconflow")
            {
                return await CallSiliconFlowApiAsync(config, message, conversationHistory);
            }

            // 对于其他提供商，使用Semantic Kernel
            var kernel = CreateKernel(config);
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var chatHistory = new ChatHistory();
            
            // 添加历史对话
            if (conversationHistory != null)
            {
                foreach (var historyMessage in conversationHistory)
                {
                    chatHistory.AddUserMessage(historyMessage);
                }
            }
            
            chatHistory.AddUserMessage(message);

            var result = await chatService.GetChatMessageContentAsync(chatHistory);

            return new ChatResponse
            {
                Content = result.Content ?? string.Empty,
                ModelUsed = modelId,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to model {ModelId}", modelId);
            throw;
        }
    }

    /// <summary>
    /// 发送流式消息(支持思考过程)
    /// </summary>
    public async IAsyncEnumerable<StreamingResponse> SendStreamingMessageAsync(string message, string modelId, List<string>? conversationHistory = null)
    {
        var config = await GetModelConfigAsync(modelId);
        if (config == null)
        {
            yield return StreamingResponse.CreateContent($"Error: Model {modelId} not found", true);
            yield break;
        }

        if (!config.SupportsStreaming)
        {
            // 如果模型不支持流式，降级为普通调用
            var response = await SendMessageAsync(message, modelId, conversationHistory);
            yield return StreamingResponse.CreateContent(response.Content, true);
            yield break;
        }

        var kernel = CreateKernel(config);
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        
        // 添加历史对话
        if (conversationHistory != null)
        {
            foreach (var historyMessage in conversationHistory)
            {
                chatHistory.AddUserMessage(historyMessage);
            }
        }
        
        chatHistory.AddUserMessage(message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = config.MaxTokens ?? 4000,
            Temperature = config.Temperature ?? 0.7
        };

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                yield return StreamingResponse.CreateContent(chunk.Content, false);
            }
        }

        yield return StreamingResponse.CreateContent("", true);
    }

    /// <summary>
    /// 获取可用的AI模型列表
    /// </summary>
    public async Task<List<AIModelConfig>> GetAvailableModelsAsync()
    {
        return await Task.FromResult(_modelConfigs.Values.Where(m => m.IsValid).ToList());
    }

    /// <summary>
    /// 根据ID获取模型配置
    /// </summary>
    public async Task<AIModelConfig?> GetModelConfigAsync(string modelId)
    {
        return await Task.FromResult(_modelConfigs.TryGetValue(modelId, out var config) ? config : null);
    }

    /// <summary>
    /// 获取默认模型ID
    /// </summary>
    public async Task<string> GetDefaultModelIdAsync()
    {
        var defaultModelId = _configuration["AIModels:DefaultModel"];
        if (string.IsNullOrEmpty(defaultModelId))
        {
            var availableModels = await GetAvailableModelsAsync();
            return availableModels.FirstOrDefault()?.Id ?? "gpt-3.5-turbo";
        }
        return defaultModelId;
    }

    /// <summary>
    /// 验证模型是否可用
    /// </summary>
    public async Task<bool> ValidateModelAsync(string modelId)
    {
        var config = await GetModelConfigAsync(modelId);
        return config?.IsValid ?? false;
    }

    /// <summary>
    /// 加载模型配置
    /// </summary>
    private Dictionary<string, AIModelConfig> LoadModelConfigurations()
    {
        var configs = new Dictionary<string, AIModelConfig>();
        var modelsSection = _configuration.GetSection("AIModels:Models");

        foreach (var modelSection in modelsSection.GetChildren())
        {
            var config = new AIModelConfig
            {
                Id = modelSection["Id"] ?? string.Empty,
                Name = modelSection["Name"] ?? string.Empty,
                Provider = modelSection["Provider"] ?? string.Empty,
                ApiKey = GetApiKey(modelSection["Provider"] ?? string.Empty),
                SupportsStreaming = bool.Parse(modelSection["SupportsStreaming"] ?? "true"),
                SupportsThinking = bool.Parse(modelSection["SupportsThinking"] ?? "false"),
                ApiEndpoint = modelSection["ApiEndpoint"],
                ModelName = modelSection["ModelName"],
                MaxTokens = int.TryParse(modelSection["MaxTokens"], out var maxTokens) ? maxTokens : null,
                Temperature = double.TryParse(modelSection["Temperature"], out var temperature) ? temperature : null
            };

            if (config.IsValid)
            {
                configs[config.Id] = config;
                _logger.LogInformation("Loaded model configuration: {ModelId} ({Provider})", config.Id, config.Provider);
            }
            else
            {
                _logger.LogWarning("Invalid model configuration: {ModelId}", config.Id);
            }
        }

        return configs;
    }

    /// <summary>
    /// 获取API密钥
    /// </summary>
    private string GetApiKey(string provider)
    {
        return provider.ToLower() switch
        {
            "openai" => Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty,
            "anthropic" => Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? string.Empty,
            "siliconflow" => Environment.GetEnvironmentVariable("SILICONFLOW_API_KEY") ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// 直接调用SiliconFlow API
    /// </summary>
    private async Task<ChatResponse> CallSiliconFlowApiAsync(AIModelConfig config, string message, List<string>? conversationHistory = null)
    {
        var apiEndpoint = config.ApiEndpoint ?? "https://api.siliconflow.cn/v1";
        var url = $"{apiEndpoint}/chat/completions";

        // 构建消息列表
        var messages = new List<object>();
        
        // 添加历史对话
        if (conversationHistory != null)
        {
            foreach (var historyMessage in conversationHistory)
            {
                messages.Add(new { role = "user", content = historyMessage });
            }
        }
        
        // 添加当前消息
        messages.Add(new { role = "user", content = message });

        var requestBody = new
        {
            model = config.ModelName ?? config.Id,
            messages = messages,
            max_tokens = config.MaxTokens ?? 4000,
            temperature = config.Temperature ?? 0.7
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");

        var response = await _httpClient.PostAsync(url, httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"SiliconFlow API error: {response.StatusCode} - {responseContent}");
        }

        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        var choice = jsonResponse.GetProperty("choices")[0];
        var messageObj = choice.GetProperty("message");
        var content = messageObj.GetProperty("content").GetString() ?? "";
        
        // 检查是否有思考过程内容（reasoning_content）
        string? thinkingContent = null;
        if (messageObj.TryGetProperty("reasoning_content", out var reasoningElement))
        {
            thinkingContent = reasoningElement.GetString();
        }

        return new ChatResponse
        {
            Content = content,
            ThinkingContent = thinkingContent,
            ModelUsed = config.Id,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建Semantic Kernel实例
    /// </summary>
    private Kernel CreateKernel(AIModelConfig config)
    {
        var builder = Kernel.CreateBuilder();

        switch (config.Provider.ToLower())
        {
            case "openai":
                builder.AddOpenAIChatCompletion(
                    modelId: config.Id,
                    apiKey: config.ApiKey);
                break;
            case "anthropic":
                // 注意：Semantic Kernel可能需要特定的Anthropic连接器
                // 这里使用OpenAI兼容格式作为示例
                builder.AddOpenAIChatCompletion(
                    modelId: config.Id,
                    apiKey: config.ApiKey);
                break;
            case "siliconflow":
                // SiliconFlow 使用 OpenAI 兼容的 API
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(config.ApiEndpoint ?? "https://api.siliconflow.cn/v1")
                };
                builder.AddOpenAIChatCompletion(
                    modelId: config.ModelName ?? config.Id,
                    apiKey: config.ApiKey,
                    httpClient: httpClient);
                break;
            default:
                throw new NotSupportedException($"Provider {config.Provider} is not supported");
        }

        return builder.Build();
    }
}