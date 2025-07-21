namespace AIChat.Application.DTOs;

/// <summary>
/// 对话数据传输对象
/// </summary>
public class ConversationDto
{
    /// <summary>
    /// 对话唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 对话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 使用的AI模型ID
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// 模型显示名称
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 最后一条消息内容预览
    /// </summary>
    public string LastMessagePreview { get; set; } = string.Empty;

    /// <summary>
    /// 对话消息列表(可选)
    /// </summary>
    public List<MessageDto>? Messages { get; set; }
}

/// <summary>
/// 消息数据传输对象
/// </summary>
public class MessageDto
{
    /// <summary>
    /// 消息唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

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
    public DateTime CreatedAt { get; set; }

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

/// <summary>
/// 创建对话请求数据传输对象
/// </summary>
public class CreateConversationRequestDto
{
    /// <summary>
    /// 对话标题(可选)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 使用的AI模型ID(可选，不提供则使用默认模型)
    /// </summary>
    public string? ModelId { get; set; }
}

/// <summary>
/// 更新对话请求数据传输对象
/// </summary>
public class UpdateConversationRequestDto
{
    /// <summary>
    /// 新的对话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
}