//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ERPChatbotAssistant.Server.Data;
//using ERPChatbotAssistant.Server.Models;

//namespace ERPChatbotAssistant.Server.Services;

//public class TrainingDataService
//{
//    private readonly ApplicationDbContext _context;

//    public TrainingDataService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<List<TrainingExample>> GetTrainingData()
//    {
//        var trainingData = new List<TrainingExample>();

//        // Get recent conversations from messages
//        var recentMessages = await _context.Messages
//            .OrderByDescending(m => m.Timestamp)
//            .Take(100)
//            .OrderBy(m => m.Timestamp)
//            .ToListAsync();

//         //Group messages into conversations
//        var conversations = new List<List<Message>>();
//        var currentConversation = new List<Message>();

//        foreach (var message in recentMessages)
//        {
//            if (currentConversation.Count > 0 && 
//                (message.Timestamp - currentConversation.Last().Timestamp).TotalMinutes > 30)
//            {
//                if (currentConversation.Count >= 2)
//                {
//                    conversations.Add(new List<Message>(currentConversation));
//                }
//                currentConversation.Clear();
//            }
//            currentConversation.Add(message);
//        }

//        if (currentConversation.Count >= 2)
//        {
//            conversations.Add(currentConversation);
//        }

//        // Convert conversations to training examples
//        foreach (var conversation in conversations)
//        {
//            for (int i = 0; i < conversation.Count - 1; i++)
//            {
//                if (conversation[i].IsUserMessage && !conversation[i + 1].IsUserMessage)
//                {
//                    trainingData.Add(new TrainingExample
//                    {
//                        Input = conversation[i].Content,
//                        Output = conversation[i + 1].Content,
//                        Metadata = new Dictionary<string, string>
//                        {
//                            { "timestamp", conversation[i].Timestamp.ToString() },
//                            { "conversation_id", Guid.NewGuid().ToString() }
//                        }
//                    });
//                }
//            }
//        }

//        // Add ERP-specific examples
//        trainingData.AddRange(GetERPSpecificExamples());

//        return trainingData;
//    }

//    private List<TrainingExample> GetERPSpecificExamples()
//    {
//        return new List<TrainingExample>
//        {
//            new TrainingExample
//            {
//                Input = " ",
//                Output = "I can help you check the stock level. Please provide the product ID or name.",
//                Metadata = new Dictionary<string, string>
//                {
//                    { "category", "inventory" },
//                    { "type", "stock_query" }
//                }
//            },
//            new TrainingExample
//            {
//                Input = "How do I create a new sales order?",
//                Output = "To create a new sales order:\n1. Go to Sales > Orders\n2. Click 'New Order'\n3. Select the customer\n4. Add products\n5. Review and submit",
//                Metadata = new Dictionary<string, string>
//                {
//                    { "category", "sales" },
//                    { "type", "process_guide" }
//                }
//            }
//        };
//    }
//}

//public class TrainingExample
//{
//    public string Input { get; set; } = string.Empty;
//    public string Output { get; set; } = string.Empty;
//    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
//} 