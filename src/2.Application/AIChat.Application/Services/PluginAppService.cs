using AIChat.Domain.Repositories;
using AIChat.Shared.Plugins;
using Microsoft.Extensions.Logging;

namespace AIChat.Application.Services;

/// <summary>
/// 插件应用服务 - 编排插件业务流程
/// </summary>
public class PluginAppService
{
    private readonly IPluginRepository _pluginRepository;
    private readonly PluginManager _pluginManager;
    private readonly ILogger<PluginAppService> _logger;

    public PluginAppService(
        IPluginRepository pluginRepository,
        PluginManager pluginManager,
        ILogger<PluginAppService> logger)
    {
        _pluginRepository = pluginRepository;
        _pluginManager = pluginManager;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有已安装的插件
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetAllInstalledPluginsAsync()
    {
        _logger.LogInformation("正在获取所有已安装的插件");
        return await _pluginRepository.GetAllInstalledPluginsAsync();
    }

    /// <summary>
    /// 根据ID获取插件详情
    /// </summary>
    public async Task<PluginManifest?> GetPluginByIdAsync(string pluginId)
    {
        _logger.LogInformation("正在获取插件详情，ID: {PluginId}", pluginId);
        return await _pluginRepository.GetPluginByIdAsync(pluginId);
    }

    /// <summary>
    /// 安装插件
    /// </summary>
    public async Task<bool> InstallPluginAsync(PluginManifest manifest)
    {
        try
        {
            _logger.LogInformation("开始安装插件: {PluginName} (ID: {PluginId})", manifest.Name, manifest.Id);

            // 检查插件是否已存在
            var existingPlugin = await _pluginRepository.GetPluginByIdAsync(manifest.Id);
            if (existingPlugin != null)
            {
                _logger.LogWarning("插件已存在，无法重复安装: {PluginId}", manifest.Id);
                return false;
            }

            // 验证插件依赖
            if (!await ValidatePluginDependenciesAsync(manifest))
            {
                _logger.LogError("插件依赖验证失败: {PluginId}", manifest.Id);
                return false;
            }

            // 设置插件状态为已安装但未启用
            manifest.Status = PluginStatus.Installed;
            manifest.CreatedAt = DateTime.UtcNow;
            manifest.UpdatedAt = DateTime.UtcNow;

            // 保存到数据库
            await _pluginRepository.SavePluginAsync(manifest);

            // 加载插件到内存
            await _pluginManager.LoadPluginAsync(manifest.Id);

            _logger.LogInformation("插件安装成功: {PluginName}", manifest.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "安装插件失败: {PluginId}", manifest.Id);
            return false;
        }
    }

    /// <summary>
    /// 启用插件
    /// </summary>
    public async Task<bool> EnablePluginAsync(string pluginId)
    {
        try
        {
            _logger.LogInformation("正在启用插件: {PluginId}", pluginId);

            var plugin = await _pluginRepository.GetPluginByIdAsync(pluginId);
            if (plugin == null)
            {
                _logger.LogWarning("插件不存在: {PluginId}", pluginId);
                return false;
            }

            // 验证插件依赖是否已启用
            if (!await ValidateEnabledDependenciesAsync(plugin))
            {
                _logger.LogError("插件依赖未满足: {PluginId}", pluginId);
                return false;
            }

            // 启用插件
            await _pluginManager.EnablePluginAsync(pluginId);

            // 更新数据库状态
            await _pluginRepository.UpdatePluginStatusAsync(pluginId, PluginStatus.Enabled);

            _logger.LogInformation("插件启用成功: {PluginId}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启用插件失败: {PluginId}", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 禁用插件
    /// </summary>
    public async Task<bool> DisablePluginAsync(string pluginId)
    {
        try
        {
            _logger.LogInformation("正在禁用插件: {PluginId}", pluginId);

            // 检查是否有其他插件依赖此插件
            var dependentPlugins = await GetDependentPluginsAsync(pluginId);
            if (dependentPlugins.Any())
            {
                _logger.LogWarning("无法禁用插件，存在依赖此插件的其他插件: {PluginId}", pluginId);
                return false;
            }

            // 禁用插件
            await _pluginManager.DisablePluginAsync(pluginId);

            // 更新数据库状态
            await _pluginRepository.UpdatePluginStatusAsync(pluginId, PluginStatus.Disabled);

            _logger.LogInformation("插件禁用成功: {PluginId}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "禁用插件失败: {PluginId}", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 卸载插件
    /// </summary>
    public async Task<bool> UninstallPluginAsync(string pluginId)
    {
        try
        {
            _logger.LogInformation("正在卸载插件: {PluginId}", pluginId);

            // 先禁用插件
            await DisablePluginAsync(pluginId);

            // 卸载插件
            await _pluginManager.UnloadPluginAsync(pluginId);

            // 从数据库删除
            await _pluginRepository.DeletePluginAsync(pluginId);

            _logger.LogInformation("插件卸载成功: {PluginId}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卸载插件失败: {PluginId}", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 根据类型获取插件列表
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetPluginsByTypeAsync(PluginType pluginType)
    {
        _logger.LogInformation("正在获取类型为 {PluginType} 的插件列表", pluginType);
        return await _pluginRepository.GetPluginsByTypeAsync(pluginType);
    }

    /// <summary>
    /// 根据状态获取插件列表
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetPluginsByStatusAsync(PluginStatus status)
    {
        _logger.LogInformation("正在获取状态为 {PluginStatus} 的插件列表", status);
        return await _pluginRepository.GetPluginsByStatusAsync(status);
    }

    /// <summary>
    /// 重载插件
    /// </summary>
    public async Task<bool> ReloadPluginAsync(string pluginId)
    {
        try
        {
            _logger.LogInformation("正在重载插件: {PluginId}", pluginId);

            var plugin = await _pluginRepository.GetPluginByIdAsync(pluginId);
            if (plugin == null)
            {
                _logger.LogWarning("插件不存在: {PluginId}", pluginId);
                return false;
            }

            // 重载插件
            await _pluginManager.ReloadPluginAsync(pluginId);

            _logger.LogInformation("插件重载成功: {PluginId}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重载插件失败: {PluginId}", pluginId);
            return false;
        }
    }

    /// <summary>
    /// 验证插件依赖是否满足
    /// </summary>
    private async Task<bool> ValidatePluginDependenciesAsync(PluginManifest manifest)
    {
        foreach (var dependencyId in manifest.Dependencies)
        {
            var dependency = await _pluginRepository.GetPluginByIdAsync(dependencyId);
            if (dependency == null)
            {
                _logger.LogError("依赖插件不存在: {DependencyId}", dependencyId);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 验证插件的已启用依赖
    /// </summary>
    private async Task<bool> ValidateEnabledDependenciesAsync(PluginManifest manifest)
    {
        foreach (var dependencyId in manifest.Dependencies)
        {
            var dependency = await _pluginRepository.GetPluginByIdAsync(dependencyId);
            if (dependency == null || dependency.Status != PluginStatus.Enabled)
            {
                _logger.LogError("依赖插件未启用: {DependencyId}", dependencyId);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取依赖指定插件的其他插件
    /// </summary>
    private async Task<IEnumerable<PluginManifest>> GetDependentPluginsAsync(string pluginId)
    {
        var allPlugins = await _pluginRepository.GetAllInstalledPluginsAsync();
        return allPlugins.Where(p => p.Dependencies.Contains(pluginId) && p.Status == PluginStatus.Enabled);
    }
}