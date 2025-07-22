# 时间戳插件 (TimestampPlugin)

## 概述

时间戳插件是 AIChat 插件系统的一个示例插件，展示了如何为聊天消息添加精确时间戳功能。该插件支持多种时间格式、时区转换，并提供了完整的 API 接口和事件处理机制。

## 功能特性

### 🕒 核心功能
- **多格式支持**: 支持 7 种不同的时间格式，包括标准格式和 Unix 时间戳
- **时区转换**: 自动处理不同时区的时间转换
- **自动时间戳**: 可配置为自动为所有消息添加时间戳
- **API 接口**: 提供 RESTful API 供其他插件或前端调用
- **事件驱动**: 基于事件总线的消息处理机制

### 📅 支持的时间格式
1. `yyyy-MM-dd HH:mm:ss` - 标准格式 (默认)
2. `yyyy-MM-dd HH:mm:ss.fff` - 包含毫秒
3. `MM/dd/yyyy HH:mm:ss` - 美式格式
4. `dd/MM/yyyy HH:mm:ss` - 欧式格式
5. `yyyy年MM月dd日 HH:mm:ss` - 中文格式
6. `HH:mm:ss` - 仅时间
7. `unix` - Unix 时间戳

## 安装和配置

### 安装步骤

1. **编译插件**
   ```bash
   dotnet build TimestampPlugin.csproj -c Release
   ```

2. **复制文件到插件目录**
   ```
   AIChat/plugins/TimestampPlugin/
   ├── TimestampPlugin.dll
   ├── manifest.json
   └── README.md
   ```

3. **通过 AIChat 插件管理器启用插件**

### 配置参数

插件支持以下配置参数：

```json
{
  "enabled": true,
  "defaultFormat": "yyyy-MM-dd HH:mm:ss",
  "timeZone": "Asia/Shanghai",
  "showMilliseconds": false,
  "autoAddToMessages": true
}
```

#### 配置说明

| 参数 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `enabled` | boolean | `true` | 是否启用时间戳功能 |
| `defaultFormat` | string | `"yyyy-MM-dd HH:mm:ss"` | 默认时间格式 |
| `timeZone` | string | `"Asia/Shanghai"` | 时区设置 |
| `showMilliseconds` | boolean | `false` | 是否显示毫秒 |
| `autoAddToMessages` | boolean | `true` | 是否自动为消息添加时间戳 |

## API 接口

### 1. 获取当前时间戳

**请求**
```http
GET /api/plugins/timestamp/current
```

**响应**
```json
{
  "timestamp": "2024-01-01 12:30:45",
  "format": "yyyy-MM-dd HH:mm:ss",
  "timeZone": "Asia/Shanghai",
  "utc": "2024-01-01T04:30:45.000Z"
}
```

### 2. 格式化指定时间

**请求**
```http
POST /api/plugins/timestamp/format
Content-Type: application/json

{
  "dateTime": "2024-01-01T12:30:45.000Z",
  "format": "yyyy年MM月dd日 HH:mm:ss",
  "timeZone": "Asia/Shanghai"
}
```

**响应**
```json
{
  "timestamp": "2024年01月01日 20:30:45",
  "format": "yyyy年MM月dd日 HH:mm:ss",
  "timeZone": "Asia/Shanghai",
  "originalDateTime": "2024-01-01T12:30:45.000Z"
}
```

### 3. 获取支持的格式

**请求**
```http
GET /api/plugins/timestamp/formats
```

**响应**
```json
{
  "formats": [
    "yyyy-MM-dd HH:mm:ss",
    "yyyy-MM-dd HH:mm:ss.fff",
    "MM/dd/yyyy HH:mm:ss",
    "dd/MM/yyyy HH:mm:ss",
    "yyyy年MM月dd日 HH:mm:ss",
    "HH:mm:ss",
    "unix"
  ],
  "defaultFormat": "yyyy-MM-dd HH:mm:ss"
}
```

## 事件系统

### 发布的事件

插件会发布以下事件：

#### TimestampPluginEvent
```csharp
public class TimestampPluginEvent : IPluginEvent
{
    public string EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourcePluginId { get; set; }
    public string Action { get; set; }        // Started, Stopped, etc.
    public string Message { get; set; }
}
```

### 订阅的事件

插件订阅以下系统事件：

#### MessageSentEvent
当有新消息发送时，插件会自动处理并添加时间戳（如果启用了 `autoAddToMessages`）。

## 使用示例

### 在其他插件中调用 API

```csharp
// 获取当前时间戳
var result = await _context.CallPluginApiAsync(
    "com.aichat.plugins.timestamp", 
    "getCurrentTimestamp"
);

// 格式化指定时间
var formatResult = await _context.CallPluginApiAsync(
    "com.aichat.plugins.timestamp",
    "formatTimestamp",
    new
    {
        dateTime = DateTime.UtcNow,
        format = "unix",
        timeZone = "UTC"
    }
);
```

### 前端 JavaScript 调用

```javascript
// 获取当前时间戳
const response = await fetch('/api/plugins/timestamp/current');
const data = await response.json();
console.log('当前时间戳:', data.timestamp);

// 格式化时间
const formatResponse = await fetch('/api/plugins/timestamp/format', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    dateTime: new Date().toISOString(),
    format: 'yyyy年MM月dd日 HH:mm:ss',
    timeZone: 'Asia/Shanghai'
  })
});
const formatData = await formatResponse.json();
console.log('格式化时间:', formatData.timestamp);
```

## 开发说明

### 项目结构

```
TimestampPlugin/
├── TimestampPlugin.cs          # 主插件类
├── TimestampPlugin.csproj      # 项目文件
├── manifest.json               # 插件清单
├── README.md                   # 说明文档
└── icon.png                    # 插件图标
```

### 关键代码解析

#### 1. 插件初始化
```csharp
public async Task InitializeAsync(IPluginContext context)
{
    _context = context;
    _config = _context.GetPluginConfiguration<TimestampPluginConfig>(Id);
    RegisterApis();
    SubscribeToEvents();
    // ... 其他初始化逻辑
}
```

#### 2. API 注册
```csharp
private void RegisterApis()
{
    _context.RegisterApi("getCurrentTimestamp", async (parameters) =>
    {
        var timestamp = GetFormattedTimestamp(DateTime.UtcNow, 
            _config.DefaultFormat, _config.TimeZone);
        return new { timestamp, format, timeZone, utc };
    });
}
```

#### 3. 事件处理
```csharp
private void SubscribeToEvents()
{
    _context.SubscribeEvent<MessageSentEvent>(async (evt) =>
    {
        if (_config?.AutoAddToMessages == true)
        {
            var timestamp = GetFormattedTimestamp(evt.Timestamp, 
                _config.DefaultFormat, _config.TimeZone);
            // 处理消息时间戳...
        }
    });
}
```

## 测试指南

### 单元测试

```csharp
[Test]
public async Task Should_Initialize_Successfully()
{
    // Arrange
    var plugin = new TimestampPlugin();
    var mockContext = new Mock<IPluginContext>();
    
    // Act
    await plugin.InitializeAsync(mockContext.Object);
    
    // Assert
    Assert.IsTrue(plugin.IsEnabled);
}

[Test]
public void Should_Format_Timestamp_Correctly()
{
    // Arrange
    var dateTime = new DateTime(2024, 1, 1, 12, 30, 45);
    var plugin = new TimestampPlugin();
    
    // Act
    var result = plugin.GetFormattedTimestamp(dateTime, 
        "yyyy-MM-dd HH:mm:ss", "UTC");
    
    // Assert
    Assert.AreEqual("2024-01-01 12:30:45", result);
}
```

### 集成测试

1. **安装插件**：通过插件管理器安装并启用插件
2. **配置测试**：修改插件配置并验证生效
3. **API 测试**：调用各个 API 端点验证响应
4. **事件测试**：发送消息验证时间戳自动添加功能

## 常见问题

### Q: 时区设置不生效怎么办？
A: 确保系统支持指定的时区 ID，可以使用标准的时区标识符如 "UTC"、"Asia/Shanghai" 等。

### Q: Unix 时间戳格式如何使用？
A: 在配置中将 `defaultFormat` 设置为 "unix"，或在 API 调用时指定 format 参数为 "unix"。

### Q: 如何自定义时间格式？
A: 目前仅支持预定义的 7 种格式，如需自定义格式，请修改 `_supportedFormats` 列表并重新编译插件。

### Q: 插件日志在哪里查看？
A: 插件日志会输出到 AIChat 主应用的日志系统中，可通过日志管理界面查看。

## 版本历史

- **v1.0.0** (2024-01-01)
  - 初始版本发布
  - 支持基本时间戳功能
  - 提供 API 接口和事件处理

## 许可证

本插件示例遵循 Apache 2.0 许可证。

## 技术支持

- 📖 [插件开发文档](../../README.md)
- 🐛 [问题反馈](https://github.com/your-repo/issues)
- 💬 [开发者社区](https://github.com/your-repo/discussions)