import React, { useState } from 'react';
import { ConversationListProps } from '../types';
import '../styles/components/ConversationList.css';

/**
 * 对话列表组件 - Apple HIG设计
 */
const ConversationList: React.FC<ConversationListProps> = ({
  conversations,
  currentConversationId,
  onSelectConversation,
  onCreateConversation,
  onDeleteConversation
}) => {
  const [isCreating, setIsCreating] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
      return date.toLocaleTimeString('zh-CN', {
        hour: '2-digit',
        minute: '2-digit'
      });
    } else if (days === 1) {
      return '昨天';
    } else if (days < 7) {
      return `${days}天前`;
    } else {
      return date.toLocaleDateString('zh-CN', {
        month: 'short',
        day: 'numeric'
      });
    }
  };

  const handleCreateConversation = async () => {
    if (isCreating) return;
    
    setIsCreating(true);
    try {
      await onCreateConversation();
    } finally {
      setIsCreating(false);
    }
  };

  const handleDeleteConversation = async (conversationId: string, event: React.MouseEvent) => {
    event.stopPropagation();
    
    if (deletingId === conversationId) return;
    
    setDeletingId(conversationId);
    try {
      await onDeleteConversation(conversationId);
    } finally {
      setDeletingId(null);
    }
  };

  const getModelIcon = (modelId: string) => {
    if (modelId.includes('claude')) return '🧠';
    if (modelId.includes('gpt')) return '🤖';
    return '💬';
  };

  return (
    <div className="conversation-list">
      {/* 列表头部 */}
      <div className="list-header">
        <h2 className="list-title">对话历史</h2>
        <button
          className={`create-button ${isCreating ? 'creating' : ''}`}
          onClick={handleCreateConversation}
          disabled={isCreating}
          aria-label="创建新对话"
        >
          {isCreating ? (
            <div className="loading-spinner"></div>
          ) : (
            <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
              <path
                d="M8 3V13M3 8H13"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
              />
            </svg>
          )}
        </button>
      </div>

      {/* 对话列表 */}
      <div className="conversations-container">
        {conversations.length === 0 ? (
          <div className="empty-list">
            <div className="empty-icon">📝</div>
            <div className="empty-text">暂无对话记录</div>
            <div className="empty-hint">点击 + 开始新对话</div>
          </div>
        ) : (
          <div className="conversation-items">
            {conversations.map((conversation) => (
              <div
                key={conversation.id}
                className={`conversation-item ${
                  conversation.id === currentConversationId ? 'active' : ''
                } ${deletingId === conversation.id ? 'deleting' : ''}`}
                onClick={() => onSelectConversation(conversation.id)}
              >
                <div className="conversation-content">
                  <div className="conversation-header">
                    <div className="conversation-title-row">
                      <span className="model-icon">
                        {getModelIcon(conversation.modelId)}
                      </span>
                      <h3 className="conversation-title">
                        {conversation.title || '新对话'}
                      </h3>
                      <div className="conversation-time">
                        {formatDate(conversation.updatedAt)}
                      </div>
                    </div>
                    
                    <div className="conversation-meta">
                      <div className="model-info">
                        <span className="model-name">
                          {conversation.modelName}
                        </span>
                        <span className="message-count">
                          {conversation.messageCount} 条消息
                        </span>
                      </div>
                    </div>
                  </div>

                  {conversation.lastMessagePreview && (
                    <div className="message-preview">
                      {conversation.lastMessagePreview}
                    </div>
                  )}
                </div>

                <div className="conversation-actions">
                  <button
                    className={`delete-button ${
                      deletingId === conversation.id ? 'deleting' : ''
                    }`}
                    onClick={(e) => handleDeleteConversation(conversation.id, e)}
                    disabled={deletingId === conversation.id}
                    aria-label="删除对话"
                  >
                    {deletingId === conversation.id ? (
                      <div className="loading-spinner small"></div>
                    ) : (
                      <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                        <path
                          d="M10.5 3.5L3.5 10.5M3.5 3.5L10.5 10.5"
                          stroke="currentColor"
                          strokeWidth="1.5"
                          strokeLinecap="round"
                        />
                      </svg>
                    )}
                  </button>
                </div>

                {/* 活跃指示器 */}
                {conversation.id === currentConversationId && (
                  <div className="active-indicator"></div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* 列表底部统计 */}
      {conversations.length > 0 && (
        <div className="list-footer">
          <div className="list-stats">
            <span className="total-conversations">
              共 {conversations.length} 个对话
            </span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConversationList;