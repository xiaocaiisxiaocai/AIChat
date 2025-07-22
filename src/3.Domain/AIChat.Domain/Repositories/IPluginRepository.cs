using AIChat.Shared.Plugins;

namespace AIChat.Domain.Repositories;

/// <summary>
/// 插件仓储接口 - 负责插件数据的持久化操作
/// </summary>
public interface IPluginRepository
{
    /// <summary>
    /// 获取所有已安装的插件
    /// </summary>
    /// <returns>插件清单列表</returns>
    Task<IEnumerable<PluginManifest>> GetAllInstalledPluginsAsync();
    
    /// <summary>
    /// 根据ID获取插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>插件清单，如果不存在则返回null</returns>
    Task<PluginManifest?> GetPluginByIdAsync(string pluginId);
    
    /// <summary>
    /// 保存插件清单
    /// </summary>
    /// <param name="manifest">插件清单</param>
    /// <returns></returns>
    Task SavePluginAsync(PluginManifest manifest);
    
    /// <summary>
    /// 更新插件状态
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="status">新状态</param>
    /// <returns></returns>
    Task UpdatePluginStatusAsync(string pluginId, PluginStatus status);
    
    /// <summary>
    /// 删除插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    Task DeletePluginAsync(string pluginId);
    
    /// <summary>
    /// 检查插件是否存在
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns></returns>
    Task<bool> PluginExistsAsync(string pluginId);
    
    /// <summary>
    /// 根据类型获取插件列表
    /// </summary>
    /// <param name="pluginType">插件类型</param>
    /// <returns>指定类型的插件列表</returns>
    Task<IEnumerable<PluginManifest>> GetPluginsByTypeAsync(PluginType pluginType);
    
    /// <summary>
    /// 根据状态获取插件列表
    /// </summary>
    /// <param name="status">插件状态</param>
    /// <returns>指定状态的插件列表</returns>
    Task<IEnumerable<PluginManifest>> GetPluginsByStatusAsync(PluginStatus status);
}