using AIChat.Application.DTOs;
using AIChat.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace AIChat.Api.Hubs;

/// <summary>
/// 聊天SignalR Hub - 处理实时聊天通信
/// </summary>
public class ChatHub : Hub
{
    private readonly ChatAppService _chatAppService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ChatAppService chatAppService, ILogger<ChatHub> logger)
    {
        _chatAppService = chatAppService;
        _logger = logger;
    }

    /// <summary>
    /// 发送流式聊天消息
    /// </summary>
    public async Task SendStreamingMessage(ChatRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                await Clients.Caller.SendAsync("StreamingError", "消息内容不能为空");
                return;
            }

            _logger.LogInformation("用户 {ConnectionId} 发送消息: {Message}", Context.ConnectionId, request.Message);

            await foreach (var chunk in _chatAppService.SendStreamingMessageAsync(request))
            {
                // 发送流式响应到客户端
                await Clients.Caller.SendAsync("StreamingResponse", chunk);

                // 输出当前使用的模型
                if (!string.IsNullOrEmpty(chunk.ModelUsed))
                {
                    _logger.LogInformation("[{DateTime}] 当前使用模型: {ModelId}", DateTime.Now, chunk.ModelUsed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理流式消息时发生错误 - ConnectionId: {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("StreamingError", ex.Message);
        }
    }

    /// <summary>
    /// 加入对话房间
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("用户 {ConnectionId} 加入对话 {ConversationId}", Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// 离开对话房间
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("用户 {ConnectionId} 离开对话 {ConversationId}", Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// 客户端连接时触发
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("用户连接 - ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 客户端断开连接时触发
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("用户断开连接 - ConnectionId: {ConnectionId}, 异常: {Exception}", 
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 向特定对话房间的所有用户广播消息
    /// </summary>
    public async Task BroadcastToConversation(string conversationId, object message)
    {
        await Clients.Group($"conversation_{conversationId}").SendAsync("ConversationUpdate", message);
    }
}