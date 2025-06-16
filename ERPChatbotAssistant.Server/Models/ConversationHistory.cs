using System;
using System.Collections.Generic;

namespace ERPChatbotAssistant.Server.Models
{
    public class ConversationHistory
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string? UserMessage { get; set; }
        public string? BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Context { get; set; }  // Additional context or metadata
    }

    public class TrainingData
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public string Keywords { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 