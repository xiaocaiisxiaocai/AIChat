using AIChat.Domain.Entities;

namespace AIChat.Domain.Repositories;

/// <summary>
/// 对话仓储接口 - 定义对话数据访问操作
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// 创建新对话
    /// </summary>
    Task<Conversation> CreateAsync(Conversation conversation);

    /// <summary>
    /// 根据ID获取对话
    /// </summary>
    Task<Conversation?> GetByIdAsync(string id);

    /// <summary>
    /// 获取对话列表(分页)
    /// </summary>
    Task<List<Conversation>> GetListAsync(int pageIndex = 1, int pageSize = 20);

    /// <summary>
    /// 更新对话
    /// </summary>
    Task<Conversation> UpdateAsync(Conversation conversation);

    /// <summary>
    /// 删除对话
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 获取对话总数
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// 根据模型ID获取对话列表
    /// </summary>
    Task<List<Conversation>> GetByModelIdAsync(string modelId);
}