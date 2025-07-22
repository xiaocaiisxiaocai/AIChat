using System.Text.Json.Serialization;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件清单 - 定义插件的元数据和配置信息
/// </summary>
public class PluginManifest
{
    /// <summary>
    /// 插件唯一标识符
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 插件显示名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 插件版本号
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 插件描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 插件作者
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 插件类型
    /// </summary>
    [JsonPropertyName("type")]
    public PluginType Type { get; set; }

    /// <summary>
    /// 插件状态
    /// </summary>
    [JsonPropertyName("status")]
    public PluginStatus Status { get; set; } = PluginStatus.Disabled;

    /// <summary>
    /// 插件主程序集文件名
    /// </summary>
    [JsonPropertyName("assembly")]
    public string Assembly { get; set; } = string.Empty;

    /// <summary>
    /// 插件入口类的完整类名
    /// </summary>
    [JsonPropertyName("entryPoint")]
    public string EntryPoint { get; set; } = string.Empty;

    /// <summary>
    /// 插件依赖的其他插件ID列表
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// 插件所需的最低系统版本
    /// </summary>
    [JsonPropertyName("minSystemVersion")]
    public string MinSystemVersion { get; set; } = string.Empty;

    /// <summary>
    /// 插件配置参数定义
    /// </summary>
    [JsonPropertyName("configSchema")]
    public Dictionary<string, object> ConfigSchema { get; set; } = new();

    /// <summary>
    /// 插件权限要求
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// 插件提供的API端点
    /// </summary>
    [JsonPropertyName("apiEndpoints")]
    public List<PluginApiEndpoint> ApiEndpoints { get; set; } = new();

    /// <summary>
    /// 插件UI挂载点定义
    /// </summary>
    [JsonPropertyName("uiMountPoints")]
    public List<PluginUIMountPoint> UIMountPoints { get; set; } = new();

    /// <summary>
    /// 插件图标文件路径（相对于插件目录）
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 插件是否支持热重载
    /// </summary>
    [JsonPropertyName("supportsHotReload")]
    public bool SupportsHotReload { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 插件API端点定义
/// </summary>
public class PluginApiEndpoint
{
    /// <summary>
    /// API路径
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// HTTP方法
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// API描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 所需权限
    /// </summary>
    [JsonPropertyName("requiredPermissions")]
    public List<string> RequiredPermissions { get; set; } = new();
}

/// <summary>
/// 插件UI挂载点定义
/// </summary>
public class PluginUIMountPoint
{
    /// <summary>
    /// 挂载点ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 挂载点位置（如：chatInput、messageList、sidebar等）
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// 组件名称
    /// </summary>
    [JsonPropertyName("component")]
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// 优先级（数字越小优先级越高）
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否默认显示
    /// </summary>
    [JsonPropertyName("defaultVisible")]
    public bool DefaultVisible { get; set; } = true;
}