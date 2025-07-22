using AIChat.Api.Hubs;
using AIChat.Api.Middleware;
using AIChat.Application.Services;
using AIChat.Domain.Repositories;
using AIChat.Domain.Services;
using AIChat.Infrastructure.Data;
using AIChat.Infrastructure.ExternalServices;
using AIChat.Infrastructure.Repositories;
using AIChat.Shared.Plugins;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 加载 .env 文件
LoadEnvironmentVariables();

// 配置Serilog日志
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.Console()
        .WriteTo.File("logs/aichat-.txt", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(context.Configuration);
});

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AIChat API", Version = "v1" });
});

// 添加SignalR
builder.Services.AddSignalR();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                           ?? new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 注册数据库上下文
builder.Services.AddSingleton<DatabaseContext>();

// 注册仓储
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IPluginRepository, PluginRepository>();

// 注册领域服务
builder.Services.AddScoped<IAIModelService, AIModelService>();
builder.Services.AddScoped<IChatDomainService, ChatDomainService>();

// 注册插件系统服务
builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<IPluginEventBus, PluginEventBus>();
builder.Services.AddSingleton<IPluginStorage, PluginStorage>();
builder.Services.AddScoped<IPluginContext, SimplePluginContext>();

// 注册应用服务
builder.Services.AddScoped<ChatAppService>();
builder.Services.AddScoped<PluginAppService>();

// 添加HTTP客户端
builder.Services.AddHttpClient();

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIChat API v1");
        c.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
    });
}

// 添加自定义中间件
app.UseMiddleware<ModelTrackingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

// 映射控制器
app.MapControllers();

// 映射SignalR Hub
app.MapHub<ChatHub>("/chathub");

// 添加健康检查端点
app.MapGet("/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

// 启动时初始化数据库
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    app.Logger.LogInformation("数据库初始化完成");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "数据库初始化失败");
}

app.Logger.LogInformation("AIChat API 启动完成");
app.Logger.LogInformation("Swagger UI: http://localhost:5000");
app.Logger.LogInformation("健康检查: http://localhost:5000/health");

app.Run();

/// <summary>
/// 加载 .env 文件中的环境变量
/// </summary>
static void LoadEnvironmentVariables()
{
    var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (!File.Exists(envFile))
    {
        Console.WriteLine(".env file not found, skipping environment variable loading");
        return;
    }

    try
    {
        var lines = File.ReadAllLines(envFile);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
                Console.WriteLine($"Loaded environment variable: {key}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading .env file: {ex.Message}");
    }
}
