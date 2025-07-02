namespace ERPChatbotAssistant.Server.Models
{
    public class TrainingDataCsvDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Keywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Embedding { get; set; }
    }
}

