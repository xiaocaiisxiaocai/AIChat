using System.ComponentModel;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件基础接口 - 所有插件必须实现的核心接口
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 插件唯一标识符
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// 插件名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 插件版本
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// 插件描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 插件作者
    /// </summary>
    string Author { get; }
    
    /// <summary>
    /// 插件类型
    /// </summary>
    PluginType Type { get; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// 插件依赖的最低AIChat版本
    /// </summary>
    string MinimumAIChatVersion { get; }
    
    /// <summary>
    /// 插件依赖项
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }
    
    /// <summary>
    /// 插件初始化
    /// </summary>
    /// <param name="context">插件上下文</param>
    /// <returns></returns>
    Task InitializeAsync(IPluginContext context);
    
    /// <summary>
    /// 插件启动
    /// </summary>
    /// <returns></returns>
    Task StartAsync();
    
    /// <summary>
    /// 插件停止
    /// </summary>
    /// <returns></returns>
    Task StopAsync();
    
    /// <summary>
    /// 插件销毁
    /// </summary>
    /// <returns></returns>
    Task DisposeAsync();
    
    /// <summary>
    /// 获取插件配置架构
    /// </summary>
    /// <returns>JSON Schema描述插件配置项</returns>
    string GetConfigurationSchema();
    
    /// <summary>
    /// 验证插件兼容性
    /// </summary>
    /// <param name="aichatVersion">AIChat版本</param>
    /// <returns></returns>
    bool IsCompatible(string aichatVersion);
}

/// <summary>
/// 插件类型枚举
/// </summary>
public enum PluginType
{
    [Description("AI模型插件")]
    AIProvider,
    
    [Description("UI组件插件")]
    UIComponent,
    
    [Description("消息处理插件")]
    MessageProcessor,
    
    [Description("存储插件")]
    Storage,
    
    [Description("通知插件")]
    Notification,
    
    [Description("工具插件")]
    Tool,
    
    [Description("主题插件")]
    Theme,
    
    [Description("扩展插件")]
    Extension
}

/// <summary>
/// 插件状态
/// </summary>
public enum PluginStatus
{
    Unloaded,
    Loading,
    Loaded,
    Installed,
    Initializing,
    Enabled,
    Running,
    Stopping,
    Stopped,
    Error,
    Disabled
}