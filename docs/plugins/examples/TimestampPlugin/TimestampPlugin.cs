using AIChat.Shared.Plugins;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIChat.Plugins.Timestamp;

/// <summary>
/// 时间戳插件 - 为消息添加时间戳功能
/// </summary>
public class TimestampPlugin : IPlugin
{
    #region Plugin Properties
    
    public string Id => "com.aichat.plugins.timestamp";
    public string Name => "时间戳插件";
    public string Version => "1.0.0";
    public string Description => "为聊天消息添加精确时间戳，支持多种时间格式和时区转换";
    public string Author => "AIChat Team";
    public PluginType Type => PluginType.MessageProcessor;
    public bool IsEnabled { get; set; } = true;
    public string MinimumAIChatVersion => "1.0.0";
    public IReadOnlyList<string> Dependencies => new List<string>();

    #endregion

    #region Private Fields

    private IPluginContext? _context;
    private TimestampPluginConfig? _config;
    private readonly List<string> _supportedFormats = new()
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fff",
        "MM/dd/yyyy HH:mm:ss",
        "dd/MM/yyyy HH:mm:ss",
        "yyyy年MM月dd日 HH:mm:ss",
        "HH:mm:ss",
        "unix" // Unix时间戳
    };

    #endregion

    #region Plugin Lifecycle

    /// <summary>
    /// 插件初始化
    /// </summary>
    public async Task InitializeAsync(IPluginContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        
        // 获取插件配置
        _config = _context.GetPluginConfiguration<TimestampPluginConfig>(Id);
        
        // 注册API端点
        RegisterApis();
        
        // 订阅消息相关事件
        SubscribeToEvents();
        
        _context.Logger.LogInformation("时间戳插件初始化完成 - Version: {Version}", Version);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// 插件启动
    /// </summary>
    public async Task StartAsync()
    {
        if (_context == null || _config == null)
        {
            throw new InvalidOperationException("插件未正确初始化");
        }

        _context.Logger.LogInformation("时间戳插件已启动 - 格式: {Format}, 时区: {TimeZone}", 
            _config.DefaultFormat, _config.TimeZone);

        // 发布插件启动事件
        await _context.PublishEventAsync(new TimestampPluginEvent
        {
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            SourcePluginId = Id,
            Action = "Started",
            Message = "时间戳插件已成功启动"
        });
    }

    /// <summary>
    /// 插件停止
    /// </summary>
    public async Task StopAsync()
    {
        if (_context != null)
        {
            _context.Logger.LogInformation("时间戳插件正在停止...");
            
            // 发布插件停止事件
            await _context.PublishEventAsync(new TimestampPluginEvent
            {
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                SourcePluginId = Id,
                Action = "Stopped",
                Message = "时间戳插件已停止"
            });
        }
    }

    /// <summary>
    /// 插件资源清理
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            _context.Logger.LogInformation("时间戳插件资源清理完成");
        }
        
        _context = null;
        _config = null;
        
        await Task.CompletedTask;
    }

    #endregion

    #region Plugin Configuration

    /// <summary>
    /// 获取插件配置架构
    /// </summary>
    public string GetConfigurationSchema()
    {
        return JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                enabled = new
                {
                    type = "boolean",
                    @default = true,
                    description = "是否启用时间戳功能"
                },
                defaultFormat = new
                {
                    type = "string",
                    @default = "yyyy-MM-dd HH:mm:ss",
                    description = "默认时间格式",
                    @enum = _supportedFormats.ToArray()
                },
                timeZone = new
                {
                    type = "string",
                    @default = "Asia/Shanghai",
                    description = "时区设置"
                },
                showMilliseconds = new
                {
                    type = "boolean",
                    @default = false,
                    description = "是否显示毫秒"
                },
                autoAddToMessages = new
                {
                    type = "boolean",
                    @default = true,
                    description = "是否自动为消息添加时间戳"
                }
            },
            required = new[] { "enabled", "defaultFormat", "timeZone" }
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// 检查版本兼容性
    /// </summary>
    public bool IsCompatible(string aichatVersion)
    {
        try
        {
            var currentVersion = Version.Parse(aichatVersion);
            var minimumVersion = Version.Parse(MinimumAIChatVersion);
            return currentVersion >= minimumVersion;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 注册插件API
    /// </summary>
    private void RegisterApis()
    {
        if (_context == null) return;

        // 获取当前时间戳
        _context.RegisterApi("getCurrentTimestamp", async (parameters) =>
        {
            var format = _config?.DefaultFormat ?? "yyyy-MM-dd HH:mm:ss";
            var timeZone = _config?.TimeZone ?? "Asia/Shanghai";
            
            var timestamp = GetFormattedTimestamp(DateTime.UtcNow, format, timeZone);
            
            return new
            {
                timestamp,
                format,
                timeZone,
                utc = DateTime.UtcNow
            };
        });

        // 格式化指定时间
        _context.RegisterApi("formatTimestamp", async (parameters) =>
        {
            if (parameters is not JsonElement jsonParams)
                return new { error = "Invalid parameters" };

            var dateTime = jsonParams.TryGetProperty("dateTime", out var dtProp) 
                ? DateTime.Parse(dtProp.GetString() ?? DateTime.UtcNow.ToString())
                : DateTime.UtcNow;
                
            var format = jsonParams.TryGetProperty("format", out var formatProp)
                ? formatProp.GetString() ?? _config?.DefaultFormat ?? "yyyy-MM-dd HH:mm:ss"
                : _config?.DefaultFormat ?? "yyyy-MM-dd HH:mm:ss";
                
            var timeZone = jsonParams.TryGetProperty("timeZone", out var tzProp)
                ? tzProp.GetString() ?? _config?.TimeZone ?? "Asia/Shanghai"
                : _config?.TimeZone ?? "Asia/Shanghai";

            var timestamp = GetFormattedTimestamp(dateTime, format, timeZone);
            
            return new
            {
                timestamp,
                format,
                timeZone,
                originalDateTime = dateTime
            };
        });

        // 获取支持的时间格式
        _context.RegisterApi("getSupportedFormats", async (parameters) =>
        {
            return new
            {
                formats = _supportedFormats,
                defaultFormat = _config?.DefaultFormat ?? "yyyy-MM-dd HH:mm:ss"
            };
        });
    }

    /// <summary>
    /// 订阅系统事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (_context == null) return;

        // 订阅消息发送事件（模拟事件，实际需要根据系统事件定义）
        _context.SubscribeEvent<MessageSentEvent>(async (evt) =>
        {
            if (_config?.AutoAddToMessages == true && _config.Enabled)
            {
                var timestamp = GetFormattedTimestamp(evt.Timestamp, _config.DefaultFormat, _config.TimeZone);
                
                _context.Logger.LogDebug("为消息 {MessageId} 添加时间戳: {Timestamp}", 
                    evt.MessageId, timestamp);

                // 这里可以添加实际的消息时间戳处理逻辑
                await _context.Storage.SetAsync(Id, $"message_timestamp_{evt.MessageId}", timestamp);
            }
        });
    }

    /// <summary>
    /// 获取格式化的时间戳
    /// </summary>
    private string GetFormattedTimestamp(DateTime dateTime, string format, string timeZone)
    {
        try
        {
            // 转换时区
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);

            // 特殊处理Unix时间戳
            if (format.ToLower() == "unix")
            {
                var unixTimestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
                return unixTimestamp.ToString();
            }

            // 标准格式化
            return localTime.ToString(format);
        }
        catch (Exception ex)
        {
            _context?.Logger.LogError(ex, "时间戳格式化失败: {Format}, {TimeZone}", format, timeZone);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    #endregion
}

/// <summary>
/// 时间戳插件配置
/// </summary>
public class TimestampPluginConfig
{
    public bool Enabled { get; set; } = true;
    public string DefaultFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public string TimeZone { get; set; } = "Asia/Shanghai";
    public bool ShowMilliseconds { get; set; } = false;
    public bool AutoAddToMessages { get; set; } = true;
}

/// <summary>
/// 时间戳插件事件
/// </summary>
public class TimestampPluginEvent : IPluginEvent
{
    public string EventId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string SourcePluginId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 消息发送事件（示例事件，需要根据实际系统事件定义）
/// </summary>
public class MessageSentEvent : IPluginEvent
{
    public string EventId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string SourcePluginId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}