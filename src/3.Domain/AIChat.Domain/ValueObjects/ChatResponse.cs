namespace AIChat.Domain.ValueObjects;

/// <summary>
/// AI聊天响应值对象
/// </summary>
public class ChatResponse
{
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 令牌使用统计
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// 判断是否包含思考过程
    /// </summary>
    public bool HasThinking => !string.IsNullOrEmpty(ThinkingContent);
}

/// <summary>
/// 流式响应值对象
/// </summary>
public class StreamingResponse
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
    /// 创建思考类型的流式响应
    /// </summary>
    public static StreamingResponse CreateThinking(string content, bool isComplete = false)
    {
        return new StreamingResponse
        {
            Type = "thinking",
            Content = content,
            IsComplete = isComplete
        };
    }

    /// <summary>
    /// 创建内容类型的流式响应
    /// </summary>
    public static StreamingResponse CreateContent(string content, bool isComplete = false)
    {
        return new StreamingResponse
        {
            Type = "content",
            Content = content,
            IsComplete = isComplete
        };
    }
}

/// <summary>
/// 令牌使用统计
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// 输入令牌数
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// 输出令牌数
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// 总令牌数
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;
}