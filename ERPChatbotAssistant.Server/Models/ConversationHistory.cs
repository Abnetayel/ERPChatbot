using System;
using System.Collections.Generic;

namespace ERPChatbotAssistant.Server.Models
{
    public class ConversationHistory
    {
        public int Id { get; set; }
        public required string SessionId { get; set; }
        public string? UserMessage { get; set; }
        public string? BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Context { get; set; }  // Additional context or metadata
    }

    public class TrainingData
    {
        public int Id { get; set; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
        public required string Category { get; set; }
        public required string Keywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string Embedding { get; set; }
    }
} 