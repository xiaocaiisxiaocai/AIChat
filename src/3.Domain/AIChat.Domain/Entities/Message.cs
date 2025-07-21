namespace AIChat.Domain.Entities;

/// <summary>
/// 消息实体 - 表示对话中的单条消息
/// </summary>
public class Message
{
    /// <summary>
    /// 消息唯一标识
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 所属对话ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// 消息角色：user(用户) 或 assistant(AI助手)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// AI思考过程内容(仅Claude等支持思考的模型使用)
    /// </summary>
    public string? ThinkingContent { get; set; }

    /// <summary>
    /// 使用的AI模型ID
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// 消息创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建用户消息
    /// </summary>
    public static Message CreateUserMessage(string content, string conversationId)
    {
        return new Message
        {
            Role = "user",
            Content = content,
            ConversationId = conversationId
        };
    }

    /// <summary>
    /// 创建AI助手消息
    /// </summary>
    public static Message CreateAssistantMessage(string content, string modelId, string conversationId, string? thinkingContent = null)
    {
        return new Message
        {
            Role = "assistant",
            Content = content,
            ThinkingContent = thinkingContent,
            ModelId = modelId,
            ConversationId = conversationId
        };
    }

    /// <summary>
    /// 判断是否为用户消息
    /// </summary>
    public bool IsUserMessage => Role == "user";

    /// <summary>
    /// 判断是否为AI助手消息
    /// </summary>
    public bool IsAssistantMessage => Role == "assistant";

    /// <summary>
    /// 判断是否包含思考过程
    /// </summary>
    public bool HasThinking => !string.IsNullOrEmpty(ThinkingContent);
}