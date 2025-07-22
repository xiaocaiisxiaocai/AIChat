using AIChat.Domain.Repositories;
using AIChat.Infrastructure.Data;
using AIChat.Shared.Plugins;
using SqlSugar;

namespace AIChat.Infrastructure.Repositories;

/// <summary>
/// 插件仓储实现 - 使用SqlSugar实现插件数据持久化
/// </summary>
public class PluginRepository : IPluginRepository
{
    private readonly DatabaseContext _dbContext;

    public PluginRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取所有已安装的插件
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetAllInstalledPluginsAsync()
    {
        return await _dbContext.Plugins
            .AsQueryable()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取插件
    /// </summary>
    public async Task<PluginManifest?> GetPluginByIdAsync(string pluginId)
    {
        return await _dbContext.Plugins.GetByIdAsync(pluginId);
    }

    /// <summary>
    /// 保存插件清单
    /// </summary>
    public async Task SavePluginAsync(PluginManifest manifest)
    {
        var existingPlugin = await _dbContext.Plugins.GetByIdAsync(manifest.Id);
        
        if (existingPlugin != null)
        {
            // 更新现有插件
            manifest.UpdatedAt = DateTime.UtcNow;
            await _dbContext.Plugins.UpdateAsync(manifest);
        }
        else
        {
            // 创建新插件
            manifest.CreatedAt = DateTime.UtcNow;
            manifest.UpdatedAt = DateTime.UtcNow;
            await _dbContext.Plugins.InsertAsync(manifest);
        }
    }

    /// <summary>
    /// 更新插件状态
    /// </summary>
    public async Task UpdatePluginStatusAsync(string pluginId, PluginStatus status)
    {
        var plugin = await _dbContext.Plugins.GetByIdAsync(pluginId);
        if (plugin != null)
        {
            plugin.Status = status;
            plugin.UpdatedAt = DateTime.UtcNow;
            await _dbContext.Plugins.UpdateAsync(plugin);
        }
    }

    /// <summary>
    /// 删除插件
    /// </summary>
    public async Task DeletePluginAsync(string pluginId)
    {
        await _dbContext.Plugins.DeleteByIdAsync(pluginId);
    }

    /// <summary>
    /// 检查插件是否存在
    /// </summary>
    public async Task<bool> PluginExistsAsync(string pluginId)
    {
        var plugin = await _dbContext.Plugins.GetByIdAsync(pluginId);
        return plugin != null;
    }

    /// <summary>
    /// 根据类型获取插件列表
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetPluginsByTypeAsync(PluginType pluginType)
    {
        return await _dbContext.Plugins
            .AsQueryable()
            .Where(p => p.Type == pluginType)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根据状态获取插件列表
    /// </summary>
    public async Task<IEnumerable<PluginManifest>> GetPluginsByStatusAsync(PluginStatus status)
    {
        return await _dbContext.Plugins
            .AsQueryable()
            .Where(p => p.Status == status)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}