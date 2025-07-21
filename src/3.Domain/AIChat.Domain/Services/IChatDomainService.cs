using AIChat.Domain.Entities;
using AIChat.Domain.ValueObjects;

namespace AIChat.Domain.Services;

/// <summary>
/// 聊天领域服务接口 - 定义核心业务逻辑
/// </summary>
public interface IChatDomainService
{
    /// <summary>
    /// 开始新对话
    /// </summary>
    /// <param name="modelId">使用的AI模型ID</param>
    /// <param name="title">对话标题</param>
    /// <returns>新创建的对话</returns>
    Task<Conversation> StartNewConversationAsync(string modelId, string? title = null);

    /// <summary>
    /// 处理用户消息并获取AI回复
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <param name="userMessage">用户消息内容</param>
    /// <param name="modelId">使用的AI模型ID</param>
    /// <returns>AI响应</returns>
    Task<ChatResponse> ProcessUserMessageAsync(string conversationId, string userMessage, string? modelId = null);

    /// <summary>
    /// 处理用户消息并获取流式AI回复
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <param name="userMessage">用户消息内容</param>
    /// <param name="modelId">使用的AI模型ID</param>
    /// <returns>流式AI响应</returns>
    IAsyncEnumerable<StreamingResponse> ProcessUserMessageStreamAsync(string conversationId, string userMessage, string? modelId = null);

    /// <summary>
    /// 生成对话标题
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <param name="modelId">使用的AI模型ID</param>
    /// <returns>生成的标题</returns>
    Task<string> GenerateConversationTitleAsync(string conversationId, string? modelId = null);

    /// <summary>
    /// 获取对话历史上下文
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <param name="maxMessages">最大消息数量</param>
    /// <returns>对话历史</returns>
    Task<List<Message>> GetConversationHistoryAsync(string conversationId, int maxMessages = 10);

    /// <summary>
    /// 验证对话是否存在
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ConversationExistsAsync(string conversationId);
}