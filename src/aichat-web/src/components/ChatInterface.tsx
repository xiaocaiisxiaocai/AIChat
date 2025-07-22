import React, { useState, useRef, useEffect } from 'react';
import { ChatInterfaceProps, Message } from '../types';
import MessageBubble from './MessageBubble';
// ModelSelector 组件暂未使用
import '../styles/components/ChatInterface.css';

/**
 * 聊天界面主组件 - Apple HIG设计
 */
const ChatInterface: React.FC<ChatInterfaceProps> = ({
  conversation,
  selectedModel: _selectedModel,
  onSendMessage,
  isLoading
}) => {
  const [inputMessage, setInputMessage] = useState('');
  const [isStreaming] = useState(false);
  const [thinkingContent] = useState('');
  const [currentResponse] = useState('');
  
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);

  // 自动滚动到底部
  const scrollToBottom = () => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [conversation?.messages, thinkingContent, currentResponse]);

  // 自动调整输入框高度
  const adjustTextareaHeight = () => {
    if (inputRef.current) {
      inputRef.current.style.height = 'auto';
      inputRef.current.style.height = `${Math.min(inputRef.current.scrollHeight, 120)}px`;
    }
  };

  useEffect(() => {
    adjustTextareaHeight();
  }, [inputMessage]);

  const handleSendMessage = () => {
    if (!inputMessage.trim() || isLoading || isStreaming || !onSendMessage) return;

    onSendMessage(inputMessage.trim());
    setInputMessage('');

    // 重置输入框高度
    if (inputRef.current) {
      inputRef.current.style.height = 'auto';
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setInputMessage(e.target.value);
  };

  // 模拟流式响应和思考过程（实际应该从props或context获取）
  const isThinking = isStreaming && thinkingContent;
  const messages = conversation?.messages || [];

  return (
    <div className="chat-interface">
      {/* 聊天头部 */}
      <div className="chat-header">
        <div className="conversation-info">
          <h1 className="conversation-title">
            {conversation?.title || '新对话'}
          </h1>
          <div className="conversation-meta">
            <span className="message-count">
              {conversation?.messageCount || 0} 条消息
            </span>
            {conversation?.modelName && (
              <span className="current-model">
                当前模型: {conversation.modelName}
              </span>
            )}
          </div>
        </div>
        
        {/* 状态指示器 */}
        <div className="status-indicators">
          {isLoading && (
            <div className="status-indicator loading">
              <div className="loading-spinner"></div>
              <span>处理中...</span>
            </div>
          )}
          {isStreaming && (
            <div className="status-indicator streaming">
              <div className="streaming-dot"></div>
              <span>实时响应</span>
            </div>
          )}
        </div>
      </div>

      {/* 消息列表 */}
      <div className="messages-container" ref={messagesContainerRef}>
        <div className="messages-list">
          {messages.length === 0 ? (
            <div className="empty-conversation">
              <div className="empty-icon">💬</div>
              <h3 className="empty-title">开始新对话</h3>
              <p className="empty-description">
                选择一个AI模型，然后发送您的第一条消息
              </p>
            </div>
          ) : (
            messages.map((message: Message) => (
              <MessageBubble
                key={message.id}
                message={message}
                isThinking={false}
                thinkingContent=""
              />
            ))
          )}
          
          {/* 实时思考过程显示 */}
          {isThinking && (
            <div className="thinking-container">
              <MessageBubble
                message={{
                  id: 'thinking',
                  role: 'assistant',
                  content: currentResponse,
                  createdAt: new Date().toISOString(),
                  modelId: conversation?.modelId || '',
                  isUserMessage: false,
                  isAssistantMessage: true,
                  hasThinking: true,
                  thinkingContent: thinkingContent
                }}
                isThinking={true}
                thinkingContent={thinkingContent}
              />
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </div>
      </div>

      {/* 输入区域 */}
      <div className="chat-input-container">
        <div className="chat-input">
          <div className="input-wrapper">
            <textarea
              ref={inputRef}
              value={inputMessage}
              onChange={handleInputChange}
              onKeyPress={handleKeyPress}
              placeholder="输入您的消息..."
              className="message-input"
              disabled={isLoading || isStreaming}
              rows={1}
              maxLength={4000}
            />
            
            <div className="input-actions">
              <div className="input-meta">
                <span className="char-count">
                  {inputMessage.length}/4000
                </span>
              </div>
              
              <button
                onClick={handleSendMessage}
                disabled={!inputMessage.trim() || isLoading || isStreaming}
                className="send-button"
                aria-label="发送消息"
              >
                <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <path
                    d="M2.5 10L17.5 2.5L10 17.5L8.75 11.25L2.5 10Z"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    fill="currentColor"
                  />
                </svg>
              </button>
            </div>
          </div>
          
          {/* 输入提示 */}
          <div className="input-hint">
            <span className="hint-text">
              按 Enter 发送，Shift + Enter 换行
            </span>
          </div>
        </div>
      </div>

      {/* 快捷操作 */}
      <div className="quick-actions">
        <button className="quick-action" disabled={isLoading}>
          <span>📝</span>
          <span>总结对话</span>
        </button>
        <button className="quick-action" disabled={isLoading}>
          <span>🔄</span>
          <span>重新生成</span>
        </button>
        <button className="quick-action" disabled={isLoading}>
          <span>📋</span>
          <span>复制最后回复</span>
        </button>
        <button className="quick-action" disabled={isLoading}>
          <span>🗑️</span>
          <span>清空对话</span>
        </button>
      </div>
    </div>
  );
};

export default ChatInterface;