using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 简化的插件上下文实现 - 用于最小依赖注册
/// </summary>
public class SimplePluginContext : IPluginContext
{
    public SimplePluginContext(
        IServiceProvider serviceProvider,
        ILogger<SimplePluginContext> logger,
        IConfiguration configuration)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // 创建简单的事件总线和存储实现
        EventBus = new PluginEventBus();
        Storage = new PluginStorage();
    }

    public ILogger Logger { get; }
    public IConfiguration Configuration { get; }
    public IServiceProvider ServiceProvider { get; }
    public IPluginEventBus EventBus { get; }
    public IPluginStorage Storage { get; }

    public T GetPluginConfiguration<T>(string pluginId) where T : class, new()
    {
        var section = Configuration.GetSection($"Plugins:{pluginId}");
        var config = section.Get<T>();
        return config ?? new T();
    }

    public async Task SetPluginConfigurationAsync<T>(string pluginId, T configuration) where T : class
    {
        await Storage.SetAsync(pluginId, "Configuration", configuration);
        Logger.LogInformation("插件 {PluginId} 的配置已更新", pluginId);
    }

    public async Task PublishEventAsync<TEvent>(TEvent eventData) where TEvent : IPluginEvent
    {
        await EventBus.PublishAsync(eventData);
    }

    public void SubscribeEvent<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent
    {
        EventBus.Subscribe(handler);
    }

    public async Task<object?> CallPluginApiAsync(string pluginId, string apiName, object? parameters = null)
    {
        // 简化实现 - 在实际应用中需要更复杂的插件间通信机制
        Logger.LogWarning("CallPluginApiAsync 尚未完全实现: {PluginId}.{ApiName}", pluginId, apiName);
        await Task.CompletedTask;
        return null;
    }

    public void RegisterApi(string apiName, Func<object?, Task<object?>> handler)
    {
        Logger.LogInformation("注册插件API: {ApiName}", apiName);
    }

    public string GetPluginDataDirectory(string pluginId)
    {
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIChat",
            "Plugins",
            pluginId);

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        return dataDirectory;
    }

    public string GetPluginTempDirectory(string pluginId)
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "AIChat",
            "Plugins",
            pluginId);

        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        return tempDirectory;
    }
}