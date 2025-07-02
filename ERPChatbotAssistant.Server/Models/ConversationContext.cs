using System.Collections.Generic;

namespace ERPChatbotAssistant.Server.Models
{
    public class ConversationContext
    {
        public string Intent { get; set; } = string.Empty;
        public Dictionary<string, string> Entities { get; set; } = new Dictionary<string, string>();
        public string State { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public double? Confidence { get; set; }
        
    }
} 