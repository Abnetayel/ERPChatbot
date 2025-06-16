using Microsoft.AspNetCore.Mvc;
using ERPChatbotAssistant.Server.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ERPChatbotAssistant.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly ModelTrainingService _modelTrainingService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ModelTrainingService modelTrainingService, ILogger<ChatController> logger)
    {
        _modelTrainingService = modelTrainingService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            // Generate a session ID if not provided
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

            var response = await _modelTrainingService.GenerateResponse(request.Message, sessionId);
            var timestamp = DateTime.UtcNow;
            
            return Ok(new ChatResponse 
            { 
                MainResponse = response.MainResponse,
                FollowUpQuestion = response.FollowUpQuestion,
                SessionId = sessionId,
                Timestamp = timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "An error occurred while processing your message");
        }
    }
}

public class ChatRequest
{
    [Required]
    public string Message { get; set; }
    public string SessionId { get; set; }
}

public class ChatResponse
{
    public string MainResponse { get; set; }
    public string FollowUpQuestion { get; set; }
    public string SessionId { get; set; }
    public DateTime Timestamp { get; set; }
} 