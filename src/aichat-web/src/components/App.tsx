import React, { useState, useEffect } from 'react';
import { chatApi } from '../services/apiService';
import ChatInterface from './ChatInterface';
import ConversationList from './ConversationList';
import ModelSelector from './ModelSelector';
import { PluginManager } from './PluginManager';
import '../styles/components/App.css';

/**
 * 主应用组件 - Apple HIG设计
 */
const App: React.FC = () => {
  const [models, setModels] = useState<any[]>([]);
  const [selectedModel, setSelectedModel] = useState<any | null>(null);
  const [conversations, setConversations] = useState<any[]>([]);
  const [currentConversation, setCurrentConversation] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [errorVisible, setErrorVisible] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [showPluginManager, setShowPluginManager] = useState(false);

  useEffect(() => {
    // 加载可用模型
    const loadModels = async () => {
      try {
        const availableModels = await chatApi.getAvailableModels();
        setModels(availableModels);
        
        if (availableModels.length > 0) {
          const defaultModel = availableModels.find(m => m.isDefault) || availableModels[0];
          setSelectedModel(defaultModel);
        }
      } catch (err) {
        setError('无法加载AI模型');
        setErrorVisible(true);
      }
    };

    // 加载对话列表
    const loadConversations = async () => {
      try {
        const conversationList = await chatApi.getConversations();
        setConversations(conversationList);
      } catch (err) {
        setError('无法加载对话列表');
        setErrorVisible(true);
      } finally {
        setLoading(false);
      }
    };

    loadModels();
    loadConversations();
  }, []);

  const handleSelectModel = (modelId: string) => {
    const model = models.find(m => m.id === modelId);
    if (model) {
      setSelectedModel(model);
    }
  };

  // 切换侧边栏
  const toggleSidebar = () => {
    setSidebarCollapsed(!sidebarCollapsed);
  };

  // 切换插件管理器
  const togglePluginManager = () => {
    setShowPluginManager(!showPluginManager);
  };

  const handleCreateConversation = async () => {
    if (!selectedModel) return;

    try {
      const newConversation = await chatApi.createConversation({
        title: '新对话',
        modelId: selectedModel.id
      });
      
      setConversations(prev => [newConversation, ...prev]);
      setCurrentConversation(newConversation);
    } catch (err) {
      setError('创建对话失败');
      setErrorVisible(true);
    }
  };

  const handleSelectConversation = async (conversation: any) => {
    try {
      const conversationDetails = await chatApi.getConversation(conversation.id);
      setCurrentConversation(conversationDetails);
    } catch (err) {
      setError('加载对话详情失败');
      setErrorVisible(true);
    }
  };

  const handleDeleteConversation = async (conversationId: string) => {
    try {
      await chatApi.deleteConversation(conversationId);
      setConversations(prev => prev.filter(c => c.id !== conversationId));

      if (currentConversation?.id === conversationId) {
        setCurrentConversation(null);
      }
    } catch (err) {
      setError('删除对话失败');
      setErrorVisible(true);
    }
  };

  const handleSendMessage = async (message: string) => {
    if (!selectedModel || !currentConversation) return;

    try {
      await chatApi.sendMessage({
        message,
        conversationId: currentConversation.id,
        modelId: selectedModel.id
      });

      // 重新加载对话详情以获取最新消息
      const updatedConversation = await chatApi.getConversation(currentConversation.id);
      setCurrentConversation(updatedConversation);

      // 更新对话列表中的对话信息
      setConversations(prev =>
        prev.map(c => c.id === currentConversation.id ? updatedConversation : c)
      );
    } catch (err) {
      setError('发送消息失败');
      setErrorVisible(true);
    }
  };

  // 错误提示自动消失
  useEffect(() => {
    if (error) {
      setErrorVisible(true);
      const timer = setTimeout(() => {
        setErrorVisible(false);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [error]);

  return (
    <div className="app">
      <div className={`sidebar ${sidebarCollapsed ? 'collapsed' : ''}`}>
        <div className="sidebar-header">
          <div className="app-logo">
            <span className="logo-icon">🤖</span>
            <span className="logo-text">AIChat</span>
          </div>
          <div className="header-actions">
            <button
              className="plugin-manager-button"
              onClick={togglePluginManager}
              aria-label="插件管理"
              title="插件管理"
            >
              <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                <path d="M8 1C8.55228 1 9 1.44772 9 2V3.5C9 4.05228 8.55228 4.5 8 4.5C7.44772 4.5 7 4.05228 7 3.5V2C7 1.44772 7.44772 1 8 1Z" fill="currentColor" />
                <path d="M11.5 8C11.5 8.55228 11.0523 9 10.5 9H9C8.44772 9 8 8.55228 8 8C8 7.44772 8.44772 7 9 7H10.5C11.0523 7 11.5 7.44772 11.5 8Z" fill="currentColor" />
                <path d="M8 11.5C8.55228 11.5 9 11.9477 9 12.5V14C9 14.5523 8.55228 15 8 15C7.44772 15 7 14.5523 7 14V12.5C7 11.9477 7.44772 11.5 8 11.5Z" fill="currentColor" />
                <path d="M4.5 8C4.5 8.55228 4.94772 9 5.5 9H7C7.55228 9 8 8.55228 8 8C8 7.44772 7.55228 7 7 7H5.5C4.94772 7 4.5 7.44772 4.5 8Z" fill="currentColor" />
                <circle cx="8" cy="8" r="2" fill="currentColor"/>
              </svg>
            </button>
            <button
              className="sidebar-toggle"
              onClick={toggleSidebar}
              aria-label={sidebarCollapsed ? '展开侧边栏' : '收起侧边栏'}
            >
              <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                <path
                  d={sidebarCollapsed ? "M6 4L10 8L6 12" : "M10 4L6 8L10 12"}
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </button>
          </div>
        </div>
        
        <div className="sidebar-content">
          <div className="model-selector-container">
            <ModelSelector 
              models={models}
              selectedModelId={selectedModel?.id}
              onModelChange={handleSelectModel}
              disabled={loading}
            />
          </div>
          
          <div className="conversations-wrapper">
            <ConversationList
              conversations={conversations}
              currentConversationId={currentConversation?.id}
              onSelectConversation={handleSelectConversation}
              onDeleteConversation={handleDeleteConversation}
              onCreateConversation={handleCreateConversation}
              loading={loading}
            />
          </div>
        </div>
      </div>
      
      <div className="main-content">
        {error && errorVisible && (
          <div className="error-toast" role="alert">
            <span className="error-icon">⚠️</span>
            <span className="error-message">{error}</span>
            <button className="error-close" onClick={() => setErrorVisible(false)}>
              ×
            </button>
          </div>
        )}
        
        <ChatInterface
          conversation={currentConversation}
          selectedModel={selectedModel}
          onSendMessage={handleSendMessage}
          isLoading={loading}
        />
      </div>

      {/* 插件管理器 */}
      {showPluginManager && (
        <PluginManager onClose={() => setShowPluginManager(false)} />
      )}
    </div>
  );
};

export default App;