using AIChat.Domain.Entities;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace AIChat.Infrastructure.Data;

/// <summary>
/// 数据库上下文 - 使用SqlSugar ORM
/// </summary>
public class DatabaseContext
{
    private readonly IConfiguration _configuration;
    private readonly SqlSugarClient _db;

    public DatabaseContext(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var connectionString = _configuration.GetConnectionString("DefaultConnection") 
                              ?? "Data Source=aichat.db";

        _db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        // 开发环境下打印SQL
        _db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine($"[SQL] {sql}");
            if (pars != null && pars.Any())
            {
                Console.WriteLine($"[Parameters] {string.Join(", ", pars.Select(p => $"{p.ParameterName}={p.Value}"))}");
            }
        };

        // 初始化数据库表
        InitializeTables();
    }

    /// <summary>
    /// 获取SqlSugar客户端
    /// </summary>
    public SqlSugarClient Database => _db;

    /// <summary>
    /// 对话表操作
    /// </summary>
    public SimpleClient<Conversation> Conversations => new(_db);

    /// <summary>
    /// 消息表操作
    /// </summary>
    public SimpleClient<Message> Messages => new(_db);

    /// <summary>
    /// 初始化数据库表结构
    /// </summary>
    private void InitializeTables()
    {
        try
        {
            // 创建表结构
            _db.CodeFirst.InitTables<Conversation>();
            _db.CodeFirst.InitTables<Message>();

            Console.WriteLine("[Database] Tables initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Database] Error initializing tables: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _db?.Dispose();
    }
}