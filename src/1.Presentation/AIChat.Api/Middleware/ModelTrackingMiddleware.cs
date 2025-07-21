namespace AIChat.Api.Middleware;

/// <summary>
/// 模型追踪中间件 - 自动追踪并输出当前使用的AI模型
/// </summary>
public class ModelTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ModelTrackingMiddleware> _logger;

    public ModelTrackingMiddleware(RequestDelegate next, ILogger<ModelTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 记录请求开始时间
        var startTime = DateTime.UtcNow;
        
        // 从请求头中获取模型ID
        var modelId = context.Request.Headers["X-Model-Id"].FirstOrDefault();
        
        // 如果请求体是JSON，尝试解析模型ID
        if (string.IsNullOrEmpty(modelId) && context.Request.ContentType?.Contains("application/json") == true)
        {
            modelId = await ExtractModelIdFromRequestBodyAsync(context);
        }

        // 执行下一个中间件
        await _next(context);
        
        // 从响应头中获取实际使用的模型
        var actualModelUsed = context.Response.Headers["X-Current-Model"].FirstOrDefault();
        
        // 计算请求处理时间
        var duration = DateTime.UtcNow - startTime;
        
        // 输出模型追踪信息
        if (!string.IsNullOrEmpty(actualModelUsed))
        {
            _logger.LogInformation(
                "[模型追踪] 路径: {Path}, 使用模型: {ModelId}, 处理时间: {Duration}ms, 状态码: {StatusCode}",
                context.Request.Path,
                actualModelUsed,
                duration.TotalMilliseconds,
                context.Response.StatusCode);
                
            // 在控制台单独输出模型信息，便于快速查看
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 当前使用模型: {actualModelUsed}");
        }
        else if (!string.IsNullOrEmpty(modelId))
        {
            _logger.LogInformation(
                "[模型追踪] 路径: {Path}, 请求模型: {ModelId}, 处理时间: {Duration}ms, 状态码: {StatusCode}",
                context.Request.Path,
                modelId,
                duration.TotalMilliseconds,
                context.Response.StatusCode);
        }
    }

    /// <summary>
    /// 从请求体中提取模型ID
    /// </summary>
    private async Task<string?> ExtractModelIdFromRequestBodyAsync(HttpContext context)
    {
        try
        {
            // 启用请求体重用
            context.Request.EnableBuffering();
            
            // 读取请求体
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            // 重置流位置
            context.Request.Body.Position = 0;
            
            // 简单的JSON解析，查找modelId字段
            if (!string.IsNullOrEmpty(body) && body.Contains("modelId"))
            {
                try
                {
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(body);
                    if (jsonDoc.RootElement.TryGetProperty("modelId", out var modelIdElement))
                    {
                        return modelIdElement.GetString();
                    }
                }
                catch
                {
                    // JSON解析失败，忽略
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析请求体中的模型ID时发生错误");
        }
        
        return null;
    }
}