using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件上下文接口 - 为插件提供系统访问能力
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    ILogger Logger { get; }
    
    /// <summary>
    /// 配置访问器
    /// </summary>
    IConfiguration Configuration { get; }
    
    /// <summary>
    /// 服务容器
    /// </summary>
    IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    /// 事件总线
    /// </summary>
    IPluginEventBus EventBus { get; }
    
    /// <summary>
    /// 插件存储服务
    /// </summary>
    IPluginStorage Storage { get; }
    
    /// <summary>
    /// 获取插件配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    T GetPluginConfiguration<T>(string pluginId) where T : class, new();
    
    /// <summary>
    /// 设置插件配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="pluginId">插件ID</param>
    /// <param name="configuration">配置对象</param>
    /// <returns></returns>
    Task SetPluginConfigurationAsync<T>(string pluginId, T configuration) where T : class;
    
    /// <summary>
    /// 发布插件事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    Task PublishEventAsync<TEvent>(TEvent eventData) where TEvent : IPluginEvent;
    
    /// <summary>
    /// 订阅插件事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    void SubscribeEvent<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent;
    
    /// <summary>
    /// 调用其他插件的API
    /// </summary>
    /// <param name="pluginId">目标插件ID</param>
    /// <param name="apiName">API名称</param>
    /// <param name="parameters">参数</param>
    /// <returns></returns>
    Task<object?> CallPluginApiAsync(string pluginId, string apiName, object? parameters = null);
    
    /// <summary>
    /// 注册插件API
    /// </summary>
    /// <param name="apiName">API名称</param>
    /// <param name="handler">API处理器</param>
    void RegisterApi(string apiName, Func<object?, Task<object?>> handler);
    
    /// <summary>
    /// 获取插件数据目录
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    string GetPluginDataDirectory(string pluginId);
    
    /// <summary>
    /// 获取插件临时目录
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    string GetPluginTempDirectory(string pluginId);
}

/// <summary>
/// 插件事件接口
/// </summary>
public interface IPluginEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    string EventId { get; }
    
    /// <summary>
    /// 事件时间戳
    /// </summary>
    DateTime Timestamp { get; }
    
    /// <summary>
    /// 事件源插件ID
    /// </summary>
    string SourcePluginId { get; }
}

/// <summary>
/// 插件存储接口
/// </summary>
public interface IPluginStorage
{
    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    Task SetAsync(string pluginId, string key, object value);
    
    /// <summary>
    /// 获取数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="pluginId">插件ID</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string pluginId, string key);
    
    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    Task RemoveAsync(string pluginId, string key);
    
    /// <summary>
    /// 检查键是否存在
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    Task<bool> ExistsAsync(string pluginId, string key);
    
    /// <summary>
    /// 获取插件的所有键
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetKeysAsync(string pluginId);
    
    /// <summary>
    /// 清空插件数据
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    Task ClearAsync(string pluginId);
}

/// <summary>
/// 插件事件总线接口
/// </summary>
public interface IPluginEventBus
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent;
    
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IPluginEvent;
    
    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent;
}