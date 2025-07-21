# CLAUDE.md

此文件为 Claude Code 在 AIChat 项目中的核心指导文档。
每次对话完成要及时提交git，且在必要的方法中需要注释清晰

## 项目概述

AIChat 是一个智能聊天应用，采用 DDD 分层架构：
- **后端**: C# + ASP.NET Core + Microsoft Semantic Kernel + SqlSugar ORM
- **前端**: React + TypeScript + Apple HIG 设计
- **核心功能**: 多AI模型支持、对话历史存储、模型使用追踪

## 新手快速开始

### 1. 环境要求
- **.NET 8 SDK**: [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+**: [下载地址](https://nodejs.org/)
- **Git**: 版本控制工具
- **Visual Studio 2022** 或 **VS Code**: 推荐IDE

### 2. 环境变量配置 (必须)
在系统环境变量或 `.env` 文件中设置：
```bash
# OpenAI API Key (如果使用GPT-4)
OPENAI_API_KEY=your_openai_api_key_here

# Anthropic API Key (如果使用Claude)
ANTHROPIC_API_KEY=your_anthropic_api_key_here

# 数据库连接字符串
DATABASE_CONNECTION_STRING=Data Source=aichat.db
```

### 3. 首次启动步骤
```bash
# 1. 克隆项目
git clone <repository-url>
cd AIChat

# 2. 后端启动 (先启动后端)
cd src/AIChat.Api
dotnet restore                    # 安装.NET包
dotnet build                      # 编译项目
dotnet run                        # 启动API (默认端口5000)

# 3. 前端启动 (新开终端)
cd src/aichat-web
npm install                       # 安装依赖
npm start                         # 启动前端 (默认端口3000)
```

### 4. 验证安装
- 后端API: http://localhost:5000/swagger
- 前端界面: http://localhost:3000
- 数据库会自动创建 (SQLite文件)

## 技术栈

### 后端 (C#)
- **框架**: ASP.NET Core Web API (.NET 8)
- **AI集成**: Microsoft Semantic Kernel (支持 OpenAI、Claude、自定义API)
- **ORM**: SqlSugar (CodeFirst，支持 SQLite/SQL Server)
- **实时通信**: SignalR
- **自动化**: 模型使用追踪
- **日志**: Serilog

### 前端 (React)
- **核心**: React 18 + TypeScript
- **设计**: Apple HIG 风格组件
- **状态**: Redux Toolkit 或 Zustand
- **特性**: 流式渲染、思考过程显示、模型选择器

## 开发命令

### 后端 (必须无警告)
```bash
cd src/AIChat.Api
dotnet restore
dotnet format
dotnet build --verbosity normal --no-restore
dotnet test --no-build --verbosity normal
dotnet run --no-build
```

### 前端 (必须无错误)
```bash
cd src/aichat-web
npm install
npm run format
npm run lint
npx tsc --noEmit
npm start
```


## DDD分层架构详解

### 项目结构说明
```
AIChat/
├── src/
│   ├── 1.Presentation/           # 表现层 (用户交互)
│   │   ├── AIChat.Api/          # Web API + SignalR + 中间件
│   │   └── AIChat.Web/          # React + TypeScript + Apple HIG
│   ├── 2.Application/           # 应用服务层 (业务流程编排)
│   │   └── AIChat.Application/  # 聊天服务 + 模型管理 + Git服务
│   ├── 3.Domain/               # 领域层 (核心业务逻辑)
│   │   └── AIChat.Domain/      # 实体 + 值对象 + 仓储接口
│   ├── 4.Infrastructure/       # 基础设施层 (外部依赖)
│   │   └── AIChat.Infrastructure/ # SqlSugar + AI服务 + Git集成
│   └── 5.Shared/              # 共享层 (公共工具)
│       └── AIChat.Shared/     # 常量 + 扩展方法 + 工具类
└── tests/                     # 测试项目
```

### 各层职责说明

#### 1. Presentation Layer (表现层)
**职责**: 处理用户请求，格式化响应
- **AIChat.Api**: 
  - Controllers: 处理HTTP请求
  - Middlewares: 模型追踪、异常处理、Git自动提交
  - Hubs: SignalR实时通信
  - DTOs: 数据传输对象
- **AIChat.Web**: React前端应用

#### 2. Application Layer (应用服务层)  
**职责**: 编排业务流程，协调领域服务
- **Services**: 
  - ChatAppService: 聊天业务流程
  - ModelAppService: AI模型管理
- **依赖**: 只依赖Domain层，不依赖Infrastructure

#### 3. Domain Layer (领域层)
**职责**: 核心业务逻辑，不依赖任何外部框架
- **Entities**: 业务实体 (Conversation, Message, AIModel)
- **ValueObjects**: 值对象 (ModelConfiguration)
- **Repositories**: 仓储接口 (由Infrastructure实现)
- **Services**: 领域服务 (纯业务逻辑)

#### 4. Infrastructure Layer (基础设施层)
**职责**: 实现外部依赖，提供技术能力
- **Data**: SqlSugar数据访问
- **ExternalServices**: AI API调用 (OpenAI, Claude)
- **Repositories**: 仓储模式实现

#### 5. Shared Layer (共享层)
**职责**: 跨层公共代码
- **Constants**: 常量定义
- **Extensions**: 扩展方法  
- **Utilities**: 工具类

### 核心组件

#### 后端 (关键中间件)
- **ModelTrackingMiddleware**: 自动追踪并输出当前使用的AI模型
- **ChatController**: 聊天API + 模型信息输出
- **ChatHub**: SignalR实时通信

#### 前端 (核心组件)
- **ModelSelector**: AI模型选择器 (实时显示当前模型)
- **ChatInterface**: 主聊天界面 (Apple风格)
- **StreamingMessage**: 流式消息渲染 (支持思考过程)
- **ConversationList**: 历史对话管理

### 数据库设计 (SqlSugar CodeFirst)

#### 核心实体
```csharp
[SugarTable("Conversations")]
public class Conversation
{
    [SugarColumn(IsPrimaryKey = true)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string ModelId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
}

[SugarTable("Messages")]
public class Message
{
    [SugarColumn(IsPrimaryKey = true)]
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string Role { get; set; } // "user" | "assistant"
    public string Content { get; set; }
    public string? ThinkingContent { get; set; }
    public string ModelId { get; set; }
    public DateTime CreatedAt { get; set; }
}

```

#### 数据管理
- **对话历史**: SqlSugar持久化存储
- **模型追踪**: 每条消息记录使用的AI模型  

### AI模型配置

#### appsettings.json 结构
```json
{
  "AIModels": {
    "DefaultModel": "claude-3-5-sonnet",
    "Models": [
      {
        "Id": "gpt-4",
        "Name": "GPT-4",
        "Provider": "OpenAI",
        "ApiKey": "{环境变量}",
        "SupportsStreaming": true,
        "SupportsThinking": false
      },
      {
        "Id": "claude-3-5-sonnet", 
        "Name": "Claude 3.5 Sonnet",
        "Provider": "Anthropic",
        "ApiKey": "{环境变量}",
        "SupportsStreaming": true,
        "SupportsThinking": true
      }
    ]
  }
}
```

#### 核心接口示例
```csharp
// AI模型服务接口 (Domain层定义，Infrastructure层实现)
public interface IAIModelService
{
    /// <summary>
    /// 发送普通消息 (非流式)
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="modelId">AI模型ID</param>
    /// <returns>AI响应</returns>
    Task<ChatResponse> SendMessageAsync(string message, string modelId);
    
    /// <summary>
    /// 发送流式消息 (支持思考过程)
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="modelId">AI模型ID</param>
    /// <returns>流式响应</returns>
    IAsyncEnumerable<StreamingResponse> SendStreamingMessageAsync(string message, string modelId);
    
    /// <summary>
    /// 获取可用的AI模型列表
    /// </summary>
    /// <returns>模型配置列表</returns>
    Task<List<AIModelConfig>> GetAvailableModelsAsync();
}

// 聊天响应数据结构
public class ChatResponse
{
    public string Content { get; set; }          // AI回答内容
    public string? ThinkingContent { get; set; } // 思考过程 (仅Claude等支持)
    public string ModelUsed { get; set; }        // 使用的模型ID
    public DateTime CreatedAt { get; set; }      // 创建时间
}

// 流式响应数据结构
public class StreamingResponse
{
    public string Type { get; set; }             // "thinking" | "content"
    public string Content { get; set; }          // 内容片段
    public bool IsComplete { get; set; }         // 是否完成
}
```

### 思考模型UI设计 (仅Claude支持)

#### React组件示例
```typescript
// 思考状态接口
interface ThinkingState {
  isThinking: boolean;           // 是否正在思考
  thinkingContent: string;       // 思考过程内容
  isThinkingComplete: boolean;   // 思考是否完成
}

// 思考气泡组件
const ThinkingBubble: React.FC<{ content: string; isComplete: boolean }> = ({ content, isComplete }) => {
  return (
    <div className={`thinking-bubble ${isComplete ? 'complete' : 'active'}`}>
      <div className="thinking-header">
        <span className="thinking-icon">🤔</span>
        <span>AI正在思考...</span>
        {!isComplete && <div className="typing-dots">...</div>}
      </div>
      <div className="thinking-content">
        {content}
      </div>
    </div>
  );
};
```

#### Apple风格样式
```css
/* 思考气泡样式 (虚线边框，半透明背景) */
.thinking-bubble {
  border: 2px dashed #007AFF;              /* 苹果蓝色虚线边框 */
  background: rgba(0, 122, 255, 0.1);      /* 半透明蓝色背景 */
  border-radius: 12px;                     /* 苹果风格圆角 */
  padding: 12px 16px;
  margin-bottom: 8px;
  font-style: italic;
  color: #007AFF;
}

/* 正常消息气泡样式 */
.content-bubble {
  border: 1px solid #E5E5E7;              /* 浅灰色边框 */
  background: #FFFFFF;                     /* 白色背景 */
  border-radius: 12px;
  padding: 12px 16px;
  color: #000000;
}

/* 思考完成后淡化效果 */
.thinking-bubble.complete {
  opacity: 0.7;
  border-style: solid;                     /* 完成后改为实线 */
}

/* 打字机动画效果 */
.typing-dots {
  display: inline-block;
  animation: typing 1.5s infinite;
}

@keyframes typing {
  0%, 60%, 100% { opacity: 0; }
  30% { opacity: 1; }
}
```

## 开发最佳实践

### 代码质量要求
- **零警告编译**: C# 和 TypeScript 必须无任何编译警告
- **代码格式化**: 使用 `dotnet format` 和 `npm run format`
- **类型安全**: 启用严格的TypeScript类型检查
- **异常处理**: 所有async方法都要有try-catch

### DDD架构原则
```csharp
// ✅ 正确: Application层只依赖Domain层
public class ChatAppService
{
    private readonly IChatRepository _repository;  // Domain层接口
    private readonly IChatDomainService _domainService;
    
    // 业务流程编排，不包含具体实现细节
}

// ❌ 错误: Application层直接依赖Infrastructure
public class ChatAppService
{
    private readonly SqlSugarClient _db;  // 违反DDD分层原则
}
```

### 模型追踪实现 (重要)
每次AI输出都必须显示当前使用的模型：

```csharp
// ModelTrackingMiddleware 示例
public class ModelTrackingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // 记录当前请求使用的模型
        var modelId = context.Request.Headers["X-Model-Id"];
        
        await next(context);
        
        // 在响应中添加模型信息
        context.Response.Headers.Add("X-Current-Model", modelId);
        
        // 输出到控制台/日志
        Console.WriteLine($"[{DateTime.Now}] 当前使用模型: {modelId}");
    }
}
```

### 必须功能清单
- ✅ **SqlSugar CodeFirst**: 自动创建数据库表结构
- ✅ **对话历史存储**: 每条消息都要持久化
- ✅ **流式渲染**: Server-Sent Events + 思考过程显示  
- ✅ **模型显示**: 实时显示当前使用的AI模型

### 常见问题排除

#### 1. API密钥配置问题
```bash
# 检查环境变量是否设置
echo $OPENAI_API_KEY
echo $ANTHROPIC_API_KEY

# Windows PowerShell
$env:OPENAI_API_KEY
$env:ANTHROPIC_API_KEY
```

#### 2. 数据库连接失败
- 检查 `appsettings.json` 中的连接字符串
- 确保SQLite文件有写入权限
- 查看 SqlSugar 初始化日志

#### 3. 前后端通信问题
- 后端API地址: http://localhost:5000
- 前端代理配置检查 `package.json` 中的 `proxy` 设置
- CORS策略配置检查


### 安全要求
- **环境变量**: API密钥绝对不能硬编码到代码中
- **输入验证**: 使用FluentValidation验证所有用户输入
- **SQL注入防护**: SqlSugar参数化查询自动防护
- **异常处理**: 不向前端暴露敏感的错误信息