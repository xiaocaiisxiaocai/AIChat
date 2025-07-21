namespace AIChat.Domain.ValueObjects;

/// <summary>
/// AI模型配置值对象
/// </summary>
public class AIModelConfig
{
    /// <summary>
    /// 模型唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 模型显示名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 提供商：OpenAI、Anthropic等
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// API密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否支持流式输出
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// 是否支持思考过程(如Claude)
    /// </summary>
    public bool SupportsThinking { get; set; }

    /// <summary>
    /// API端点URL
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// 温度参数(控制输出随机性)
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// 判断模型是否有效配置
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(Id) && 
                          !string.IsNullOrEmpty(Name) && 
                          !string.IsNullOrEmpty(Provider) && 
                          !string.IsNullOrEmpty(ApiKey);

    /// <summary>
    /// 获取模型完整显示名称
    /// </summary>
    public string GetDisplayName() => $"{Name} ({Provider})";
}