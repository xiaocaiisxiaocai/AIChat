# AIChat - 智能对话应用

AIChat 是一个采用 DDD 分层架构的智能聊天应用，支持多种 AI 模型（OpenAI GPT、Anthropic Claude）。

## ✨ 特性

- 🤖 **多AI模型支持** - OpenAI GPT、Anthropic Claude
- 💬 **流式对话** - 实时显示AI响应过程  
- 🧠 **思考过程** - Claude模型支持思考过程可视化
- 📱 **Apple HIG设计** - 优雅的用户界面设计
- 💾 **对话持久化** - SQLite数据库存储对话历史
- ⚡ **实时通信** - SignalR支持实时消息推送
- 🏗️ **DDD架构** - 清晰的分层架构设计

## 🛠️ 技术栈

### 后端
- **框架**: ASP.NET Core 8 Web API
- **AI集成**: Microsoft Semantic Kernel  
- **数据库**: SqlSugar ORM + SQLite
- **实时通信**: SignalR
- **日志**: Serilog
- **架构**: Domain-Driven Design (DDD)

### 前端
- **框架**: React 18 + TypeScript
- **设计**: Apple Human Interface Guidelines
- **状态管理**: React Hooks
- **实时通信**: SignalR Client + Server-Sent Events
- **HTTP客户端**: Axios

## 🚀 快速开始

### 1. 环境要求

- **.NET 8 SDK** - [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [下载地址](https://nodejs.org/)
- **Git** - 版本控制工具

### 2. 环境变量配置

在系统环境变量中设置：

```bash
# OpenAI API Key (可选)
OPENAI_API_KEY=your_openai_api_key_here

# Anthropic API Key (可选) 
ANTHROPIC_API_KEY=your_anthropic_api_key_here
```

### 3. 启动应用

#### 后端 API

```bash
# 进入后端目录
cd src/1.Presentation/AIChat.Api

# 安装依赖
dotnet restore

# 启动API服务 (默认端口: 5000)
dotnet run
```

#### 前端 Web

```bash
# 进入前端目录  
cd src/aichat-web

# 安装依赖
npm install

# 启动开发服务器 (默认端口: 3000)
npm start
```

### 4. 访问应用

- **前端界面**: http://localhost:3000
- **后端API**: http://localhost:5000/swagger
- **健康检查**: http://localhost:5000/health

## 📁 项目结构

```
AIChat/
├── src/
│   ├── 1.Presentation/           # 表现层
│   │   └── AIChat.Api/          # Web API + SignalR
│   ├── 2.Application/           # 应用服务层
│   │   └── AIChat.Application/  # 业务流程编排
│   ├── 3.Domain/               # 领域层
│   │   └── AIChat.Domain/      # 核心业务逻辑
│   ├── 4.Infrastructure/       # 基础设施层
│   │   └── AIChat.Infrastructure/ # 数据访问 + 外部服务
│   ├── 5.Shared/              # 共享层
│   │   └── AIChat.Shared/     # 公共工具
│   └── aichat-web/            # React 前端
├── tests/                     # 测试项目
├── CLAUDE.md                  # 开发指导文档
└── README.md
```

## 🎯 核心功能

### AI模型管理
- 支持多种AI模型配置
- 实时模型切换
- 模型状态追踪

### 对话管理  
- 创建/删除对话
- 对话历史浏览
- 消息持久化存储

### 实时通信
- 流式消息显示
- 思考过程可视化
- SignalR实时推送

## 🔧 开发命令

### 后端开发

```bash
# 编译项目
dotnet build

# 运行测试
dotnet test

# 代码格式化
dotnet format
```

### 前端开发

```bash
# 类型检查
npx tsc --noEmit

# 代码格式化  
npm run format

# ESLint检查
npm run lint
```

## 📊 API接口

### 聊天相关
- `POST /api/chat/message` - 发送消息
- `POST /api/chat/message/stream` - 流式消息  
- `GET /api/chat/conversations` - 获取对话列表
- `GET /api/chat/conversations/{id}` - 获取对话详情
- `POST /api/chat/conversations` - 创建对话
- `PUT /api/chat/conversations/{id}` - 更新对话
- `DELETE /api/chat/conversations/{id}` - 删除对话

### 模型管理
- `GET /api/chat/models` - 获取可用模型

### SignalR Hub
- `/chathub` - 实时聊天通信

## 🎨 UI设计

项目采用 Apple Human Interface Guidelines (HIG) 设计原则：

- **简洁性** - 清晰的界面层次
- **一致性** - 统一的交互模式
- **直观性** - 符合用户期望的操作方式
- **美观性** - 优雅的视觉设计

### 关键组件
- **ThinkingBubble** - 思考过程显示组件
- **MessageBubble** - 消息气泡组件  
- **ModelSelector** - AI模型选择器
- **ChatInterface** - 主聊天界面
- **ConversationList** - 对话列表

## 🔐 环境配置

### appsettings.json 示例

```json
{
  "AIModels": {
    "DefaultModel": "gpt-3.5-turbo",
    "Models": [
      {
        "Id": "gpt-4",
        "Name": "GPT-4", 
        "Provider": "OpenAI",
        "SupportsStreaming": true,
        "SupportsThinking": false
      },
      {
        "Id": "claude-3-5-sonnet",
        "Name": "Claude 3.5 Sonnet",
        "Provider": "Anthropic", 
        "SupportsStreaming": true,
        "SupportsThinking": true
      }
    ]
  }
}
```

## 🚧 开发注意事项

1. **API密钥安全** - 绝不将API密钥硬编码到代码中
2. **DDD原则** - 严格遵循分层架构原则
3. **错误处理** - 完善的异常处理机制
4. **性能优化** - 流式渲染提升用户体验
5. **可访问性** - 支持键盘导航和屏幕阅读器

## 📝 许可证

Apache License 2.0 - 详见 [LICENSE](LICENSE) 文件

本项目采用 Apache 2.0 开源许可证，允许商业使用、修改和分发。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

---

**AIChat** - 让AI对话更加智能和优雅 🤖✨
