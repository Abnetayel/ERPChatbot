using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERPChatbotAssistant.Server.Data;
using ERPChatbotAssistant.Server.Models;
using ERPChatbotAssistant.Server.Services;
using System.ComponentModel.DataAnnotations;

namespace ERPChatbotAssistant.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        ApplicationDbContext context,
        IChatService chatService,
        ILogger<ChatController> logger)
    {
        _context = context;
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new chat session for a user
    /// </summary>
    /// <returns>The created chat session</returns>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ChatSession), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatSession>> CreateSession()
    {
        try
        {
            var session = new ChatSession
            {
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, "An error occurred while creating the chat session");
        }
    }

    /// <summary>
    /// Gets a chat session by ID
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>The chat session with its messages</returns>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(ChatSession), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatSession>> GetSession(int sessionId)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
        {
            return NotFound();
        }

        return session;
    }

    /// <summary>
    /// Adds a new message to a chat session and gets the bot's response
    /// </summary>
    /// <param name="request">The message request</param>
    /// <returns>The bot's response message</returns>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Message>> AddMessage([FromBody] CreateMessageRequest request)
    {
        try
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == request.SessionId);

            if (session == null)
            {
                return NotFound("Chat session not found");
            }

            // Process the message and get bot response first
            var botResponse = await _chatService.ProcessMessageAsync(request.Content);
            
            // Create and save the user message
            var userMessage = new Message
            {
                SessionId = request.SessionId,
                Content = request.Content,
                IsUserMessage = true,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(userMessage);

            // Create and save the bot message
            var botMessage = new Message
            {
                SessionId = request.SessionId,
                Content = botResponse,
                IsUserMessage = false,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(botMessage);
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(botMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", request.Content);
            return StatusCode(500, "An error occurred while processing the message");
        }
    }

    /// <summary>
    /// Gets a specific message by ID
    /// </summary>
    /// <param name="id">The message ID</param>
    /// <returns>The message</returns>
    [HttpGet("messages/{id}")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Message>> GetMessage(int id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        return message;
    }

    /// <summary>
    /// Gets all messages for a specific chat session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>List of messages in the session</returns>
    [HttpGet("sessions/{sessionId}/messages")]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Message>>> GetSessionMessages(int sessionId)
    {
        var messages = await _context.Messages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return messages;
    }
}

public class CreateSessionRequest
{
    public int UserId { get; set; } // Made optional for anonymous sessions
} 