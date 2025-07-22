import axios, { AxiosResponse } from 'axios';
import {
  ChatRequest,
  ChatResponse,
  Conversation,
  AIModel,
  CreateConversationRequest,
  UpdateConversationRequest,
  ApiError,
  PluginManifest,
  PluginInstallRequest
} from '../types';

// API 基础配置
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// 请求拦截器
apiClient.interceptors.request.use(
  (config) => {
    // API请求日志
    return config;
  },
  (error) => {
    // API请求错误已记录
    return Promise.reject(error);
  }
);

// 响应拦截器
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    const currentModel = response.headers['x-current-model'];
    if (currentModel) {
      // 当前模型日志
    }
    return response;
  },
  (error) => {
    // API响应错误已记录
    
    const apiError: ApiError = {
      error: error.response?.data?.error || 'API请求失败',
      message: error.response?.data?.message || error.message,
      statusCode: error.response?.status
    };
    
    return Promise.reject(apiError);
  }
);

/**
 * 聊天相关API
 */
export const chatApi = {
  /**
   * 发送聊天消息
   */
  sendMessage: async (request: ChatRequest): Promise<ChatResponse> => {
    const response = await apiClient.post<ChatResponse>('/chat/message', request);
    return response.data;
  },

  /**
   * 发送流式聊天消息
   */
  sendStreamingMessage: async (
    request: ChatRequest,
    onChunk: (_chunk: string) => void,
    onError: (_error: string) => void
  ): Promise<void> => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/chat/message/stream`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const reader = response.body?.getReader();
      if (!reader) {
        throw new Error('无法读取响应流');
      }

      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        
        if (done) {
          break;
        }

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (line.startsWith('data: ')) {
            try {
              const data = JSON.parse(line.slice(6));
              onChunk(JSON.stringify(data));
            } catch (e) {
              // SSE数据解析失败
            }
          }
        }
      }
    } catch (error) {
      // 流式请求失败错误已记录
      onError(error instanceof Error ? error.message : '未知错误');
    }
  },

  /**
   * 获取对话列表
   */
  getConversations: async (pageIndex = 1, pageSize = 20): Promise<Conversation[]> => {
    const response = await apiClient.get<Conversation[]>('/chat/conversations', {
      params: { pageIndex, pageSize }
    });
    return response.data;
  },

  /**
   * 获取对话详情
   */
  getConversation: async (id: string): Promise<Conversation> => {
    const response = await apiClient.get<Conversation>(`/chat/conversations/${id}`);
    return response.data;
  },

  /**
   * 创建新对话
   */
  createConversation: async (request: CreateConversationRequest): Promise<Conversation> => {
    const response = await apiClient.post<Conversation>('/chat/conversations', request);
    return response.data;
  },

  /**
   * 更新对话标题
   */
  updateConversation: async (id: string, request: UpdateConversationRequest): Promise<Conversation> => {
    const response = await apiClient.put<Conversation>(`/chat/conversations/${id}`, request);
    return response.data;
  },

  /**
   * 删除对话
   */
  deleteConversation: async (id: string): Promise<void> => {
    await apiClient.delete(`/chat/conversations/${id}`);
  },

  /**
   * 获取可用AI模型列表
   */
  getAvailableModels: async (): Promise<AIModel[]> => {
    const response = await apiClient.get<AIModel[]>('/chat/models');
    return response.data;
  },
};

/**
 * 插件管理API
 */
export const pluginApi = {
  /**
   * 获取已安装的插件列表
   */
  getInstalledPlugins: async (): Promise<PluginManifest[]> => {
    const response = await apiClient.get<PluginManifest[]>('/plugins');
    return response.data;
  },

  /**
   * 获取已启用的插件列表
   */
  getEnabledPlugins: async (): Promise<PluginManifest[]> => {
    const response = await apiClient.get<PluginManifest[]>('/plugins/enabled');
    return response.data;
  },

  /**
   * 根据ID获取插件详情
   */
  getPlugin: async (pluginId: string): Promise<PluginManifest> => {
    const response = await apiClient.get<PluginManifest>(`/plugins/${pluginId}`);
    return response.data;
  },

  /**
   * 安装新插件
   */
  installPlugin: async (plugin: PluginInstallRequest): Promise<PluginManifest> => {
    const response = await apiClient.post<PluginManifest>('/plugins/install', plugin);
    return response.data;
  },

  /**
   * 启用插件
   */
  enablePlugin: async (pluginId: string): Promise<void> => {
    await apiClient.post(`/plugins/${pluginId}/enable`);
  },

  /**
   * 禁用插件
   */
  disablePlugin: async (pluginId: string): Promise<void> => {
    await apiClient.post(`/plugins/${pluginId}/disable`);
  },

  /**
   * 卸载插件
   */
  uninstallPlugin: async (pluginId: string): Promise<void> => {
    await apiClient.delete(`/plugins/${pluginId}`);
  },

  /**
   * 重新加载插件
   */
  reloadPlugin: async (pluginId: string): Promise<PluginManifest> => {
    const response = await apiClient.post<PluginManifest>(`/plugins/${pluginId}/reload`);
    return response.data;
  },

  /**
   * 获取插件统计信息
   */
  getPluginStats: async (): Promise<{
    totalInstalled: number;
    totalEnabled: number;
    totalDisabled: number;
    totalErrors: number;
    byType: Record<string, number>;
  }> => {
    const response = await apiClient.get('/plugins/stats');
    return response.data;
  },
};

/**
 * 健康检查API
 */
export const healthApi = {
  /**
   * 检查API健康状态
   */
  checkHealth: async (): Promise<{ status: string; timestamp: string; version: string }> => {
    const response = await apiClient.get('/health');
    return response.data;
  },
};

/**
 * 错误处理工具函数
 */
export const handleApiError = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const apiError = error as { response?: { data?: ApiError } };
    return apiError.response?.data?.message || '网络请求失败';
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return '未知错误';
};

export default apiClient;