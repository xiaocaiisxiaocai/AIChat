import React, { useState, useEffect } from 'react';
import { PluginManifest, PluginStatus } from '../types';
import { pluginApi } from '../services/apiService';
import { PluginCard } from './PluginCard';

interface PluginManagerProps {
  onClose?: () => void;
}

/**
 * 插件管理器组件 - Apple HIG风格设计
 * 提供插件列表、搜索、安装、管理等功能
 */
export const PluginManager: React.FC<PluginManagerProps> = ({ onClose }) => {
  const [plugins, setPlugins] = useState<PluginManifest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState<string>('all');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  useEffect(() => {
    loadPlugins();
  }, []);

  const loadPlugins = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await pluginApi.getInstalledPlugins();
      setPlugins(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : '加载插件失败');
      console.error('加载插件失败:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePluginStatusChange = (pluginId: string, newStatus: PluginStatus) => {
    setPlugins(prev => 
      prev.map(plugin => 
        plugin.id === pluginId 
          ? { ...plugin, status: newStatus }
          : plugin
      )
    );
  };

  const handlePluginUninstall = (pluginId: string) => {
    setPlugins(prev => prev.filter(plugin => plugin.id !== pluginId));
  };

  const filteredPlugins = plugins.filter(plugin => {
    const matchesSearch = plugin.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         plugin.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         plugin.author.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesType = filterType === 'all' || plugin.type === filterType;
    const matchesStatus = filterStatus === 'all' || plugin.status === filterStatus;
    
    return matchesSearch && matchesType && matchesStatus;
  });

  const getPluginTypeOptions = () => {
    const types = Array.from(new Set(plugins.map(p => p.type)));
    return [
      { value: 'all', label: '全部类型' },
      ...types.map(type => ({
        value: type,
        label: getTypeText(type)
      }))
    ];
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

  const getStatusCount = (status: PluginStatus): number => {
    return plugins.filter(p => p.status === status).length;
  };

  // 样式定义
  const overlayStyle: React.CSSProperties = {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1000,
  };

  const modalStyle: React.CSSProperties = {
    backgroundColor: '#FFFFFF',
    borderRadius: '12px',
    width: '90%',
    maxWidth: '800px',
    maxHeight: '90%',
    overflow: 'hidden',
    boxShadow: '0 20px 40px rgba(0, 0, 0, 0.3)',
  };

  const headerStyle: React.CSSProperties = {
    padding: '20px 24px',
    borderBottom: '1px solid #E5E5E7',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  };

  const titleStyle: React.CSSProperties = {
    fontSize: '20px',
    fontWeight: 600,
    color: '#000000',
    margin: 0,
  };

  const closeButtonStyle: React.CSSProperties = {
    background: 'none',
    border: 'none',
    fontSize: '24px',
    cursor: 'pointer',
    color: '#8E8E93',
    padding: '4px',
    borderRadius: '6px',
    transition: 'background-color 0.2s ease',
  };

  const filtersStyle: React.CSSProperties = {
    padding: '16px 24px',
    borderBottom: '1px solid #E5E5E7',
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
  };

  const searchStyle: React.CSSProperties = {
    width: '100%',
    padding: '8px 12px',
    border: '1px solid #C7C7CC',
    borderRadius: '8px',
    fontSize: '16px',
    outline: 'none',
    transition: 'border-color 0.2s ease',
  };

  const filterRowStyle: React.CSSProperties = {
    display: 'flex',
    gap: '12px',
    alignItems: 'center',
  };

  const selectStyle: React.CSSProperties = {
    padding: '6px 10px',
    border: '1px solid #C7C7CC',
    borderRadius: '6px',
    fontSize: '14px',
    background: '#FFFFFF',
    cursor: 'pointer',
    outline: 'none',
  };

  const statsStyle: React.CSSProperties = {
    display: 'flex',
    gap: '16px',
    fontSize: '12px',
    color: '#8E8E93',
  };

  const statStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    gap: '4px',
  };

  const contentStyle: React.CSSProperties = {
    padding: '16px 24px',
    maxHeight: '500px',
    overflowY: 'auto',
  };

  const loadingStyle: React.CSSProperties = {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    padding: '40px',
    color: '#8E8E93',
  };

  const errorStyle: React.CSSProperties = {
    background: 'rgba(255, 59, 48, 0.1)',
    border: '1px solid rgba(255, 59, 48, 0.2)',
    borderRadius: '8px',
    padding: '12px 16px',
    margin: '16px 0',
    color: '#FF3B30',
    fontSize: '14px',
  };

  const emptyStyle: React.CSSProperties = {
    textAlign: 'center',
    padding: '40px 20px',
    color: '#8E8E93',
  };

  const emptyIconStyle: React.CSSProperties = {
    fontSize: '48px',
    marginBottom: '12px',
  };

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={modalStyle} onClick={(e) => e.stopPropagation()}>
        {/* 头部 */}
        <div style={headerStyle}>
          <h2 style={titleStyle}>插件管理器</h2>
          <button style={closeButtonStyle} onClick={onClose}>
            ×
          </button>
        </div>

        {/* 过滤器和搜索 */}
        <div style={filtersStyle}>
          <input
            type="text"
            placeholder="搜索插件名称、描述或作者..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={searchStyle}
          />
          
          <div style={filterRowStyle}>
            <select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
              style={selectStyle}
            >
              {getPluginTypeOptions().map(option => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              style={selectStyle}
            >
              <option value="all">全部状态</option>
              <option value={PluginStatus.Enabled}>已启用</option>
              <option value={PluginStatus.Disabled}>已禁用</option>
              <option value={PluginStatus.Error}>错误</option>
            </select>

            <div style={statsStyle}>
              <div style={statStyle}>
                <span>总计: {plugins.length}</span>
              </div>
              <div style={statStyle}>
                <span style={{ color: '#34C759' }}>●</span>
                <span>启用: {getStatusCount(PluginStatus.Enabled)}</span>
              </div>
              <div style={statStyle}>
                <span style={{ color: '#FF9500' }}>●</span>
                <span>禁用: {getStatusCount(PluginStatus.Disabled)}</span>
              </div>
              <div style={statStyle}>
                <span style={{ color: '#FF3B30' }}>●</span>
                <span>错误: {getStatusCount(PluginStatus.Error)}</span>
              </div>
            </div>
          </div>
        </div>

        {/* 内容区域 */}
        <div style={contentStyle}>
          {loading && (
            <div style={loadingStyle}>
              <span>正在加载插件...</span>
            </div>
          )}

          {error && (
            <div style={errorStyle}>
              ⚠️ {error}
              <button
                style={{
                  marginLeft: '12px',
                  background: 'none',
                  border: 'none',
                  color: '#FF3B30',
                  textDecoration: 'underline',
                  cursor: 'pointer',
                }}
                onClick={loadPlugins}
              >
                重试
              </button>
            </div>
          )}

          {!loading && !error && filteredPlugins.length === 0 && (
            <div style={emptyStyle}>
              <div style={emptyIconStyle}>🔌</div>
              <div style={{ fontSize: '16px', marginBottom: '8px' }}>
                {searchTerm || filterType !== 'all' || filterStatus !== 'all' 
                  ? '未找到匹配的插件' 
                  : '还没有安装任何插件'
                }
              </div>
              <div style={{ fontSize: '14px', color: '#AEAEB2' }}>
                {searchTerm || filterType !== 'all' || filterStatus !== 'all'
                  ? '尝试调整搜索条件或过滤器'
                  : '您可以从插件商店安装插件来扩展功能'
                }
              </div>
            </div>
          )}

          {!loading && !error && filteredPlugins.map(plugin => (
            <PluginCard
              key={plugin.id}
              plugin={plugin}
              onStatusChange={handlePluginStatusChange}
              onUninstall={handlePluginUninstall}
            />
          ))}
        </div>
      </div>
    </div>
  );
};