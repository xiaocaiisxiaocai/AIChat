import React, { useState } from 'react';
import { ModelSelectorProps } from '../types';
import '../styles/components/ModelSelector.css';

/**
 * AI模型选择器组件 - Apple HIG设计
 */
const ModelSelector: React.FC<ModelSelectorProps> = ({
  models,
  selectedModelId,
  onModelChange,
  disabled = false
}) => {
  const [isOpen, setIsOpen] = useState(false);
  
  const selectedModel = models.find(model => model.id === selectedModelId);
  
  const handleModelSelect = (modelId: string) => {
    onModelChange(modelId);
    setIsOpen(false);
  };
  
  const getModelIcon = (provider: string) => {
    switch (provider.toLowerCase()) {
      case 'openai':
        return '🤖';
      case 'anthropic':
        return '🧠';
      default:
        return '💬';
    }
  };
  
  const getProviderColor = (provider: string) => {
    switch (provider.toLowerCase()) {
      case 'openai':
        return '#10A37F';
      case 'anthropic':
        return '#FF6B35';
      default:
        return '#007AFF';
    }
  };

  return (
    <div className={`model-selector ${disabled ? 'disabled' : ''}`}>
      {/* 当前选中的模型显示 */}
      <button
        className={`model-selector-button ${isOpen ? 'open' : ''}`}
        onClick={() => !disabled && setIsOpen(!isOpen)}
        disabled={disabled}
        aria-label="选择AI模型"
        aria-expanded={isOpen}
      >
        <div className="selected-model">
          <div className="model-icon">
            {selectedModel ? getModelIcon(selectedModel.provider) : '💬'}
          </div>
          <div className="model-info">
            <div className="model-name">
              {selectedModel?.name || '选择模型'}
            </div>
            <div className="model-provider">
              {selectedModel?.provider || '未选择'}
            </div>
          </div>
          <div className="model-features">
            {selectedModel?.supportsThinking && (
              <div className="feature-badge thinking" title="支持思考过程">
                🤔
              </div>
            )}
            {selectedModel?.supportsStreaming && (
              <div className="feature-badge streaming" title="支持流式输出">
                ⚡
              </div>
            )}
          </div>
        </div>
        
        <div className={`dropdown-arrow ${isOpen ? 'open' : ''}`}>
          <svg width="12" height="12" viewBox="0 0 12 12" fill="none">
            <path
              d="M3 4.5L6 7.5L9 4.5"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </div>
      </button>
      
      {/* 下拉模型列表 */}
      {isOpen && !disabled && (
        <div className="model-dropdown">
          <div className="dropdown-header">
            <span className="dropdown-title">选择AI模型</span>
            <button
              className="close-button"
              onClick={() => setIsOpen(false)}
              aria-label="关闭"
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
          
          <div className="model-list">
            {models.map((model) => (
              <button
                key={model.id}
                className={`model-option ${model.id === selectedModelId ? 'selected' : ''} ${!model.isValid ? 'unavailable' : ''}`}
                onClick={() => model.isValid && handleModelSelect(model.id)}
                disabled={!model.isValid}
              >
                <div className="model-option-content">
                  <div className="model-icon">
                    {getModelIcon(model.provider)}
                  </div>
                  <div className="model-details">
                    <div className="model-name">{model.name}</div>
                    <div 
                      className="model-provider"
                      style={{ color: getProviderColor(model.provider) }}
                    >
                      {model.provider}
                    </div>
                  </div>
                  <div className="model-features">
                    {model.supportsThinking && (
                      <div className="feature-badge thinking" title="支持思考过程">
                        🤔
                      </div>
                    )}
                    {model.supportsStreaming && (
                      <div className="feature-badge streaming" title="支持流式输出">
                        ⚡
                      </div>
                    )}
                  </div>
                  {!model.isValid && (
                    <div className="unavailable-badge">
                      不可用
                    </div>
                  )}
                </div>
                
                {model.id === selectedModelId && (
                  <div className="selected-indicator">
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                      <path
                        d="M13.5 4.5L6 12L2.5 8.5"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  </div>
                )}
              </button>
            ))}
          </div>
          
          {models.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">📭</div>
              <div className="empty-text">暂无可用模型</div>
            </div>
          )}
        </div>
      )}
      
      {/* 遮罩层 */}
      {isOpen && (
        <div 
          className="model-selector-overlay"
          onClick={() => setIsOpen(false)}
        />
      )}
    </div>
  );
};

export default ModelSelector;