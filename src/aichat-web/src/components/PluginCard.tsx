import React from 'react';
import { PluginManifest, PluginStatus } from '../types';
import { pluginApi } from '../services/apiService';

interface PluginCardProps {
  plugin: PluginManifest;
  onStatusChange?: (_pluginId: string, _newStatus: PluginStatus) => void;
  onUninstall?: (_pluginId: string) => void;
}

/**
 * 插件卡片组件 - Apple HIG风格设计
 * 展示插件基本信息并提供操作按钮
 */
export const PluginCard: React.FC<PluginCardProps> = ({
  plugin,
  onStatusChange,
  onUninstall,
}) => {
  const [isLoading, setIsLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const getStatusColor = (status: PluginStatus): string => {
    switch (status) {
      case PluginStatus.Enabled:
        return '#34C759'; // Apple绿色
      case PluginStatus.Disabled:
        return '#FF9500'; // Apple橙色
      case PluginStatus.Error:
        return '#FF3B30'; // Apple红色
      case PluginStatus.Loading:
        return '#007AFF'; // Apple蓝色
      default:
        return '#8E8E93'; // Apple灰色
    }
  };

  const getStatusText = (status: PluginStatus): string => {
    switch (status) {
      case PluginStatus.Enabled:
        return '已启用';
      case PluginStatus.Disabled:
        return '已禁用';
      case PluginStatus.Error:
        return '错误';
      case PluginStatus.Loading:
        return '加载中';
      default:
        return '未知';
    }
  };

  const getTypeText = (type: string): string => {
    const typeMap: Record<string, string> = {
      'AIModel': 'AI模型',
      'UIComponent': 'UI组件',
      'MessageProcessor': '消息处理',
      'Storage': '存储',
      'Notification': '通知',
      'Tool': '工具',
    };
    return typeMap[type] || type;
  };

  const handleToggleStatus = async () => {
    if (isLoading) return;

    setIsLoading(true);
    setError(null);

    try {
      if (plugin.status === PluginStatus.Enabled) {
        await pluginApi.disablePlugin(plugin.id);
        onStatusChange?.(plugin.id, PluginStatus.Disabled);
      } else if (plugin.status === PluginStatus.Disabled) {
        await pluginApi.enablePlugin(plugin.id);
        onStatusChange?.(plugin.id, PluginStatus.Enabled);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : '操作失败');
      console.error('插件状态切换失败:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleReload = async () => {
    if (isLoading) return;

    setIsLoading(true);
    setError(null);

    try {
      await pluginApi.reloadPlugin(plugin.id);
      onStatusChange?.(plugin.id, PluginStatus.Loading);
    } catch (err) {
      setError(err instanceof Error ? err.message : '重载失败');
      console.error('插件重载失败:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleUninstall = async () => {
    if (isLoading) return;

    if (!window.confirm(`确定要卸载插件 "${plugin.name}" 吗？此操作不可撤销。`)) {
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await pluginApi.uninstallPlugin(plugin.id);
      onUninstall?.(plugin.id);
    } catch (err) {
      setError(err instanceof Error ? err.message : '卸载失败');
      console.error('插件卸载失败:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const cardStyle: React.CSSProperties = {
    background: '#FFFFFF',
    border: '1px solid #E5E5E7',
    borderRadius: '12px',
    padding: '16px',
    marginBottom: '12px',
    boxShadow: '0 1px 3px rgba(0, 0, 0, 0.1)',
    transition: 'all 0.2s ease',
  };

  const headerStyle: React.CSSProperties = {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: '12px',
  };

  const infoStyle: React.CSSProperties = {
    flex: 1,
    marginRight: '16px',
  };

  const nameStyle: React.CSSProperties = {
    fontSize: '18px',
    fontWeight: 600,
    color: '#000000',
    margin: '0 0 4px 0',
  };

  const descriptionStyle: React.CSSProperties = {
    fontSize: '14px',
    color: '#6C6C70',
    margin: '0 0 8px 0',
    lineHeight: 1.4,
  };

  const metadataStyle: React.CSSProperties = {
    display: 'flex',
    gap: '12px',
    fontSize: '12px',
    color: '#8E8E93',
  };

  const tagStyle: React.CSSProperties = {
    background: '#F2F2F7',
    padding: '2px 6px',
    borderRadius: '4px',
  };

  const statusStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: '6px',
  };

  const indicatorStyle: React.CSSProperties = {
    width: '8px',
    height: '8px',
    borderRadius: '50%',
    backgroundColor: getStatusColor(plugin.status),
  };

  const statusTextStyle: React.CSSProperties = {
    fontSize: '12px',
    fontWeight: 500,
    color: '#3C3C43',
  };

  const errorStyle: React.CSSProperties = {
    background: 'rgba(255, 59, 48, 0.1)',
    border: '1px solid rgba(255, 59, 48, 0.2)',
    borderRadius: '8px',
    padding: '8px 12px',
    marginBottom: '12px',
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  };

  const errorMessageStyle: React.CSSProperties = {
    fontSize: '12px',
    color: '#FF3B30',
  };

  const actionsStyle: React.CSSProperties = {
    display: 'flex',
    gap: '8px',
  };

  const buttonBaseStyle: React.CSSProperties = {
    padding: '8px 16px',
    borderRadius: '8px',
    border: 'none',
    fontSize: '14px',
    fontWeight: 500,
    cursor: 'pointer',
    transition: 'all 0.2s ease',
    flex: 1,
    opacity: isLoading ? 0.6 : 1,
  };

  const primaryButtonStyle: React.CSSProperties = {
    ...buttonBaseStyle,
    background: '#007AFF',
    color: '#FFFFFF',
  };

  const secondaryButtonStyle: React.CSSProperties = {
    ...buttonBaseStyle,
    background: '#F2F2F7',
    color: '#007AFF',
  };

  const dangerButtonStyle: React.CSSProperties = {
    ...buttonBaseStyle,
    background: '#FF3B30',
    color: '#FFFFFF',
  };

  return (
    <div style={cardStyle}>
      <div style={headerStyle}>
        <div style={infoStyle}>
          <h3 style={nameStyle}>{plugin.name}</h3>
          <p style={descriptionStyle}>{plugin.description}</p>
          <div style={metadataStyle}>
            <span style={tagStyle}>v{plugin.version}</span>
            <span style={tagStyle}>by {plugin.author}</span>
            <span style={tagStyle}>{getTypeText(plugin.type)}</span>
          </div>
        </div>
        <div style={statusStyle}>
          <div style={indicatorStyle} />
          <span style={statusTextStyle}>{getStatusText(plugin.status)}</span>
        </div>
      </div>

      {error && (
        <div style={errorStyle}>
          <span style={{ fontSize: '14px' }}>⚠️</span>
          <span style={errorMessageStyle}>{error}</span>
        </div>
      )}

      <div style={actionsStyle}>
        <button
          style={primaryButtonStyle}
          onClick={handleToggleStatus}
          disabled={isLoading || plugin.status === PluginStatus.Error}
        >
          {isLoading ? '处理中...' : plugin.status === PluginStatus.Enabled ? '禁用' : '启用'}
        </button>

        <button
          style={secondaryButtonStyle}
          onClick={handleReload}
          disabled={isLoading}
        >
          重载
        </button>

        <button
          style={dangerButtonStyle}
          onClick={handleUninstall}
          disabled={isLoading}
        >
          卸载
        </button>
      </div>
    </div>
  );
};