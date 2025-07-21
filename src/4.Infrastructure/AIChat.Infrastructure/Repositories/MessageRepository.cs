using AIChat.Domain.Entities;
using AIChat.Domain.Repositories;
using AIChat.Infrastructure.Data;

namespace AIChat.Infrastructure.Repositories;

/// <summary>
/// 消息仓储实现 - 使用SqlSugar实现数据访问
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly DatabaseContext _dbContext;

    public MessageRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 创建新消息
    /// </summary>
    public async Task<Message> CreateAsync(Message message)
    {
        var result = await _dbContext.Messages.InsertReturnEntityAsync(message);
        return result;
    }

    /// <summary>
    /// 根据ID获取消息
    /// </summary>
    public async Task<Message?> GetByIdAsync(string id)
    {
        return await _dbContext.Messages.GetByIdAsync(id);
    }

    /// <summary>
    /// 根据对话ID获取消息列表(按创建时间排序)
    /// </summary>
    public async Task<List<Message>> GetByConversationIdAsync(string conversationId)
    {
        return await _dbContext.Messages
            .AsQueryable()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 更新消息
    /// </summary>
    public async Task<Message> UpdateAsync(Message message)
    {
        await _dbContext.Messages.UpdateAsync(message);
        return message;
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        return await _dbContext.Messages.DeleteByIdAsync(id);
    }

    /// <summary>
    /// 删除对话的所有消息
    /// </summary>
    public async Task<bool> DeleteByConversationIdAsync(string conversationId)
    {
        return await _dbContext.Messages.DeleteAsync(m => m.ConversationId == conversationId);
    }

    /// <summary>
    /// 获取最近的消息列表(跨对话)
    /// </summary>
    public async Task<List<Message>> GetRecentAsync(int count = 10)
    {
        return await _dbContext.Messages
            .AsQueryable()
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}