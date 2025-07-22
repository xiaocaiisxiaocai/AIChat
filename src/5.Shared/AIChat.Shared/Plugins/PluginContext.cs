using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件上下文实现 - 为插件提供系统服务访问能力
/// </summary>
public class PluginContext : IPluginContext, IDisposable
{
    private readonly PluginManifest _manifest;
    private readonly ConcurrentDictionary<string, Func<object?, Task<object?>>> _registeredApis;
    private bool _disposed = false;

    public PluginContext(
        IServiceProvider serviceProvider,
        ILogger logger,
        IConfiguration configuration,
        IPluginEventBus eventBus,
        IPluginStorage storage,
        PluginManifest manifest)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        
        _registeredApis = new ConcurrentDictionary<string, Func<object?, Task<object?>>>();
    }

    // IPluginContext 接口实现
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
        // 在实际实现中，这里应该将配置持久化到存储中
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
        var apiKey = $"{pluginId}:{apiName}";
        if (_registeredApis.TryGetValue(apiKey, out var handler))
        {
            return await handler(parameters);
        }
        
        throw new InvalidOperationException($"API '{apiName}' 在插件 '{pluginId}' 中未找到");
    }

    public void RegisterApi(string apiName, Func<object?, Task<object?>> handler)
    {
        var apiKey = $"{_manifest.Id}:{apiName}";
        _registeredApis.TryAdd(apiKey, handler);
        Logger.LogInformation("插件 {PluginId} 注册了API: {ApiName}", _manifest.Id, apiName);
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

    public void Dispose()
    {
        if (!_disposed)
        {
            Logger.LogInformation("正在释放插件上下文资源: {PluginId}", _manifest.Id);
            _registeredApis.Clear();
            _disposed = true;
        }
    }
}
