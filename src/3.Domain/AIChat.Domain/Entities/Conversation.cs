namespace AIChat.Domain.Entities;

/// <summary>
/// 对话实体 - 表示一次完整的对话会话
/// </summary>
public class Conversation
{
    /// <summary>
    /// 对话唯一标识
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 对话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 使用的AI模型ID
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 对话中的所有消息
    /// </summary>
    public List<Message> Messages { get; set; } = new();

    /// <summary>
    /// 添加消息到对话
    /// </summary>
    public void AddMessage(Message message)
    {
        message.ConversationId = Id;
        Messages.Add(message);
        MessageCount = Messages.Count;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新对话标题
    /// </summary>
    public void UpdateTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }
}