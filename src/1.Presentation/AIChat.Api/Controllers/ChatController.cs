using AIChat.Application.DTOs;
using AIChat.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChat.Api.Controllers;

/// <summary>
/// 聊天控制器 - 处理聊天相关的HTTP请求
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatAppService _chatAppService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ChatAppService chatAppService, ILogger<ChatController> logger)
    {
        _chatAppService = chatAppService;
        _logger = logger;
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    [HttpPost("message")]
    public async Task<ActionResult<ChatResponseDto>> SendMessage([FromBody] ChatRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("消息内容不能为空");
            }

            var response = await _chatAppService.SendMessageAsync(request);
            
            // 在响应头中添加当前使用的模型信息
            Response.Headers["X-Current-Model"] = response.ModelUsed;
            _logger.LogInformation("[{DateTime}] 当前使用模型: {ModelId}", DateTime.Now, response.ModelUsed);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息时发生错误");
            return StatusCode(500, new { error = "处理消息时发生错误", message = ex.Message });
        }
    }

    /// <summary>
    /// 发送流式聊天消息
    /// </summary>
    [HttpPost("message/stream")]
    public async Task SendStreamingMessage([FromBody] ChatRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("消息内容不能为空");
                return;
            }

            // 设置服务器发送事件(SSE)响应头
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["Access-Control-Allow-Origin"] = "*";

            await foreach (var chunk in _chatAppService.SendStreamingMessageAsync(request))
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(chunk);
                await Response.WriteAsync($"data: {jsonData}\n\n");
                await Response.Body.FlushAsync();

                // 输出当前使用的模型
                if (!string.IsNullOrEmpty(chunk.ModelUsed))
                {
                    _logger.LogInformation("[{DateTime}] 当前使用模型: {ModelId}", DateTime.Now, chunk.ModelUsed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送流式消息时发生错误");
            var errorData = System.Text.Json.JsonSerializer.Serialize(new { 
                type = "error", 
                content = ex.Message, 
                isComplete = true 
            });
            await Response.WriteAsync($"data: {errorData}\n\n");
        }
    }

    /// <summary>
    /// 获取对话列表
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations(
        [FromQuery] int pageIndex = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var conversations = await _chatAppService.GetConversationsAsync(pageIndex, pageSize);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取对话列表时发生错误");
            return StatusCode(500, new { error = "获取对话列表失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取对话详情
    /// </summary>
    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(string id)
    {
        try
        {
            var conversation = await _chatAppService.GetConversationByIdAsync(id);
            if (conversation == null)
            {
                return NotFound($"对话 {id} 不存在");
            }
            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取对话详情时发生错误 - ConversationId: {ConversationId}", id);
            return StatusCode(500, new { error = "获取对话详情失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 创建新对话
    /// </summary>
    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationRequestDto request)
    {
        try
        {
            var conversation = await _chatAppService.CreateConversationAsync(request);
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建对话时发生错误");
            return StatusCode(500, new { error = "创建对话失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 更新对话标题
    /// </summary>
    [HttpPut("conversations/{id}")]
    public async Task<ActionResult<ConversationDto>> UpdateConversation(
        string id, 
        [FromBody] UpdateConversationRequestDto request)
    {
        try
        {
            var conversation = await _chatAppService.UpdateConversationAsync(id, request);
            if (conversation == null)
            {
                return NotFound($"对话 {id} 不存在");
            }
            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新对话时发生错误 - ConversationId: {ConversationId}", id);
            return StatusCode(500, new { error = "更新对话失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 删除对话
    /// </summary>
    [HttpDelete("conversations/{id}")]
    public async Task<ActionResult> DeleteConversation(string id)
    {
        try
        {
            var success = await _chatAppService.DeleteConversationAsync(id);
            if (!success)
            {
                return NotFound($"对话 {id} 不存在");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除对话时发生错误 - ConversationId: {ConversationId}", id);
            return StatusCode(500, new { error = "删除对话失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取可用的AI模型列表
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult> GetAvailableModels()
    {
        try
        {
            var models = await _chatAppService.GetAvailableModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取AI模型列表时发生错误");
            return StatusCode(500, new { error = "获取模型列表失败", message = ex.Message });
        }
    }
}