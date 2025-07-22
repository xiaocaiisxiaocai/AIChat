using Microsoft.AspNetCore.Mvc;
using AIChat.Application.Services;
using AIChat.Shared.Plugins;

namespace AIChat.Api.Controllers;

/// <summary>
/// 插件管理API控制器
/// 提供插件的安装、启用、禁用、卸载等管理功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PluginController : ControllerBase
{
    private readonly PluginAppService _pluginAppService;
    private readonly ILogger<PluginController> _logger;

    public PluginController(PluginAppService pluginAppService, ILogger<PluginController> logger)
    {
        _pluginAppService = pluginAppService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有已安装的插件列表
    /// </summary>
    /// <returns>插件清单列表</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PluginManifest>>> GetInstalledPlugins()
    {
        try
        {
            var plugins = await _pluginAppService.GetAllInstalledPluginsAsync();
            return Ok(plugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取插件列表失败");
            return StatusCode(500, "获取插件列表失败");
        }
    }

    /// <summary>
    /// 获取特定类型的插件列表
    /// </summary>
    /// <param name="type">插件类型</param>
    /// <returns>插件清单列表</returns>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<PluginManifest>>> GetPluginsByType(PluginType type)
    {
        try
        {
            var plugins = await _pluginAppService.GetPluginsByTypeAsync(type);
            return Ok(plugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取插件列表失败，类型: {PluginType}", type);
            return StatusCode(500, "获取插件列表失败");
        }
    }

    /// <summary>
    /// 获取单个插件详情
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>插件清单</returns>
    [HttpGet("{pluginId}")]
    public async Task<ActionResult<PluginManifest>> GetPlugin(string pluginId)
    {
        try
        {
            var plugin = await _pluginAppService.GetPluginByIdAsync(pluginId);
            if (plugin == null)
            {
                return NotFound($"插件 {pluginId} 不存在");
            }
            return Ok(plugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取插件详情失败，插件ID: {PluginId}", pluginId);
            return StatusCode(500, "获取插件详情失败");
        }
    }

    /// <summary>
    /// 安装插件
    /// </summary>
    /// <param name="request">插件安装请求</param>
    /// <returns>安装结果</returns>
    [HttpPost("install")]
    public async Task<ActionResult<bool>> InstallPlugin([FromBody] InstallPluginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PluginPath))
            {
                return BadRequest("插件路径不能为空");
            }

            // 这里需要先从路径加载PluginManifest，暂时返回错误提示
            return BadRequest("插件安装功能需要先实现插件清单加载逻辑");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "插件安装失败: {PluginPath}", request.PluginPath);
            return StatusCode(500, "插件安装失败");
        }
    }

    /// <summary>
    /// 启用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>启用结果</returns>
    [HttpPost("{pluginId}/enable")]
    public async Task<ActionResult<bool>> EnablePlugin(string pluginId)
    {
        try
        {
            var result = await _pluginAppService.EnablePluginAsync(pluginId);
            if (result)
            {
                _logger.LogInformation("插件启用成功: {PluginId}", pluginId);
                return Ok(true);
            }
            else
            {
                return BadRequest($"插件 {pluginId} 启用失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "插件启用失败: {PluginId}", pluginId);
            return StatusCode(500, "插件启用失败");
        }
    }

    /// <summary>
    /// 禁用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>禁用结果</returns>
    [HttpPost("{pluginId}/disable")]
    public async Task<ActionResult<bool>> DisablePlugin(string pluginId)
    {
        try
        {
            var result = await _pluginAppService.DisablePluginAsync(pluginId);
            if (result)
            {
                _logger.LogInformation("插件禁用成功: {PluginId}", pluginId);
                return Ok(true);
            }
            else
            {
                return BadRequest($"插件 {pluginId} 禁用失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "插件禁用失败: {PluginId}", pluginId);
            return StatusCode(500, "插件禁用失败");
        }
    }

    /// <summary>
    /// 卸载插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>卸载结果</returns>
    [HttpDelete("{pluginId}")]
    public async Task<ActionResult<bool>> UninstallPlugin(string pluginId)
    {
        try
        {
            var result = await _pluginAppService.UninstallPluginAsync(pluginId);
            if (result)
            {
                _logger.LogInformation("插件卸载成功: {PluginId}", pluginId);
                return Ok(true);
            }
            else
            {
                return BadRequest($"插件 {pluginId} 卸载失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "插件卸载失败: {PluginId}", pluginId);
            return StatusCode(500, "插件卸载失败");
        }
    }

    /// <summary>
    /// 重载插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>重载结果</returns>
    [HttpPost("{pluginId}/reload")]
    public async Task<ActionResult<bool>> ReloadPlugin(string pluginId)
    {
        try
        {
            var result = await _pluginAppService.ReloadPluginAsync(pluginId);
            if (result)
            {
                _logger.LogInformation("插件重载成功: {PluginId}", pluginId);
                return Ok(true);
            }
            else
            {
                return BadRequest($"插件 {pluginId} 重载失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "插件重载失败: {PluginId}", pluginId);
            return StatusCode(500, "插件重载失败");
        }
    }

    /// <summary>
    /// 获取插件统计信息
    /// </summary>
    /// <returns>插件统计</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<PluginStatistics>> GetPluginStatistics()
    {
        try
        {
            var plugins = await _pluginAppService.GetAllInstalledPluginsAsync();
            
            var statistics = new PluginStatistics
            {
                TotalInstalled = plugins.Count(),
                EnabledCount = plugins.Count(p => p.Status == PluginStatus.Enabled),
                DisabledCount = plugins.Count(p => p.Status == PluginStatus.Disabled),
                PluginsByType = plugins.GroupBy(p => p.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取插件统计信息失败");
            return StatusCode(500, "获取插件统计信息失败");
        }
    }
}

/// <summary>
/// 插件安装请求
/// </summary>
public class InstallPluginRequest
{
    /// <summary>
    /// 插件文件路径或URL
    /// </summary>
    public string PluginPath { get; set; } = string.Empty;
}

/// <summary>
/// 插件统计信息
/// </summary>
public class PluginStatistics
{
    /// <summary>
    /// 总安装数量
    /// </summary>
    public int TotalInstalled { get; set; }

    /// <summary>
    /// 已启用数量
    /// </summary>
    public int EnabledCount { get; set; }

    /// <summary>
    /// 已禁用数量
    /// </summary>
    public int DisabledCount { get; set; }

    /// <summary>
    /// 按类型分组的插件数量
    /// </summary>
    public Dictionary<string, int> PluginsByType { get; set; } = new();
}