using AIChat.Domain.Entities;
using AIChat.Domain.Repositories;
using AIChat.Domain.Services;
using AIChat.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AIChat.Application.Services;

/// <summary>
/// 聊天领域服务实现 - 实现核心业务逻辑
/// </summary>
public class ChatDomainService : IChatDomainService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IAIModelService _aiModelService;
    private readonly ILogger<ChatDomainService> _logger;

    public ChatDomainService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IAIModelService aiModelService,
        ILogger<ChatDomainService> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _aiModelService = aiModelService;
        _logger = logger;
    }

    /// <summary>
    /// 开始新对话
    /// </summary>
    public async Task<Conversation> StartNewConversationAsync(string modelId, string? title = null)
    {
        // 验证模型是否可用
        if (!await _aiModelService.ValidateModelAsync(modelId))
        {
            throw new InvalidOperationException($"Model {modelId} is not available");
        }

        var conversation = new Conversation
        {
            ModelId = modelId,
            Title = title ?? "新对话"
        };

        return await _conversationRepository.CreateAsync(conversation);
    }

    /// <summary>
    /// 处理用户消息并获取AI回复
    /// </summary>
    public async Task<ChatResponse> ProcessUserMessageAsync(string conversationId, string userMessage, string? modelId = null)
    {
        // 验证对话是否存在
        if (!await ConversationExistsAsync(conversationId))
        {
            throw new InvalidOperationException($"Conversation {conversationId} not found");
        }

        // 确定使用的模型
        var targetModelId = modelId ?? await GetConversationModelIdAsync(conversationId);
        
        // 获取对话历史
        var history = await GetConversationHistoryAsync(conversationId);
        var historyMessages = history.Select(m => $"{m.Role}: {m.Content}").ToList();

        // 调用AI服务获取响应
        return await _aiModelService.SendMessageAsync(userMessage, targetModelId, historyMessages);
    }

    /// <summary>
    /// 处理用户消息并获取流式AI回复
    /// </summary>
    public async IAsyncEnumerable<StreamingResponse> ProcessUserMessageStreamAsync(string conversationId, string userMessage, string? modelId = null)
    {
        // 验证对话是否存在
        if (!await ConversationExistsAsync(conversationId))
        {
            yield return StreamingResponse.CreateContent($"Error: Conversation {conversationId} not found", true);
            yield break;
        }

        // 确定使用的模型
        var targetModelId = modelId ?? await GetConversationModelIdAsync(conversationId);
        
        // 获取对话历史
        var history = await GetConversationHistoryAsync(conversationId);
        var historyMessages = history.Select(m => $"{m.Role}: {m.Content}").ToList();

        // 调用AI服务获取流式响应
        await foreach (var response in _aiModelService.SendStreamingMessageAsync(userMessage, targetModelId, historyMessages))
        {
            yield return response;
        }
    }

    /// <summary>
    /// 生成对话标题
    /// </summary>
    public async Task<string> GenerateConversationTitleAsync(string conversationId, string? modelId = null)
    {
        var history = await GetConversationHistoryAsync(conversationId, 2); // 只取前两条消息
        if (!history.Any())
        {
            return "新对话";
        }

        var firstUserMessage = history.FirstOrDefault(m => m.IsUserMessage)?.Content ?? "新对话";
        
        // 生成简短的标题
        var titlePrompt = $"请为以下对话生成一个简短的标题(不超过20个字符)，只返回标题内容，不要包含引号或其他格式：\n\n用户：{firstUserMessage}";
        
        try
        {
            var targetModelId = modelId ?? await GetConversationModelIdAsync(conversationId);
            var response = await _aiModelService.SendMessageAsync(titlePrompt, targetModelId);
            
            var title = response.Content.Trim().Trim('"').Trim();
            return string.IsNullOrEmpty(title) ? "新对话" : title;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate title for conversation {ConversationId}", conversationId);
            return firstUserMessage.Length > 20 ? firstUserMessage[..20] + "..." : firstUserMessage;
        }
    }

    /// <summary>
    /// 获取对话历史上下文
    /// </summary>
    public async Task<List<Message>> GetConversationHistoryAsync(string conversationId, int maxMessages = 10)
    {
        var messages = await _messageRepository.GetByConversationIdAsync(conversationId);
        return messages.OrderBy(m => m.CreatedAt).TakeLast(maxMessages).ToList();
    }

    /// <summary>
    /// 验证对话是否存在
    /// </summary>
    public async Task<bool> ConversationExistsAsync(string conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        return conversation != null;
    }

    /// <summary>
    /// 获取对话使用的模型ID
    /// </summary>
    private async Task<string> GetConversationModelIdAsync(string conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        return conversation?.ModelId ?? await _aiModelService.GetDefaultModelIdAsync();
    }
}