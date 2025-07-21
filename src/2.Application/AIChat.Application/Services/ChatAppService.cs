using AIChat.Application.DTOs;
using AIChat.Domain.Entities;
using AIChat.Domain.Repositories;
using AIChat.Domain.Services;
using AIChat.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AIChat.Application.Services;

/// <summary>
/// 聊天应用服务 - 编排聊天业务流程
/// </summary>
public class ChatAppService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IAIModelService _aiModelService;
    private readonly IChatDomainService _chatDomainService;
    private readonly ILogger<ChatAppService> _logger;

    public ChatAppService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IAIModelService aiModelService,
        IChatDomainService chatDomainService,
        ILogger<ChatAppService> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _aiModelService = aiModelService;
        _chatDomainService = chatDomainService;
        _logger = logger;
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
    {
        try
        {
            // 确定使用的模型ID
            var modelId = request.ModelId ?? await _aiModelService.GetDefaultModelIdAsync();
            
            // 验证模型是否可用
            if (!await _aiModelService.ValidateModelAsync(modelId))
            {
                throw new InvalidOperationException($"Model {modelId} is not available");
            }

            // 处理对话
            Conversation conversation;
            if (string.IsNullOrEmpty(request.ConversationId))
            {
                // 创建新对话
                conversation = await _chatDomainService.StartNewConversationAsync(modelId);
            }
            else
            {
                // 获取现有对话
                conversation = await _conversationRepository.GetByIdAsync(request.ConversationId) 
                              ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found");
            }

            // 创建用户消息
            var userMessage = Message.CreateUserMessage(request.Message, conversation.Id);
            userMessage = await _messageRepository.CreateAsync(userMessage);

            // 获取AI响应
            var aiResponse = await _chatDomainService.ProcessUserMessageAsync(
                conversation.Id, 
                request.Message, 
                modelId);

            // 创建AI消息
            var assistantMessage = Message.CreateAssistantMessage(
                aiResponse.Content,
                aiResponse.ModelUsed,
                conversation.Id,
                aiResponse.ThinkingContent);
            assistantMessage = await _messageRepository.CreateAsync(assistantMessage);

            // 更新对话
            conversation.AddMessage(userMessage);
            conversation.AddMessage(assistantMessage);
            await _conversationRepository.UpdateAsync(conversation);

            // 如果对话没有标题，生成一个
            if (string.IsNullOrEmpty(conversation.Title))
            {
                try
                {
                    var title = await _chatDomainService.GenerateConversationTitleAsync(conversation.Id, modelId);
                    conversation.UpdateTitle(title);
                    await _conversationRepository.UpdateAsync(conversation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate conversation title for {ConversationId}", conversation.Id);
                }
            }

            return new ChatResponseDto
            {
                ConversationId = conversation.Id,
                Content = aiResponse.Content,
                ThinkingContent = aiResponse.ThinkingContent,
                ModelUsed = aiResponse.ModelUsed,
                CreatedAt = aiResponse.CreatedAt,
                UserMessageId = userMessage.Id,
                AssistantMessageId = assistantMessage.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            throw;
        }
    }

    /// <summary>
    /// 发送流式聊天消息
    /// </summary>
    public async IAsyncEnumerable<StreamingChatResponseDto> SendStreamingMessageAsync(ChatRequestDto request)
    {
        // 确定使用的模型ID
        var modelId = request.ModelId ?? await _aiModelService.GetDefaultModelIdAsync();
        
        // 验证模型是否可用
        if (!await _aiModelService.ValidateModelAsync(modelId))
        {
            yield return new StreamingChatResponseDto
            {
                Type = "error",
                Content = $"Model {modelId} is not available",
                IsComplete = true
            };
            yield break;
        }

        // 处理对话
        Conversation conversation;
        if (string.IsNullOrEmpty(request.ConversationId))
        {
            // 创建新对话
            conversation = await _chatDomainService.StartNewConversationAsync(modelId);
        }
        else
        {
            // 获取现有对话
            var existingConversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
            if (existingConversation is null)
            {
                yield return new StreamingChatResponseDto
                {
                    Type = "error",
                    Content = $"Conversation {request.ConversationId} not found",
                    IsComplete = true
                };
                yield break;
            }
            conversation = existingConversation;
        }

        // 创建用户消息
        var userMessage = Message.CreateUserMessage(request.Message, conversation.Id);
        userMessage = await _messageRepository.CreateAsync(userMessage);

        var fullContent = string.Empty;
        var fullThinking = string.Empty;

        // 获取流式AI响应
        await foreach (var streamResponse in _chatDomainService.ProcessUserMessageStreamAsync(
            conversation.Id, 
            request.Message, 
            modelId))
        {
            yield return new StreamingChatResponseDto
            {
                Type = streamResponse.Type,
                Content = streamResponse.Content,
                IsComplete = streamResponse.IsComplete,
                ConversationId = conversation.Id,
                ModelUsed = modelId
            };

            // 累积内容
            if (streamResponse.Type == "content")
            {
                fullContent += streamResponse.Content;
            }
            else if (streamResponse.Type == "thinking")
            {
                fullThinking += streamResponse.Content;
            }

            // 如果流式响应完成，保存AI消息
            if (streamResponse.IsComplete)
            {
                var assistantMessage = Message.CreateAssistantMessage(
                    fullContent,
                    modelId,
                    conversation.Id,
                    string.IsNullOrEmpty(fullThinking) ? null : fullThinking);
                await _messageRepository.CreateAsync(assistantMessage);

                // 更新对话
                conversation.AddMessage(userMessage);
                conversation.AddMessage(assistantMessage);
                await _conversationRepository.UpdateAsync(conversation);
            }
        }
    }

    /// <summary>
    /// 获取对话列表
    /// </summary>
    public async Task<List<ConversationDto>> GetConversationsAsync(int pageIndex = 1, int pageSize = 20)
    {
        var conversations = await _conversationRepository.GetListAsync(pageIndex, pageSize);
        var availableModels = await _aiModelService.GetAvailableModelsAsync();
        var modelDict = availableModels.ToDictionary(m => m.Id, m => m);

        return conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            ModelId = c.ModelId,
            ModelName = modelDict.TryGetValue(c.ModelId, out var model) ? model.Name : c.ModelId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            MessageCount = c.MessageCount,
            LastMessagePreview = GetLastMessagePreview(c.Messages)
        }).ToList();
    }

    /// <summary>
    /// 根据ID获取对话详情
    /// </summary>
    public async Task<ConversationDto?> GetConversationByIdAsync(string id)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id);
        if (conversation == null) return null;

        var availableModels = await _aiModelService.GetAvailableModelsAsync();
        var model = availableModels.FirstOrDefault(m => m.Id == conversation.ModelId);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            ModelName = model?.Name ?? conversation.ModelId,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            MessageCount = conversation.MessageCount,
            LastMessagePreview = GetLastMessagePreview(conversation.Messages),
            Messages = conversation.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                ThinkingContent = m.ThinkingContent,
                ModelId = m.ModelId,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }

    /// <summary>
    /// 创建新对话
    /// </summary>
    public async Task<ConversationDto> CreateConversationAsync(CreateConversationRequestDto request)
    {
        var modelId = request.ModelId ?? await _aiModelService.GetDefaultModelIdAsync();
        var conversation = await _chatDomainService.StartNewConversationAsync(modelId, request.Title);
        
        var model = await _aiModelService.GetModelConfigAsync(modelId);
        
        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            ModelName = model?.Name ?? modelId,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            MessageCount = conversation.MessageCount,
            LastMessagePreview = string.Empty
        };
    }

    /// <summary>
    /// 更新对话标题
    /// </summary>
    public async Task<ConversationDto?> UpdateConversationAsync(string id, UpdateConversationRequestDto request)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id);
        if (conversation == null) return null;

        conversation.UpdateTitle(request.Title);
        await _conversationRepository.UpdateAsync(conversation);

        var model = await _aiModelService.GetModelConfigAsync(conversation.ModelId);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ModelId = conversation.ModelId,
            ModelName = model?.Name ?? conversation.ModelId,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            MessageCount = conversation.MessageCount,
            LastMessagePreview = GetLastMessagePreview(conversation.Messages)
        };
    }

    /// <summary>
    /// 删除对话
    /// </summary>
    public async Task<bool> DeleteConversationAsync(string id)
    {
        return await _conversationRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取可用的AI模型列表
    /// </summary>
    public async Task<List<AIModelConfig>> GetAvailableModelsAsync()
    {
        return await _aiModelService.GetAvailableModelsAsync();
    }

    /// <summary>
    /// 获取最后一条消息的预览文本
    /// </summary>
    private static string GetLastMessagePreview(List<Message> messages)
    {
        if (!messages.Any()) return string.Empty;
        
        var lastMessage = messages.OrderByDescending(m => m.CreatedAt).First();
        var preview = lastMessage.Content;
        
        // 限制预览长度
        return preview.Length > 100 ? preview[..100] + "..." : preview;
    }
}