using AIChat.Domain.ValueObjects;

namespace AIChat.Domain.Services;

/// <summary>
/// AI模型服务接口 - 定义AI模型相关操作
/// </summary>
public interface IAIModelService
{
    /// <summary>
    /// 发送普通消息(非流式)
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="modelId">AI模型ID</param>
    /// <param name="conversationHistory">对话历史上下文</param>
    /// <returns>AI响应</returns>
    Task<ChatResponse> SendMessageAsync(string message, string modelId, List<string>? conversationHistory = null);

    /// <summary>
    /// 发送流式消息(支持思考过程)
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="modelId">AI模型ID</param>
    /// <param name="conversationHistory">对话历史上下文</param>
    /// <returns>流式响应</returns>
    IAsyncEnumerable<StreamingResponse> SendStreamingMessageAsync(string message, string modelId, List<string>? conversationHistory = null);

    /// <summary>
    /// 获取可用的AI模型列表
    /// </summary>
    /// <returns>模型配置列表</returns>
    Task<List<AIModelConfig>> GetAvailableModelsAsync();

    /// <summary>
    /// 根据ID获取模型配置
    /// </summary>
    /// <param name="modelId">模型ID</param>
    /// <returns>模型配置</returns>
    Task<AIModelConfig?> GetModelConfigAsync(string modelId);

    /// <summary>
    /// 获取默认模型ID
    /// </summary>
    /// <returns>默认模型ID</returns>
    Task<string> GetDefaultModelIdAsync();

    /// <summary>
    /// 验证模型是否可用
    /// </summary>
    /// <param name="modelId">模型ID</param>
    /// <returns>是否可用</returns>
    Task<bool> ValidateModelAsync(string modelId);
}