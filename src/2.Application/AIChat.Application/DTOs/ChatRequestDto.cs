namespace AIChat.Application.DTOs;

/// <summary>
/// 聊天请求数据传输对象
/// </summary>
public class ChatRequestDto
{
    /// <summary>
    /// 用户消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 对话ID(可选，用于继续现有对话)
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// 使用的AI模型ID(可选，不提供则使用默认模型)
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// 是否使用流式响应
    /// </summary>
    public bool UseStreaming { get; set; } = true;
}

/// <summary>
/// 聊天响应数据传输对象
/// </summary>
public class ChatResponseDto
{
    /// <summary>
    /// 对话ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// AI回答内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 思考过程内容(仅Claude等支持)
    /// </summary>
    public string? ThinkingContent { get; set; }

    /// <summary>
    /// 使用的模型ID
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// 响应创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 用户消息ID
    /// </summary>
    public string UserMessageId { get; set; } = string.Empty;

    /// <summary>
    /// AI消息ID
    /// </summary>
    public string AssistantMessageId { get; set; } = string.Empty;
}

/// <summary>
/// 流式聊天响应数据传输对象
/// </summary>
public class StreamingChatResponseDto
{
    /// <summary>
    /// 响应类型：thinking(思考) 或 content(内容)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 内容片段
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// 对话ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// 使用的模型ID
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;
}