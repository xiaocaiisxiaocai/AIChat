import React from 'react';
import { ThinkingBubbleProps } from '../types';
import '../styles/components/ThinkingBubble.css';

/**
 * 思考气泡组件 - 显示AI的思考过程 (Apple HIG设计)
 */
const ThinkingBubble: React.FC<ThinkingBubbleProps> = ({
  content,
  isComplete,
  isVisible
}) => {
  if (!isVisible) return null;

  return (
    <div className={`thinking-bubble ${isComplete ? 'complete' : 'active'}`}>
      <div className="thinking-header">
        <div className="thinking-icon">
          <div className="thinking-dots">
            <div className="dot"></div>
            <div className="dot"></div>
            <div className="dot"></div>
          </div>
        </div>
        <span className="thinking-label">AI正在思考...</span>
        {!isComplete && (
          <div className="typing-indicator">
            <div className="typing-dot"></div>
            <div className="typing-dot"></div>
            <div className="typing-dot"></div>
          </div>
        )}
      </div>
      
      {content && (
        <div className="thinking-content">
          <div className="thinking-text">
            {content}
          </div>
        </div>
      )}
      
      {isComplete && (
        <div className="thinking-footer">
          <span className="thinking-complete-label">思考完成</span>
        </div>
      )}
    </div>
  );
};

export default ThinkingBubble;