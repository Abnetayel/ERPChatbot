//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ERPChatbotAssistant.Server.Data;
//using ERPChatbotAssistant.Server.Models;
//using Microsoft.Extensions.Logging;
//using System.Linq;

//namespace ERPChatbotAssistant.Server.Services;

//public class ChatService
//{
//    private readonly ApplicationDbContext _context;
//    private readonly ILogger<ChatService> _logger;
//    private readonly ModelTrainingService _modelTrainingService;

//    public ChatService(
//        ApplicationDbContext context,
//        ILogger<ChatService> logger,
//        ModelTrainingService modelTrainingService)
//    {
//        _context = context;
//        _logger = logger;
//        _modelTrainingService = modelTrainingService;
//    }

//    public async Task<List<Message>> GetRecentMessages(int count = 10)
//    {
//        return await _context.Messages
//            .OrderByDescending(m => m.Timestamp)
//            .Take(count)
//            .OrderBy(m => m.Timestamp)
//            .ToListAsync();
//    }

//    public async Task<Message> AddMessage(string content, bool isUserMessage)
//    {
//        var message = new Message
//        {
//            Content = content,
//            IsUserMessage = isUserMessage,
//            Timestamp = DateTime.UtcNow
//        };

//        _context.Messages.Add(message);
//        await _context.SaveChangesAsync();
//        return message;
//    }

//    public async Task<Message> ProcessMessage(Message message)
//    {
//        try
//        {
//            // Store the user message
//            _context.Messages.Add(message);
//            await _context.SaveChangesAsync();

//            // Generate response using the trained model
//            string responseContent = await _modelTrainingService.GenerateResponse(message.Content);

//            // Create and store the bot response
//            var botResponse = new Message
//            {
//                Content = responseContent,
//                IsUserMessage = false,
//                Timestamp = DateTime.UtcNow
//            };

//            _context.Messages.Add(botResponse);
//            await _context.SaveChangesAsync();

//            return botResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error processing message");
//            throw;
//        }
//    }

//    public async Task<string> ProcessMessageAsync(string message)
//    {
//        try
//        {
//            _logger.LogInformation("Processing message: {Message}", message);
//            var response = await _modelTrainingService.GenerateResponse(message);
//            _logger.LogInformation("Generated response: {Response}", response);
//            return response;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error processing message: {Message}", message);
//            throw;
//        }
//    }
//} 