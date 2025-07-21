import * as signalR from '@microsoft/signalr';
import { StreamingChatResponse, ConnectionState } from '../types';

/**
 * SignalR连接服务 - 处理实时通信
 */
class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private connectionState: ConnectionState = 'Disconnected';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 3000;

  /**
   * 初始化SignalR连接
   */
  async initialize(hubUrl: string = 'http://localhost:5000/chathub'): Promise<void> {
    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
          logMessageContent: process.env.NODE_ENV === 'development',
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectDelay;
            }
            return null; // 停止重连
          }
        })
        .configureLogging(process.env.NODE_ENV === 'development' 
          ? signalR.LogLevel.Information 
          : signalR.LogLevel.Warning)
        .build();

      this.setupConnectionEvents();
      await this.connect();
    } catch (error) {
      // SignalR初始化失败错误已记录
      throw error;
    }
  }

  /**
   * 建立连接
   */
  private async connect(): Promise<void> {
    if (!this.connection) {
      throw new Error('SignalR连接未初始化');
    }

    try {
      this.connectionState = 'Connecting';
      await this.connection.start();
      this.connectionState = 'Connected';
      this.reconnectAttempts = 0;
      // SignalR连接成功
    } catch (error) {
      this.connectionState = 'Disconnected';
      // SignalR连接失败错误已记录
      throw error;
    }
  }

  /**
   * 设置连接事件监听
   */
  private setupConnectionEvents(): void {
    if (!this.connection) return;

    this.connection.onclose(async (_error) => {
      this.connectionState = 'Disconnected';
      // SignalR连接已关闭
      
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        await this.attemptReconnect();
      }
    });

    this.connection.onreconnecting((_error) => {
      this.connectionState = 'Reconnecting';
      // SignalR正在重连
    });

    this.connection.onreconnected((_connectionId) => {
      this.connectionState = 'Connected';
      this.reconnectAttempts = 0;
      // SignalR重连成功
    });
  }

  /**
   * 尝试重连
   */
  private async attemptReconnect(): Promise<void> {
    this.reconnectAttempts++;
    // SignalR尝试重连
    
    await new Promise(resolve => setTimeout(resolve, this.reconnectDelay));
    
    try {
      await this.connect();
    } catch (error) {
      // SignalR重连失败错误已记录
      
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        await this.attemptReconnect();
      } else {
        // SignalR达到最大重连次数
      }
    }
  }

  /**
   * 发送流式消息
   */
  async sendStreamingMessage(
    request: any,
    onResponse: (_response: StreamingChatResponse) => void,
    onError: (_error: string) => void
  ): Promise<void> {
    if (!this.connection || this.connectionState !== 'Connected') {
      onError('SignalR连接未建立');
      return;
    }

    try {
      // 监听流式响应
      this.connection.off('StreamingResponse');
      this.connection.on('StreamingResponse', onResponse);

      // 监听错误
      this.connection.off('StreamingError');
      this.connection.on('StreamingError', onError);

      // 发送消息
      await this.connection.invoke('SendStreamingMessage', request);
    } catch (error) {
      // SignalR发送消息失败错误已记录
      onError(error instanceof Error ? error.message : '发送消息失败');
    }
  }

  /**
   * 加入对话房间
   */
  async joinConversation(conversationId: string): Promise<void> {
    if (!this.connection || this.connectionState !== 'Connected') {
      throw new Error('SignalR连接未建立');
    }

    try {
      await this.connection.invoke('JoinConversation', conversationId);
      // 加入对话房间成功
    } catch (error) {
      // 加入对话房间失败错误已记录
      throw error;
    }
  }

  /**
   * 离开对话房间
   */
  async leaveConversation(conversationId: string): Promise<void> {
    if (!this.connection || this.connectionState !== 'Connected') {
      return;
    }

    try {
      await this.connection.invoke('LeaveConversation', conversationId);
      // 离开对话房间成功
    } catch (error) {
      // 离开对话房间失败错误已记录
    }
  }

  /**
   * 监听对话更新
   */
  onConversationUpdate(callback: (_data: any) => void): void {
    if (!this.connection) return;
    
    this.connection.off('ConversationUpdate');
    this.connection.on('ConversationUpdate', callback);
  }

  /**
   * 断开连接
   */
  async disconnect(): Promise<void> {
    if (!this.connection) return;

    try {
      this.connectionState = 'Disconnecting';
      await this.connection.stop();
      this.connectionState = 'Disconnected';
      // SignalR连接已断开
    } catch (error) {
      // 断开连接失败错误已记录
    }
  }

  /**
   * 获取连接状态
   */
  getConnectionState(): ConnectionState {
    return this.connectionState;
  }

  /**
   * 获取连接ID
   */
  getConnectionId(): string | null {
    return this.connection?.connectionId || null;
  }

  /**
   * 检查连接是否活跃
   */
  isConnected(): boolean {
    return this.connectionState === 'Connected';
  }
}

// 创建单例实例
export const signalRService = new SignalRService();
export default signalRService;