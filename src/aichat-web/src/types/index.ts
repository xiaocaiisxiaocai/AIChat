// API 响应类型定义

export interface ChatRequest {
  message: string;
  conversationId?: string;
  modelId?: string;
  useStreaming?: boolean;
}

export interface ChatResponse {
  conversationId: string;
  content: string;
  thinkingContent?: string;
  modelUsed: string;
  createdAt: string;
  userMessageId: string;
  assistantMessageId: string;
}

export interface StreamingChatResponse {
  type: string; // 'thinking' | 'content' | 'error'
  content: string;
  isComplete: boolean;
  conversationId: string;
  modelUsed: string;
}

export interface Conversation {
  id: string;
  title: string;
  modelId: string;
  modelName: string;
  createdAt: string;
  updatedAt: string;
  messageCount: number;
  lastMessagePreview: string;
  messages?: Message[];
}

export interface Message {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  thinkingContent?: string;
  modelId: string;
  createdAt: string;
  isUserMessage: boolean;
  isAssistantMessage: boolean;
  hasThinking: boolean;
}

export interface AIModel {
  id: string;
  name: string;
  provider: string;
  apiKey: string;
  supportsStreaming: boolean;
  supportsThinking: boolean;
  apiEndpoint?: string;
  maxTokens?: number;
  temperature?: number;
  isValid: boolean;
}

export interface CreateConversationRequest {
  title?: string;
  modelId?: string;
}

export interface UpdateConversationRequest {
  title: string;
}

// UI 状态类型定义

export interface AppState {
  currentConversation: Conversation | null;
  conversations: Conversation[];
  availableModels: AIModel[];
  selectedModelId: string;
  isLoading: boolean;
  error: string | null;
}

export interface ChatState {
  messages: Message[];
  isThinking: boolean;
  thinkingContent: string;
  isStreaming: boolean;
  currentResponse: string;
  error: string | null;
}

export interface ThinkingState {
  isThinking: boolean;
  thinkingContent: string;
  isThinkingComplete: boolean;
}

// API 错误类型
export interface ApiError {
  error: string;
  message: string;
  statusCode?: number;
}

// SignalR 连接状态
export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Disconnecting' | 'Reconnecting';

// 主题模式
export type ThemeMode = 'light' | 'dark' | 'auto';

// 组件属性类型
export interface ChatInterfaceProps {
  conversation: Conversation | null;
  onSendMessage: (_message: string) => void;
  isLoading: boolean;
}

export interface ConversationListProps {
  conversations: Conversation[];
  currentConversationId?: string;
  onSelectConversation: (_conversationId: string) => void;
  onCreateConversation: () => void;
  onDeleteConversation: (_conversationId: string) => void;
}

export interface ModelSelectorProps {
  models: AIModel[];
  selectedModelId: string;
  onModelChange: (_modelId: string) => void;
  disabled?: boolean;
}

export interface ThinkingBubbleProps {
  content: string;
  isComplete: boolean;
  isVisible: boolean;
}

export interface MessageBubbleProps {
  message: Message;
  isThinking?: boolean;
  thinkingContent?: string;
}

// 工具函数类型
export type FormatDateFunction = (_date: string | Date) => string;
export type GenerateIdFunction = () => string;
export type ValidateMessageFunction = (_message: string) => boolean;