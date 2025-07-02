using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Linq;
using System.Net.Http.Headers;
using ERPChatbotAssistant.Server.Models;
using Microsoft.EntityFrameworkCore;
using ERPChatbotAssistant.Server.Data;
using System.Text.RegularExpressions;
using ERPChatbotAssistant.Server.Services;

namespace ERPChatbotAssistant.Server.Services;

public class ModelTrainingService
{
    private readonly ILogger<ModelTrainingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelId;
    private readonly string _baseUrl;
    private readonly string _applicationUrl;
    private readonly ApplicationDbContext _dbContext;
    private const int MaxHistoryLength = 10; // Maximum number of previous messages to include
    private const double SimilarityThreshold = 0.7; // Minimum similarity score to consider a match
    private readonly IntentDetectionService _intentDetectionService;

    private string BuildSystemPrompt(List<TrainingData> trainingData)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are an ERP system assistant. Your responses should be based on the following training data and guidelines:");
        
        // Add training data examples
        if (trainingData.Any())
        {
            prompt.AppendLine("\nTraining Examples:");
            foreach (var data in trainingData)
            {
                prompt.AppendLine($"Category: {data.Category}");
                prompt.AppendLine($"Question: {data.Question}");
                prompt.AppendLine($"Answer: {data.Answer}");
                prompt.AppendLine();
            }
        }

        // Add guidelines
        prompt.AppendLine("\nGuidelines:");
        prompt.AppendLine("1. Use the training examples as a reference for similar questions");
        prompt.AppendLine("2. Provide step-by-step instructions when explaining processes");
        prompt.AppendLine("3. Include specific menu paths and navigation steps");
        prompt.AppendLine("4. Mention any prerequisites or warnings when necessary");
        prompt.AppendLine("5. Keep responses concise and focused on the user's question");
        prompt.AppendLine("6. After providing the main answer, naturally ask a relevant follow-up question");
        prompt.AppendLine("7. Make the conversation feel natural and human-like");
        prompt.AppendLine("8. Use a friendly and helpful tone");
        prompt.AppendLine("9. Format your response naturally, without any special markers or tags");
        prompt.AppendLine("10. If there are multiple options, present them clearly and ask which one the user would prefer");
        prompt.AppendLine("11.Avoid lists or markdown formatting");
        prompt.AppendLine("12. You are an ERP assistant. Only answer questions related to ERP systems (such as HR, finance, inventory, procurement, etc.). If the question is not related to ERP, politely respond: \"Sorry, I can only help with ERP-related questions.\"");
        return prompt.ToString();
    }

    public ModelTrainingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ModelTrainingService> logger,
        ApplicationDbContext dbContext,
        IntentDetectionService intentDetectionService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _intentDetectionService = intentDetectionService;

        // Get API key from configuration
        _apiKey = _configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("OpenRouter API key is not configured");
            throw new InvalidOperationException("OpenRouter API key is not configured. Please check your configuration.");
        }

        _modelId = _configuration["OpenRouter:DefaultModel"] ?? "deepseek/deepseek-r1-0528-qwen3-8b:free";
        _baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai";
        _applicationUrl = _configuration["ApplicationUrl"] ?? "http://localhost:5173";

        // Configure HTTP client headers
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _applicationUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Title", "ERP Chatbot Assistant");

        _logger.LogInformation("ModelTrainingService initialized with BaseUrl: {BaseUrl}, ApplicationUrl: {ApplicationUrl}", 
            _baseUrl, _applicationUrl);
    }

    public async Task<ChatResponse> GenerateResponse(string userMessage, string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                _logger.LogWarning("Empty user message received");
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.LogWarning("Empty session ID received");
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
            }

            _logger.LogInformation("Generating response for message: {Message} in session: {SessionId}", userMessage, sessionId);
            // Get conversation history
            var conversationHistory = await GetConversationHistory(sessionId);
            _logger.LogDebug("Retrieved {Count} conversation history items", conversationHistory.Count);
            // Get top N relevant training data using embeddings
            var topMatches = await GetTopMatchesByEmbedding(userMessage, 3);
            _logger.LogDebug("Retrieved {Count} top matches by embedding", topMatches.Count);
            // Build the system prompt with top matches
            var systemPrompt = BuildSystemPrompt(topMatches);
            _logger.LogDebug("Built system prompt with {Length} characters", systemPrompt.Length);
            // Build the messages list
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };
            // Add conversation history
            foreach (var history in conversationHistory)
            {
                messages.Add(new { role = "user", content = history.UserMessage });
                messages.Add(new { role = "assistant", content = history.BotResponse });
            }

            // Add current message
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _modelId,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1000,
                top_p = 0.9,
                frequency_penalty = 0.3,
                presence_penalty = 0.3
            };

            _logger.LogDebug("Sending request to OpenRouter API with model: {ModelId}", _modelId);

            try
            {
                var requestUrl = $"{_baseUrl}/api/v1/chat/completions";
                _logger.LogDebug("Sending request to URL: {Url}", requestUrl);

                var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenRouter API error: {StatusCode} - {ErrorContent}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"OpenRouter API error: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
                
                if (result?.Choices == null || result.Choices.Count == 0)
                {
                    _logger.LogError("Invalid response from OpenRouter API: {Response}", 
                        JsonSerializer.Serialize(result));
                    throw new InvalidOperationException("Invalid response from OpenRouter API");
                }

                var fullResponse = result.Choices[0].Message.Content;
                
                // Split the response into main response and follow-up question
                var responseParts = SplitResponse(fullResponse);
                var chatResponse = new ChatResponse
                {
                    MainResponse = responseParts.mainResponse,
                    FollowUpQuestion = responseParts.followUpQuestion
                };

                // Save the conversation
                var intentResult = _intentDetectionService.DetectIntent(userMessage);
                var context = new ConversationContext
                {
                    Intent = intentResult.Intent,
                    Entities = intentResult.Entities,
                    Source = "llm",
                    State = "completed",
                    Confidence = intentResult.Confidence
                };
                await SaveConversation(sessionId, userMessage, fullResponse, context);
                
                _logger.LogInformation("Successfully generated response with follow-up question");
                
                return chatResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while calling OpenRouter API");
                throw new HttpRequestException("Unable to connect to the chat service. Please check your internet connection and try again.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing OpenRouter API response");
                throw new InvalidOperationException("Error processing the response from the chat service.", ex);
            }
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error in GenerateResponse");
            throw new InvalidOperationException("An unexpected error occurred while processing your request. Please try again later.", ex);
        }
    }

    private async Task<List<TrainingData>> GetTopMatchesByEmbedding(string userMessage, int topN)
    {
        var apiToken = _configuration["HuggingFace:ApiToken"];
        var userEmbedding = await EmbeddingsHelper.GetBgeEmbeddingAsync(userMessage, apiToken);
        var trainingDataList = await _dbContext.TrainingData.ToListAsync();
        var scored = trainingDataList
            .Select(td => new {
                Data = td,
                Similarity = EmbeddingsHelper.CosineSimilarity(userEmbedding, JsonSerializer.Deserialize<float[]>(td.Embedding))
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topN)
            .Select(x => x.Data)
            .ToList();
        return scored;
    }

    private (string mainResponse, string followUpQuestion) SplitResponse(string fullResponse)
    {
        // Common natural question patterns
        var questionPatterns = new[] 
        { 
            "Would you like",
            "Do you need",
            "Is there anything else",
            "Which option would you prefer",
            "Would you like me to",
            "Should I",
            "Can I help you with",
            "Would you like to know more about",
            "Do you want to",
            "Would you prefer",
            "Would you like me to explain",
            "Should I show you how to",
            "Would you like to try",
            "Do you need help with",
            "Would you like to see"
        };
        
        foreach (var pattern in questionPatterns)
        {
            var index = fullResponse.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                // Find the start of the sentence containing the question
                var sentenceStart = fullResponse.LastIndexOf(". ", index) + 2;
                if (sentenceStart <= 1) sentenceStart = 0;

                var mainResponse = fullResponse.Substring(0, sentenceStart).Trim();
                var followUp = fullResponse.Substring(sentenceStart).Trim();

                // Only split if the follow-up is a complete thought
                if (followUp.EndsWith("?") || followUp.EndsWith("."))
                {
                    return (mainResponse, followUp);
                }
            }
        }

        // If no clear question found, return the full response as main response
        return (fullResponse, string.Empty);
    }

    private async Task<List<ConversationHistory>> GetConversationHistory(string sessionId)
    {
        return await _dbContext.ConversationHistories
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.Timestamp)
            .Take(MaxHistoryLength)
            .OrderBy(c => c.Timestamp)
            .ToListAsync();
    }

    private async Task SaveConversation(string sessionId, string userMessage, string botResponse, ConversationContext context)
    {
        var history = new ConversationHistory
        {
            SessionId = sessionId,
            UserMessage = userMessage,
            BotResponse = botResponse,
            Timestamp = DateTime.UtcNow,
            Context = JsonSerializer.Serialize(context)
        };
        _dbContext.ConversationHistories.Add(history);
        await _dbContext.SaveChangesAsync();
    }
}

public class ChatResponse
{
    public string MainResponse { get; set; } = string.Empty;
    public string FollowUpQuestion { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class OpenRouterResponse
{
    public List<Choice> Choices { get; set; } = new List<Choice>();
}

public class Choice
{
    public OpenRouterMessage Message { get; set; } = new OpenRouterMessage();
}

public class OpenRouterMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}