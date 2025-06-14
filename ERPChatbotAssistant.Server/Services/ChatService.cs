using ERPChatbotAssistant.Server.Data;
using ERPChatbotAssistant.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ERPChatbotAssistant.Server.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ApplicationDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Message> ProcessMessage(Message message)
    {
        // Store the user message
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Process the message and generate a response
        string responseContent = GenerateResponse(message.Content);

        // Create and store the bot response
        var botResponse = new Message
        {
            SessionId = message.SessionId,
            Content = responseContent,
            IsUserMessage = false,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(botResponse);
        await _context.SaveChangesAsync();

        return botResponse;
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        try
        {
            _logger.LogInformation("Processing message: {Message}", message);
            var response = GenerateResponse(message);
            _logger.LogInformation("Generated response: {Response}", response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
            throw;
        }
    }

    private string GenerateResponse(string userMessage)
    {
        // Convert to lowercase for easier matching
        string message = userMessage.ToLower().Trim();

        // Time related queries
        if (message.Contains("time") || message.Contains("what time") || message.Contains("current time"))
        {
            return $"The current time is {DateTime.Now.ToString("hh:mm tt")}";
        }
        else if (message.Contains("date") || message.Contains("what date") || message.Contains("today's date"))
        {
            return $"Today's date is {DateTime.Now.ToString("MMMM dd, yyyy")}";
        }
        else if (message.Contains("datetime") || message.Contains("date and time"))
        {
            return $"Current date and time: {DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt")}";
        }

        // Help command
        else if (message == "help")
        {
            return "Here's what I can help you with:\n\n" +
                   "📦 Inventory Management:\n" +
                   "   • Check stock levels\n" +
                   "   • View inventory reports\n" +
                   "   • Update inventory\n\n" +
                   "💰 Sales & Orders:\n" +
                   "   • Create new orders\n" +
                   "   • Track order status\n" +
                   "   • View sales history\n\n" +
                   "👥 Customer Management:\n" +
                   "   • Search customers\n" +
                   "   • View customer history\n" +
                   "   • Update customer details\n\n" +
                   "📊 Reports:\n" +
                   "   • Sales reports\n" +
                   "   • Inventory reports\n" +
                   "   • Financial reports\n\n" +
                   "Just ask about any of these topics!";
        }

        // Greetings
        else if (message.Contains("hello") || message.Contains("hi") || message.Contains("hey"))
        {
            return "Hello! I'm your ERP Assistant. I can help you with:\n" +
                   "• Inventory Management\n" +
                   "• Sales & Orders\n" +
                   "• Customer Information\n" +
                   "• Financial Reports\n" +
                   "• HR Management\n" +
                   "What would you like to know about?";
        }

        // Inventory related queries
        else if (message.Contains("inventory") || message.Contains("stock"))
        {
            if (message.Contains("level") || message.Contains("check"))
            {
                return "To check stock levels:\n" +
                       "1. Go to Inventory > Stock Levels\n" +
                       "2. Select the product category\n" +
                       "3. View current stock levels\n\n" +
                       "Would you like to check a specific product's stock level?";
            }
            else if (message.Contains("report"))
            {
                return "Available inventory reports:\n" +
                       "1. Stock Level Report\n" +
                       "2. Low Stock Alert Report\n" +
                       "3. Stock Movement Report\n" +
                       "4. Inventory Valuation Report\n\n" +
                       "Which report would you like to view?";
            }
            else
            {
                return "I can help you with inventory management. Would you like to:\n" +
                       "1. Check stock levels\n" +
                       "2. View inventory reports\n" +
                       "3. Update inventory\n" +
                       "4. Set up low stock alerts\n\n" +
                       "What would you like to do?";
            }
        }

        // Sales related queries
        else if (message.Contains("sales") || message.Contains("order"))
        {
            if (message.Contains("new") || message.Contains("create"))
            {
                return "To create a new sales order:\n" +
                       "1. Go to Sales > New Order\n" +
                       "2. Select customer\n" +
                       "3. Add products\n" +
                       "4. Review and submit\n\n" +
                       "Would you like me to guide you through this process?";
            }
            else if (message.Contains("track") || message.Contains("status"))
            {
                return "To track an order:\n" +
                       "1. Go to Sales > Order Tracking\n" +
                       "2. Enter order number\n" +
                       "3. View current status\n\n" +
                       "Do you have an order number to track?";
            }
            else
            {
                return "I can help you with sales and orders. Would you like to:\n" +
                       "1. Create a new order\n" +
                       "2. Track existing orders\n" +
                       "3. View sales history\n" +
                       "4. Generate sales reports\n\n" +
                       "What would you like to do?";
            }
        }

        // Customer related queries
        else if (message.Contains("customer"))
        {
            if (message.Contains("search") || message.Contains("find"))
            {
                return "To search for a customer:\n" +
                       "1. Go to Customers > Search\n" +
                       "2. Enter customer name or ID\n" +
                       "3. View customer details\n\n" +
                       "Do you have a customer name or ID to search?";
            }
            else if (message.Contains("history"))
            {
                return "To view customer history:\n" +
                       "1. Go to Customers > History\n" +
                       "2. Select customer\n" +
                       "3. View order history\n" +
                       "4. View payment history\n\n" +
                       "Which customer's history would you like to view?";
            }
            else
            {
                return "I can help you with customer information. Would you like to:\n" +
                       "1. Search for a customer\n" +
                       "2. View customer history\n" +
                       "3. Update customer details\n" +
                       "4. Add a new customer\n\n" +
                       "What would you like to do?";
            }
        }

        // Report related queries
        else if (message.Contains("report"))
        {
            if (message.Contains("sales"))
            {
                return "Available sales reports:\n" +
                       "1. Daily Sales Summary\n" +
                       "2. Monthly Sales Analysis\n" +
                       "3. Product Performance Report\n" +
                       "4. Customer Sales Report\n\n" +
                       "Which sales report would you like to view?";
            }
            else if (message.Contains("inventory"))
            {
                return "Available inventory reports:\n" +
                       "1. Stock Level Report\n" +
                       "2. Inventory Movement Report\n" +
                       "3. Low Stock Alert Report\n" +
                       "4. Inventory Valuation Report\n\n" +
                       "Which inventory report would you like to view?";
            }
            else
            {
                return "I can help you generate reports. Available reports include:\n" +
                       "1. Sales reports\n" +
                       "2. Inventory reports\n" +
                       "3. Financial reports\n" +
                       "4. Customer reports\n\n" +
                       "Which type of report would you like to see?";
            }
        }

        // Financial queries
        else if (message.Contains("financial") || message.Contains("finance"))
        {
            return "I can help you with financial information. Would you like to:\n" +
                   "1. View financial reports\n" +
                   "2. Check account balances\n" +
                   "3. View transaction history\n" +
                   "4. Generate financial statements\n\n" +
                   "What financial information do you need?";
        }

        // HR queries
        else if (message.Contains("hr") || message.Contains("human resource") || message.Contains("employee"))
        {
            return "I can help you with HR management. Would you like to:\n" +
                   "1. View employee information\n" +
                   "2. Check attendance records\n" +
                   "3. View payroll information\n" +
                   "4. Access HR policies\n\n" +
                   "What HR information do you need?";
        }

        // Default response
        return "I'm not sure I understand. Here are some things you can ask me about:\n" +
               "• Time and date\n" +
               "• Inventory management\n" +
               "• Sales and orders\n" +
               "• Customer information\n" +
               "• Financial reports\n" +
               "• HR management\n\n" +
               "Or type 'help' to see all available options.";
    }
} 