import React from 'react';
import { MessageBubbleProps } from '../types';
import ThinkingBubble from './ThinkingBubble';
import '../styles/components/MessageBubble.css';

/**
 * 消息气泡组件 - 显示用户和AI的消息 (Apple HIG设计)
 */
const MessageBubble: React.FC<MessageBubbleProps> = ({
  message,
  isThinking = false,
  thinkingContent = ''
}) => {
  const isUser = message.role === 'user';
  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('zh-CN', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className={`message-container ${isUser ? 'user' : 'assistant'}`}>
      {/* AI思考过程 (仅在AI消息前显示) */}
      {!isUser && (isThinking || message.hasThinking) && (
        <ThinkingBubble
          content={thinkingContent || message.thinkingContent || ''}
          isComplete={!isThinking}
          isVisible={true}
        />
      )}
      
      {/* 消息气泡 */}
      <div className={`message-bubble ${isUser ? 'user-bubble' : 'assistant-bubble'}`}>
        {/* 消息头部信息 */}
        <div className="message-header">
          <span className="message-role">
            {isUser ? '您' : (message.modelId || 'AI助手')}
          </span>
          <span className="message-time">
            {formatTime(message.createdAt)}
          </span>
        </div>
        
        {/* 消息内容 */}
        <div className="message-content">
          <div className="message-text">
            {message.content}
          </div>
          
          {/* AI消息的模型标识 */}
          {!isUser && message.modelId && (
            <div className="model-badge">
              <span className="model-name">{message.modelId}</span>
            </div>
          )}
        </div>
        
        {/* 消息状态指示器 */}
        <div className="message-status">
          {isUser ? (
            <div className="sent-indicator">
              <svg width="12" height="12" viewBox="0 0 12 12" fill="none">
                <path
                  d="M10.5 3L4.5 9L1.5 6"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
            </div>
          ) : (
            <div className="received-indicator">
              <div className="indicator-dot"></div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MessageBubble;