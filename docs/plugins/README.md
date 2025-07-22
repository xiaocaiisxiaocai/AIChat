# AIChat 插件开发指南

## 概述

AIChat 采用企业级插件系统架构，支持模块化扩展和热插拔功能。插件系统基于 DDD 分层架构设计，提供类型安全、安全沙箱和统一 API 接口。

## 插件系统特性

### 🚀 核心特性
- **模块化设计**: 完全独立的插件模块，互不干扰
- **热插拔支持**: 支持运行时动态加载和卸载插件
- **类型安全**: 基于 TypeScript 和 C# 的强类型系统
- **安全沙箱**: 插件运行在受控环境中，保障系统安全
- **统一 API**: 标准化的插件接口和生命周期管理
- **事件驱动**: 基于事件总线的插件间通信机制

### 🎯 插件类型
- **AIProvider**: AI模型集成插件
- **UIComponent**: 用户界面组件插件
- **MessageProcessor**: 消息处理插件
- **Storage**: 存储扩展插件
- **Notification**: 通知服务插件
- **Tool**: 实用工具插件
- **Theme**: 界面主题插件
- **Extension**: 功能扩展插件

## 快速开始

### 1. 创建插件项目结构

```
MyPlugin/
├── manifest.json          # 插件清单文件
├── MyPlugin.dll          # 编译后的插件程序集
├── MyPlugin.cs           # 插件主类
├── Configuration/        # 插件配置
│   └── MyPluginConfig.cs
├── Events/              # 插件事件定义
│   └── MyPluginEvent.cs
└── README.md           # 插件说明文档
```

### 2. 实现插件接口

创建插件主类，实现 `IPlugin` 接口：

```csharp
using AIChat.Shared.Plugins;

public class MyPlugin : IPlugin
{
    public string Id => "com.example.myplugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    public string Description => "示例插件描述";
    public string Author => "Your Name";
    public PluginType Type => PluginType.Tool;
    public bool IsEnabled { get; set; } = true;
    public string MinimumAIChatVersion => "1.0.0";
    public IReadOnlyList<string> Dependencies => new List<string>();

    private IPluginContext? _context;

    public async Task InitializeAsync(IPluginContext context)
    {
        _context = context;
        // 插件初始化逻辑
    }

    public async Task StartAsync()
    {
        // 插件启动逻辑
    }

    public async Task StopAsync()
    {
        // 插件停止逻辑
    }

    public async Task DisposeAsync()
    {
        // 插件清理逻辑
    }

    public string GetConfigurationSchema()
    {
        // 返回插件配置的 JSON Schema
        return "{}";
    }

    public bool IsCompatible(string aichatVersion)
    {
        // 检查版本兼容性
        return Version.Parse(aichatVersion) >= Version.Parse(MinimumAIChatVersion);
    }
}
```

### 3. 创建插件清单文件

在插件根目录创建 `manifest.json`：

```json
{
  "id": "com.example.myplugin",
  "name": "My Plugin",
  "version": "1.0.0",
  "description": "示例插件描述",
  "author": "Your Name",
  "type": "Tool",
  "status": "Disabled",
  "assembly": "MyPlugin.dll",
  "entryPoint": "MyPlugin.MyPlugin",
  "dependencies": [],
  "minSystemVersion": "1.0.0",
  "configSchema": {
    "type": "object",
    "properties": {
      "enabled": {
        "type": "boolean",
        "default": true,
        "description": "是否启用插件"
      }
    }
  },
  "permissions": [
    "storage.read",
    "storage.write",
    "events.publish"
  ],
  "apiEndpoints": [],
  "uiMountPoints": [],
  "icon": "icon.png",
  "supportsHotReload": true,
  "createdAt": "2024-01-01T00:00:00.000Z",
  "updatedAt": "2024-01-01T00:00:00.000Z"
}
```

## 插件开发详解

### 插件生命周期

插件系统定义了标准的生命周期管理：

1. **加载 (Loading)**: 系统加载插件程序集
2. **初始化 (Initializing)**: 调用 `InitializeAsync()` 方法
3. **启动 (Starting)**: 调用 `StartAsync()` 方法
4. **运行 (Running)**: 插件正常运行状态
5. **停止 (Stopping)**: 调用 `StopAsync()` 方法
6. **销毁 (Disposing)**: 调用 `DisposeAsync()` 方法

### 插件上下文 (IPluginContext)

插件上下文为插件提供系统访问能力：

```csharp
// 获取日志记录器
var logger = context.Logger;
logger.LogInformation("插件已启动");

// 获取配置
var config = context.GetPluginConfiguration<MyPluginConfig>(Id);

// 发布事件
await context.PublishEventAsync(new MyPluginEvent
{
    EventId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    SourcePluginId = Id
});

// 存储数据
await context.Storage.SetAsync(Id, "key", "value");
var data = await context.Storage.GetAsync<string>(Id, "key");
```

### 事件系统

插件可以发布和订阅事件进行通信：

```csharp
// 定义事件
public class MyPluginEvent : IPluginEvent
{
    public string EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourcePluginId { get; set; }
    public string Message { get; set; }
}

// 发布事件
await _context.PublishEventAsync(new MyPluginEvent 
{ 
    Message = "Hello from plugin!" 
});

// 订阅事件
_context.SubscribeEvent<MyPluginEvent>(async (evt) =>
{
    _context.Logger.LogInformation($"收到事件: {evt.Message}");
});
```

### API 注册

插件可以注册自定义 API 供其他插件调用：

```csharp
// 注册 API
_context.RegisterApi("getMessage", async (parameters) =>
{
    return new { message = "Hello from My Plugin!" };
});

// 调用其他插件 API
var result = await _context.CallPluginApiAsync("other.plugin.id", "someApi", parameters);
```

## 安全和权限

### 权限系统

插件必须在清单文件中声明所需权限：

```json
{
  "permissions": [
    "storage.read",        // 读取存储
    "storage.write",       // 写入存储
    "events.publish",      // 发布事件
    "events.subscribe",    // 订阅事件
    "api.call",           // 调用API
    "ui.mount",           // 挂载UI组件
    "network.access"      // 网络访问
  ]
}
```

### 安全最佳实践

1. **最小权限原则**: 只申请必需的权限
2. **输入验证**: 严格验证所有外部输入
3. **异常处理**: 妥善处理所有异常情况
4. **资源管理**: 正确释放占用的资源
5. **敏感数据**: 避免在日志中输出敏感信息

## 调试和测试

### 本地调试

1. 在 Visual Studio 中创建插件项目
2. 引用 AIChat.Shared 程序集
3. 实现 IPlugin 接口
4. 编译生成插件 DLL
5. 将插件文件复制到 AIChat 插件目录
6. 通过 AIChat 插件管理器加载和测试

### 单元测试

```csharp
[Test]
public async Task Plugin_Should_Initialize_Successfully()
{
    // Arrange
    var plugin = new MyPlugin();
    var mockContext = new Mock<IPluginContext>();
    
    // Act
    await plugin.InitializeAsync(mockContext.Object);
    
    // Assert
    Assert.IsTrue(plugin.IsEnabled);
}
```

## 发布和分发

### 打包插件

1. 编译插件项目生成 DLL
2. 准备插件清单文件和资源文件
3. 创建插件压缩包 (.zip)
4. 验证插件包结构和完整性

### 插件商店

AIChat 支持插件商店机制（规划中）：

- 在线浏览和搜索插件
- 一键安装和更新
- 插件评分和评论
- 开发者收益分成

## 示例插件

请参考以下示例插件了解具体实现：

- [时间戳插件](examples/TimestampPlugin/): 为消息添加时间戳
- [天气插件](examples/WeatherPlugin/): 获取天气信息
- [翻译插件](examples/TranslatePlugin/): 文本翻译功能

## 常见问题

### Q: 如何处理插件依赖关系？
A: 在清单文件的 `dependencies` 数组中声明依赖的插件 ID，系统会自动处理加载顺序。

### Q: 插件数据存储在哪里？
A: 插件数据存储在系统指定的插件数据目录中，通过 `IPluginStorage` 接口访问。

### Q: 如何实现插件间通信？
A: 使用事件总线 (`IPluginEventBus`) 发布和订阅事件，或通过 API 注册机制实现。

### Q: 插件是否支持热重载？
A: 支持，在清单文件中设置 `supportsHotReload: true` 即可启用。

## 技术支持

- 📖 [API 文档](api/README.md)
- 💬 [开发者社区](https://github.com/your-repo/discussions)
- 🐛 [问题反馈](https://github.com/your-repo/issues)
- 📧 [联系我们](mailto:developer@aichat.com)

---

**AIChat 插件系统** - 让 AI 聊天更强大，让开发更简单！