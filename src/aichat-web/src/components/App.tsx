import React, { useState, useEffect } from 'react';
import { AppState } from '../types';
import { chatApi, handleApiError } from '../services/apiService';
import ChatInterface from './ChatInterface';
import ConversationList from './ConversationList';
import ModelSelector from './ModelSelector';
import '../styles/globals.css';
import '../styles/components/App.css';

/**
 * 主应用组件 - Apple HIG设计
 */
const App: React.FC = () => {
  const [appState, setAppState] = useState<AppState>({
    currentConversation: null,
    conversations: [],
    availableModels: [],
    selectedModelId: '',
    isLoading: false,
    error: null
  });

  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  // 初始化应用
  useEffect(() => {
    initializeApp();
  }, []);

  const initializeApp = async () => {
    setAppState(prev => ({ ...prev, isLoading: true, error: null }));

    try {
      // 并行加载模型和对话列表
      const [models, conversations] = await Promise.all([
        chatApi.getAvailableModels(),
        chatApi.getConversations()
      ]);

      // 选择默认模型
      const defaultModelId = models.length > 0 ? models[0].id : '';

      setAppState(prev => ({
        ...prev,
        availableModels: models,
        conversations,
        selectedModelId: defaultModelId,
        isLoading: false
      }));

      // 初始化完成日志
    } catch (error) {
      const errorMessage = handleApiError(error);
      setAppState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false
      }));
      // 初始化失败错误已记录
    }
  };

  // 创建新对话
  const handleCreateConversation = async () => {
    try {
      const newConversation = await chatApi.createConversation({
        modelId: appState.selectedModelId || undefined
      });

      setAppState(prev => ({
        ...prev,
        conversations: [newConversation, ...prev.conversations],
        currentConversation: newConversation
      }));

      // 新对话已创建
    } catch (error) {
      const errorMessage = handleApiError(error);
      setAppState(prev => ({ ...prev, error: errorMessage }));
      // 创建对话失败错误已记录
    }
  };

  // 选择对话
  const handleSelectConversation = async (conversationId: string) => {
    if (appState.currentConversation?.id === conversationId) return;

    try {
      setAppState(prev => ({ ...prev, isLoading: true }));
      
      const conversation = await chatApi.getConversation(conversationId);
      
      setAppState(prev => ({
        ...prev,
        currentConversation: conversation,
        selectedModelId: conversation.modelId,
        isLoading: false
      }));

      // 对话选择完成
    } catch (error) {
      const errorMessage = handleApiError(error);
      setAppState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false
      }));
      // 获取对话失败错误已记录
    }
  };

  // 删除对话
  const handleDeleteConversation = async (conversationId: string) => {
    try {
      await chatApi.deleteConversation(conversationId);

      setAppState(prev => {
        const updatedConversations = prev.conversations.filter(c => c.id !== conversationId);
        const newCurrentConversation = prev.currentConversation?.id === conversationId 
          ? null 
          : prev.currentConversation;

        return {
          ...prev,
          conversations: updatedConversations,
          currentConversation: newCurrentConversation
        };
      });

      // 对话删除完成
    } catch (error) {
      const errorMessage = handleApiError(error);
      setAppState(prev => ({ ...prev, error: errorMessage }));
      // 删除对话失败错误已记录
    }
  };

  // 发送消息
  const handleSendMessage = async (message: string) => {
    if (!message.trim()) return;

    try {
      setAppState(prev => ({ ...prev, isLoading: true, error: null }));

      const response = await chatApi.sendMessage({
        message,
        conversationId: appState.currentConversation?.id,
        modelId: appState.selectedModelId,
        useStreaming: false
      });

      // 获取更新后的对话
      const updatedConversation = await chatApi.getConversation(response.conversationId);

      setAppState(prev => {
        const updatedConversations = prev.conversations.map(conv =>
          conv.id === response.conversationId ? updatedConversation : conv
        );

        // 如果是新对话，添加到列表
        if (!prev.conversations.find(c => c.id === response.conversationId)) {
          updatedConversations.unshift(updatedConversation);
        }

        return {
          ...prev,
          currentConversation: updatedConversation,
          conversations: updatedConversations,
          isLoading: false
        };
      });

      // 消息发送成功
    } catch (error) {
      const errorMessage = handleApiError(error);
      setAppState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false
      }));
      // 发送消息失败错误已记录
    }
  };

  // 切换模型
  const handleModelChange = (modelId: string) => {
    setAppState(prev => ({ ...prev, selectedModelId: modelId }));
    // 模型切换完成
  };

  // 切换侧边栏
  const toggleSidebar = () => {
    setSidebarCollapsed(!sidebarCollapsed);
  };

  // 清除错误
  const clearError = () => {
    setAppState(prev => ({ ...prev, error: null }));
  };

  return (
    <div className="app">
      {/* 侧边栏 */}
      <div className={`sidebar ${sidebarCollapsed ? 'collapsed' : ''}`}>
        <div className="sidebar-header">
          <div className="app-logo">
            <span className="logo-icon">🤖</span>
            <span className="logo-text">AIChat</span>
          </div>
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

        <div className="sidebar-content">
          {/* 模型选择器 */}
          <div className="model-selector-container">
            <ModelSelector
              models={appState.availableModels}
              selectedModelId={appState.selectedModelId}
              onModelChange={handleModelChange}
              disabled={appState.isLoading}
            />
          </div>

          {/* 对话列表 */}
          <ConversationList
            conversations={appState.conversations}
            currentConversationId={appState.currentConversation?.id}
            onSelectConversation={handleSelectConversation}
            onCreateConversation={handleCreateConversation}
            onDeleteConversation={handleDeleteConversation}
          />
        </div>
      </div>

      {/* 主内容区 */}
      <div className="main-content">
        {/* 错误提示 */}
        {appState.error && (
          <div className="error-banner">
            <div className="error-content">
              <span className="error-icon">⚠️</span>
              <span className="error-message">{appState.error}</span>
              <button
                className="error-close"
                onClick={clearError}
                aria-label="关闭错误提示"
              >
                <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                  <path
                    d="M10.5 3.5L3.5 10.5M3.5 3.5L10.5 10.5"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                  />
                </svg>
              </button>
            </div>
          </div>
        )}

        {/* 聊天界面 */}
        <ChatInterface
          conversation={appState.currentConversation}
          onSendMessage={handleSendMessage}
          isLoading={appState.isLoading}
        />
      </div>

      {/* 全局加载指示器 */}
      {appState.isLoading && (
        <div className="global-loading">
          <div className="loading-content">
            <div className="loading-spinner large"></div>
            <span className="loading-text">处理中...</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default App;