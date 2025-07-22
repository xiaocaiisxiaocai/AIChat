using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Collections.Concurrent;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件管理器 - 负责插件的加载、卸载、生命周期管理
/// </summary>
public class PluginManager : IDisposable
{
    private readonly ILogger<PluginManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, LoadedPlugin> _loadedPlugins;
    private readonly ConcurrentDictionary<string, PluginManifest> _pluginManifests;
    private readonly List<string> _pluginDirectories;
    private bool _disposed = false;

    public PluginManager(ILogger<PluginManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _loadedPlugins = new ConcurrentDictionary<string, LoadedPlugin>();
        _pluginManifests = new ConcurrentDictionary<string, PluginManifest>();
        _pluginDirectories = new List<string>();
    }

    /// <summary>
    /// 已加载的插件数量
    /// </summary>
    public int LoadedPluginsCount => _loadedPlugins.Count;

    /// <summary>
    /// 获取所有插件清单
    /// </summary>
    public IReadOnlyDictionary<string, PluginManifest> PluginManifests => _pluginManifests.AsReadOnly();

    /// <summary>
    /// 添加插件搜索目录
    /// </summary>
    public void AddPluginDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("插件目录路径不能为空", nameof(directory));

        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("插件目录不存在: {Directory}", directory);
            return;
        }

        if (!_pluginDirectories.Contains(directory))
        {
            _pluginDirectories.Add(directory);
            _logger.LogInformation("已添加插件搜索目录: {Directory}", directory);
        }
    }

    /// <summary>
    /// 扫描并发现所有插件
    /// </summary>
    public async Task<List<PluginManifest>> DiscoverPluginsAsync()
    {
        var discoveredManifests = new List<PluginManifest>();

        foreach (var directory in _pluginDirectories)
        {
            try
            {
                var manifests = await ScanDirectoryForPluginsAsync(directory);
                discoveredManifests.AddRange(manifests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫描插件目录时发生错误: {Directory}", directory);
            }
        }

        // 更新插件清单缓存
        foreach (var manifest in discoveredManifests)
        {
            _pluginManifests.AddOrUpdate(manifest.Id, manifest, (_, _) => manifest);
        }

        _logger.LogInformation("插件发现完成，共发现 {Count} 个插件", discoveredManifests.Count);
        return discoveredManifests;
    }

    /// <summary>
    /// 加载指定插件
    /// </summary>
    public async Task<bool> LoadPluginAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("插件ID不能为空", nameof(pluginId));

        if (_loadedPlugins.ContainsKey(pluginId))
        {
            _logger.LogWarning("插件 {PluginId} 已经加载", pluginId);
            return true;
        }

        if (!_pluginManifests.TryGetValue(pluginId, out var manifest))
        {
            _logger.LogError("找不到插件清单: {PluginId}", pluginId);
            return false;
        }

        try
        {
            var loadedPlugin = await LoadPluginFromManifestAsync(manifest);
            if (loadedPlugin != null)
            {
                _loadedPlugins.TryAdd(pluginId, loadedPlugin);
                _logger.LogInformation("插件 {PluginId} 加载成功", pluginId);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载插件 {PluginId} 时发生错误", pluginId);
        }

        return false;
    }

    /// <summary>
    /// 启用指定插件
    /// </summary>
    public async Task<bool> EnablePluginAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("插件ID不能为空", nameof(pluginId));

        if (!_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
        {
            _logger.LogError("插件未加载，无法启用: {PluginId}", pluginId);
            return false;
        }

        try
        {
            if (!loadedPlugin.Plugin.IsEnabled)
            {
                await loadedPlugin.Plugin.StartAsync();
                loadedPlugin.Plugin.IsEnabled = true;
                _logger.LogInformation("插件 {PluginId} 已启用", pluginId);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启用插件 {PluginId} 时发生错误", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 禁用指定插件
    /// </summary>
    public async Task<bool> DisablePluginAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("插件ID不能为空", nameof(pluginId));

        if (!_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
        {
            _logger.LogError("插件未加载，无法禁用: {PluginId}", pluginId);
            return false;
        }

        try
        {
            if (loadedPlugin.Plugin.IsEnabled)
            {
                await loadedPlugin.Plugin.StopAsync();
                loadedPlugin.Plugin.IsEnabled = false;
                _logger.LogInformation("插件 {PluginId} 已禁用", pluginId);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "禁用插件 {PluginId} 时发生错误", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 卸载指定插件
    /// </summary>
    public async Task<bool> UnloadPluginAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("插件ID不能为空", nameof(pluginId));

        if (!_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
        {
            _logger.LogWarning("插件未加载: {PluginId}", pluginId);
            return true;
        }

        try
        {
            // 先停止插件
            if (loadedPlugin.Plugin.IsEnabled)
            {
                await loadedPlugin.Plugin.StopAsync();
            }

            // 销毁插件
            await loadedPlugin.Plugin.DisposeAsync();

            // 从加载列表中移除
            _loadedPlugins.TryRemove(pluginId, out _);

            _logger.LogInformation("插件 {PluginId} 已卸载", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卸载插件 {PluginId} 时发生错误", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 重载指定插件
    /// </summary>
    public async Task<bool> ReloadPluginAsync(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("插件ID不能为空", nameof(pluginId));

        try
        {
            // 先卸载插件
            var wasEnabled = false;
            if (_loadedPlugins.TryGetValue(pluginId, out var existingPlugin))
            {
                wasEnabled = existingPlugin.Plugin.IsEnabled;
                await UnloadPluginAsync(pluginId);
            }

            // 重新加载插件
            var loadResult = await LoadPluginAsync(pluginId);
            if (!loadResult)
            {
                _logger.LogError("重载插件失败: {PluginId}", pluginId);
                return false;
            }

            // 如果之前是启用状态，重新启用
            if (wasEnabled)
            {
                await EnablePluginAsync(pluginId);
            }

            _logger.LogInformation("插件 {PluginId} 重载完成", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重载插件 {PluginId} 时发生错误", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 获取指定插件实例
    /// </summary>
    public IPlugin? GetPlugin(string pluginId)
    {
        return _loadedPlugins.TryGetValue(pluginId, out var loadedPlugin) 
            ? loadedPlugin.Plugin 
            : null;
    }

    /// <summary>
    /// 获取指定类型的所有插件
    /// </summary>
    public List<IPlugin> GetPluginsByType(PluginType pluginType)
    {
        return _loadedPlugins.Values
            .Where(p => p.Manifest.Type == pluginType)
            .Select(p => p.Plugin)
            .ToList();
    }

    /// <summary>
    /// 扫描目录查找插件
    /// </summary>
    private async Task<List<PluginManifest>> ScanDirectoryForPluginsAsync(string directory)
    {
        var manifests = new List<PluginManifest>();
        var manifestFiles = Directory.GetFiles(directory, "plugin.json", SearchOption.AllDirectories);

        foreach (var manifestFile in manifestFiles)
        {
            try
            {
                var manifestContent = await File.ReadAllTextAsync(manifestFile);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestContent);
                
                if (manifest != null && ValidateManifest(manifest, Path.GetDirectoryName(manifestFile)!))
                {
                    manifests.Add(manifest);
                    _logger.LogDebug("发现插件清单: {PluginId} 在 {Path}", manifest.Id, manifestFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析插件清单失败: {ManifestFile}", manifestFile);
            }
        }

        return manifests;
    }

    /// <summary>
    /// 验证插件清单
    /// </summary>
    private bool ValidateManifest(PluginManifest manifest, string pluginDirectory)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id) ||
            string.IsNullOrWhiteSpace(manifest.Name) ||
            string.IsNullOrWhiteSpace(manifest.Assembly) ||
            string.IsNullOrWhiteSpace(manifest.EntryPoint))
        {
            _logger.LogWarning("插件清单字段不完整: {PluginId}", manifest.Id);
            return false;
        }

        var assemblyPath = Path.Combine(pluginDirectory, manifest.Assembly);
        if (!File.Exists(assemblyPath))
        {
            _logger.LogWarning("插件程序集文件不存在: {AssemblyPath}", assemblyPath);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从清单加载插件
    /// </summary>
    private async Task<LoadedPlugin?> LoadPluginFromManifestAsync(PluginManifest manifest)
    {
        var pluginDirectory = GetPluginDirectory(manifest.Id);
        if (pluginDirectory == null)
        {
            _logger.LogError("找不到插件目录: {PluginId}", manifest.Id);
            return null;
        }

        var assemblyPath = Path.Combine(pluginDirectory, manifest.Assembly);
        var assembly = Assembly.LoadFrom(assemblyPath);

        // 查找并实例化插件入口点
        var entryType = assembly.GetType(manifest.EntryPoint);
        if (entryType == null)
        {
            _logger.LogError("找不到插件入口点类型: {EntryPoint}", manifest.EntryPoint);
            return null;
        }

        if (!typeof(IPlugin).IsAssignableFrom(entryType))
        {
            _logger.LogError("插件入口点类型必须实现IPlugin接口: {EntryPoint}", manifest.EntryPoint);
            return null;
        }

        var pluginInstance = Activator.CreateInstance(entryType) as IPlugin;
        if (pluginInstance == null)
        {
            _logger.LogError("无法创建插件实例: {EntryPoint}", manifest.EntryPoint);
            return null;
        }

        // 创建插件上下文
        var context = ActivatorUtilities.CreateInstance<PluginContext>(_serviceProvider, manifest);

        // 初始化插件
        await pluginInstance.InitializeAsync(context);

        return new LoadedPlugin
        {
            Plugin = pluginInstance,
            Manifest = manifest,
            Context = context,
            LoadedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 获取插件目录路径
    /// </summary>
    private string? GetPluginDirectory(string pluginId)
    {
        foreach (var directory in _pluginDirectories)
        {
            var manifestFile = Path.Combine(directory, pluginId, "plugin.json");
            if (File.Exists(manifestFile))
            {
                return Path.GetDirectoryName(manifestFile);
            }
        }
        return null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // 卸载所有插件
            foreach (var pluginId in _loadedPlugins.Keys.ToList())
            {
                var loadedPlugin = _loadedPlugins[pluginId];
                loadedPlugin.Plugin.StopAsync().GetAwaiter().GetResult();
            }
            _loadedPlugins.Clear();
            _disposed = true;
        }
    }
}

/// <summary>
/// 已加载的插件信息
/// </summary>
internal class LoadedPlugin
{
    public IPlugin Plugin { get; init; } = null!;
    public PluginManifest Manifest { get; init; } = null!;
    public IPluginContext Context { get; init; } = null!;
    public DateTime LoadedAt { get; init; }
}