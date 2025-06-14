using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPChatbotAssistant.Server.Models;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    public int SessionId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsUserMessage { get; set; }

    // Navigation property
    [ForeignKey("SessionId")]
    public virtual ChatSession ChatSession { get; set; } = null!;
}

public class CreateMessageRequest
{
    [Required]
    public int SessionId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public bool IsUserMessage { get; set; }
} 