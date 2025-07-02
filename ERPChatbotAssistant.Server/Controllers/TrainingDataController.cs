using Microsoft.AspNetCore.Mvc;
using ERPChatbotAssistant.Server.Data;
using ERPChatbotAssistant.Server.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using CsvHelper.Configuration;

namespace ERPChatbotAssistant.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TrainingDataController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        // POST: api/TrainingData/upload-csv
        [HttpPost("upload-csv")]
        public async Task<IActionResult> UploadCsv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var apiToken = _configuration["HuggingFace:ApiToken"];
            if (string.IsNullOrWhiteSpace(apiToken))
                return StatusCode(500, "HuggingFace API token is not configured.");

            int added = 0;
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null, // Ignore header validation
                    MissingFieldFound = null // Ignore missing fields
                };
                
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<TrainingDataCsvDto>().ToList();
                    foreach (var record in records)
                    {
                        if (!string.IsNullOrWhiteSpace(record.Question) && !string.IsNullOrWhiteSpace(record.Answer))
                        {
                            var trainingData = new TrainingData
                            {
                                Question = record.Question,
                                Answer = record.Answer,
                                Category = record.Category ?? string.Empty,
                                Keywords = record.Keywords ?? string.Empty,
                                CreatedAt = record.CreatedAt == default ? DateTime.UtcNow : record.CreatedAt,
                                Embedding = string.Empty // Will be generated below
                            };

                            if (string.IsNullOrWhiteSpace(record.Embedding))
                            {
                                var embeddingArray = await EmbeddingsHelper.GetBgeEmbeddingAsync(record.Question, apiToken);
                                trainingData.Embedding = JsonSerializer.Serialize(embeddingArray);
                            }
                            else
                            {
                                trainingData.Embedding = record.Embedding;
                            }

                            _context.TrainingData.Add(trainingData);
                            added++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { added });
        }
    }
} 