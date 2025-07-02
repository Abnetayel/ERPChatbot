using System;
using System.Collections.Generic;
using System.Linq;

namespace ERPChatbotAssistant.Server.Services
{
    public class IntentDetectionService
    {
        private readonly Dictionary<string, string[]> _intentKeywords;

        public IntentDetectionService()
        {
            _intentKeywords = new Dictionary<string, string[]>
            {
                ["request_leave"] = new[] { "leave", "vacation", "time off", "holiday", "sick leave", "personal leave", "annual leave" },
                ["check_balance"] = new[] { "balance", "check", "view", "see", "show", "display", "get" },
                ["invoice_related"] = new[] { "invoice", "bill", "payment", "receipt", "billing", "charge", "cost" },
                ["employee_related"] = new[] { "employee", "staff", "personnel", "worker", "team member", "colleague" },
                ["inventory_related"] = new[] { "inventory", "stock", "item", "product", "goods", "supplies", "materials" },
                ["purchase_related"] = new[] { "purchase", "buy", "order", "procurement", "acquisition", "buying" },
                ["sales_related"] = new[] { "sales", "sell", "revenue", "income", "deal", "transaction", "customer" },
                ["finance_related"] = new[] { "finance", "accounting", "money", "budget", "expense", "cost", "financial" },
                ["hr_related"] = new[] { "hr", "human resources", "recruitment", "hiring", "interview", "job", "position" },
                ["payroll_related"] = new[] { "payroll", "salary", "wage", "pay", "compensation", "benefits", "deduction" },
                ["attendance_related"] = new[] { "attendance", "clock in", "clock out", "time tracking", "punch", "schedule" },
                ["vendor_related"] = new[] { "vendor", "supplier", "partner", "contractor", "external" },
                ["customer_related"] = new[] { "customer", "client", "account", "relationship", "service" },
                ["report_related"] = new[] { "report", "analytics", "dashboard", "statistics", "data", "summary" },
                ["system_related"] = new[] { "system", "software", "application", "platform", "tool", "module" },
                ["logout_related"] = new[] { "logout", "log out", "sign out", "signout", "exit", "close session", "logoff", "log off" },
            };
        }

        public IntentResult DetectIntent(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return new IntentResult
                {
                    Intent = "unknown",
                    Confidence = 0.0,
                    Entities = new Dictionary<string, string>()
                };
            }

            var message = userMessage.ToLower();
            var bestMatch = new IntentResult
            {
                Intent = "general_inquiry",
                Confidence = 0.0,
                Entities = new Dictionary<string, string>()
            };

            foreach (var intent in _intentKeywords)
            {
                var keywordCount = intent.Value.Count(keyword => message.Contains(keyword));
                if (keywordCount > 0)
                {
                    var confidence = Math.Min(0.9, 0.3 + (keywordCount * 0.2)); // Base 0.3 + 0.2 per keyword, max 0.9
                    
                    if (confidence > bestMatch.Confidence)
                    {
                        bestMatch.Intent = intent.Key;
                        bestMatch.Confidence = confidence;
                    }
                }
            }

            // Extract basic entities
            bestMatch.Entities = ExtractEntities(userMessage);

            return bestMatch;
        }

        private Dictionary<string, string> ExtractEntities(string userMessage)
        {
            var entities = new Dictionary<string, string>();
            var message = userMessage.ToLower();

            // Extract dates (simple pattern matching)
            var datePatterns = new[] { "today", "tomorrow", "yesterday", "next week", "last week", "this month", "next month" };
            foreach (var pattern in datePatterns)
            {
                if (message.Contains(pattern))
                {
                    entities["date"] = pattern;
                    break;
                }
            }

            // Extract numbers (amounts, quantities)
            var numbers = System.Text.RegularExpressions.Regex.Matches(userMessage, @"\d+");
            if (numbers.Count > 0)
            {
                entities["number"] = numbers[0].Value;
            }

            // Extract currency amounts
            var currencyMatches = System.Text.RegularExpressions.Regex.Matches(userMessage, @"\$?\d+(?:\.\d{2})?");
            if (currencyMatches.Count > 0)
            {
                entities["amount"] = currencyMatches[0].Value;
            }

            return entities;
        }

        public List<string> GetAllIntents()
        {
            return _intentKeywords.Keys.ToList();
        }

        public Dictionary<string, string[]> GetIntentKeywords()
        {
            return new Dictionary<string, string[]>(_intentKeywords);
        }
    }

    public class IntentResult
    {
        public string Intent { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Dictionary<string, string> Entities { get; set; } = new Dictionary<string, string>();
    }
} 