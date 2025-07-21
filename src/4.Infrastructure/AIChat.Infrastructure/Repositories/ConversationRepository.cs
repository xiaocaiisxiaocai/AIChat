using AIChat.Domain.Entities;
using AIChat.Domain.Repositories;
using AIChat.Infrastructure.Data;
using SqlSugar;

namespace AIChat.Infrastructure.Repositories;

/// <summary>
/// 对话仓储实现 - 使用SqlSugar实现数据访问
/// </summary>
public class ConversationRepository : IConversationRepository
{
    private readonly DatabaseContext _dbContext;

    public ConversationRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 创建新对话
    /// </summary>
    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        var result = await _dbContext.Conversations.InsertReturnEntityAsync(conversation);
        return result;
    }

    /// <summary>
    /// 根据ID获取对话(包含消息)
    /// </summary>
    public async Task<Conversation?> GetByIdAsync(string id)
    {
        var conversation = await _dbContext.Conversations.GetByIdAsync(id);
        if (conversation != null)
        {
            // 加载对话的消息
            conversation.Messages = await _dbContext.Messages
                .AsQueryable()
                .Where(m => m.ConversationId == id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
        return conversation;
    }

    /// <summary>
    /// 获取对话列表(分页，按更新时间倒序)
    /// </summary>
    public async Task<List<Conversation>> GetListAsync(int pageIndex = 1, int pageSize = 20)
    {
        return await _dbContext.Conversations
            .AsQueryable()
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// 更新对话
    /// </summary>
    public async Task<Conversation> UpdateAsync(Conversation conversation)
    {
        conversation.UpdatedAt = DateTime.UtcNow;
        await _dbContext.Conversations.UpdateAsync(conversation);
        return conversation;
    }

    /// <summary>
    /// 删除对话
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        // 先删除对话的所有消息
        await _dbContext.Messages.DeleteAsync(m => m.ConversationId == id);
        
        // 再删除对话
        var result = await _dbContext.Conversations.DeleteByIdAsync(id);
        return result;
    }

    /// <summary>
    /// 获取对话总数
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await Task.FromResult(_dbContext.Conversations.Count(c => true));
    }

    /// <summary>
    /// 根据模型ID获取对话列表
    /// </summary>
    public async Task<List<Conversation>> GetByModelIdAsync(string modelId)
    {
        return await _dbContext.Conversations
            .AsQueryable()
            .Where(c => c.ModelId == modelId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }
}