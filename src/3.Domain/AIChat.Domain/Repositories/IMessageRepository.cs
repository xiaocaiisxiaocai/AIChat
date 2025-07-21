using AIChat.Domain.Entities;

namespace AIChat.Domain.Repositories;

/// <summary>
/// 消息仓储接口 - 定义消息数据访问操作
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// 创建新消息
    /// </summary>
    Task<Message> CreateAsync(Message message);

    /// <summary>
    /// 根据ID获取消息
    /// </summary>
    Task<Message?> GetByIdAsync(string id);

    /// <summary>
    /// 根据对话ID获取消息列表
    /// </summary>
    Task<List<Message>> GetByConversationIdAsync(string conversationId);

    /// <summary>
    /// 更新消息
    /// </summary>
    Task<Message> UpdateAsync(Message message);

    /// <summary>
    /// 删除消息
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 删除对话的所有消息
    /// </summary>
    Task<bool> DeleteByConversationIdAsync(string conversationId);

    /// <summary>
    /// 获取最近的消息列表
    /// </summary>
    Task<List<Message>> GetRecentAsync(int count = 10);
}