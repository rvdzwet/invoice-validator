using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for interacting with Gemini Flash 2.0 API to analyze invoice data
    /// </summary>
    public class GeminiService : IGeminiService
    {
        private readonly ILogger<GeminiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _projectId;
        private readonly string _location;
        private readonly string _modelId;

        public GeminiService(ILogger<GeminiService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Load configuration
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key is not configured.");
            _projectId = _configuration["Gemini:ProjectId"] ?? "your-project-id";
            _location = _configuration["Gemini:Location"] ?? "us-central1";
            _modelId = _configuration["Gemini:ModelId"] ?? "gemini-flash-2.0";
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice)
        {
            _logger.LogInformation("Validating if invoice is related to home improvement");
            
            var result = new ValidationResult { ExtractedInvoice = invoice };
            
            try
            {
                // Prepare the invoice data for Gemini
                var prompt = BuildHomeImprovementPrompt(invoice);
                
                // Call Gemini API
                var response = await CallGeminiApiAsync(prompt);
                result.RawGeminiResponse = response;
                
                // Parse the response
                var isHomeImprovement = ParseHomeImprovementResponse(response, result);
                result.IsHomeImprovement = isHomeImprovement;
                
                if (!isHomeImprovement)
                {
                    result.AddIssue(ValidationSeverity.Error, 
                        "The invoice does not appear to be related to home improvement expenses.");
                    result.IsValid = false;
                }
                else
                {
                    result.AddIssue(ValidationSeverity.Info, 
                        "The invoice appears to be valid home improvement expenses.");
                    result.IsValid = true;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating home improvement with Gemini");
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating with Gemini: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering)
        {
            _logger.LogInformation("Checking for fraud indicators in invoice");
            
            try
            {
                // If we already detected PDF tampering, no need to check with Gemini
                if (detectedTampering)
                {
                    _logger.LogWarning("PDF tampering already detected, skipping Gemini fraud check");
                    return true;
                }
                
                // Prepare the fraud detection prompt
                var prompt = BuildFraudDetectionPrompt(invoice);
                
                // Call Gemini API
                var response = await CallGeminiApiAsync(prompt);
                
                // Parse the response
                bool possibleFraud = ParseFraudDetectionResponse(response);
                
                if (possibleFraud)
                {
                    _logger.LogWarning("Gemini detected possible fraud in the invoice");
                }
                
                return possibleFraud;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for fraud with Gemini");
                // If we encounter an error, we should assume no fraud to avoid false positives
                return false;
            }
        }
        
        private string BuildHomeImprovementPrompt(Invoice invoice)
        {
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("You are an expert in assessing if expenses are related to home improvement projects.");
            promptBuilder.AppendLine("Please analyze the following invoice details and determine if they are for home improvement expenses.");
            promptBuilder.AppendLine("Respond with a JSON object that includes a boolean 'isHomeImprovement' field, a 'confidence' score from 0-1, and a 'reasoning' field explaining your assessment.");
            promptBuilder.AppendLine("\nInvoice Details:");
            
            // Add vendor information
            promptBuilder.AppendLine($"Vendor: {invoice.VendorName}");
            
            // Add invoice date if available
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Date: {invoice.InvoiceDate.Value.ToString("yyyy-MM-dd")}");
            }
            
            // Add line items
            promptBuilder.AppendLine("\nItems:");
            foreach (var item in invoice.LineItems)
            {
                promptBuilder.AppendLine($"- {item.Description}: {item.Quantity} x €{item.UnitPrice:N2} = €{item.TotalPrice:N2}");
            }
            
            // Add total amount
            promptBuilder.AppendLine($"\nTotal Amount: €{invoice.TotalAmount:N2}");
            
            promptBuilder.AppendLine("\nThe invoice should be considered home improvement related if it includes materials, labor, or services typically used to improve, repair, renovate, or maintain a residential property. Examples include building materials, plumbing, electrical work, flooring, paint, tools for construction, contractor services, etc.");
            
            return promptBuilder.ToString();
        }
        
        private string BuildFraudDetectionPrompt(Invoice invoice)
        {
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("You are an expert in detecting potentially fraudulent invoices.");
            promptBuilder.AppendLine("Please analyze the following invoice details and determine if there are any indicators of fraud or suspicious activity.");
            promptBuilder.AppendLine("Respond with a JSON object that includes a boolean 'possibleFraud' field, a 'confidence' score from 0-1, and a 'reasoning' field explaining any suspicious elements.");
            promptBuilder.AppendLine("\nInvoice Details:");
            
            // Add invoice metadata
            promptBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
            promptBuilder.AppendLine($"Vendor: {invoice.VendorName}");
            
            // Add vendor information if available
            if (!string.IsNullOrEmpty(invoice.VendorKvkNumber))
            {
                promptBuilder.AppendLine($"KvK Number: {invoice.VendorKvkNumber}");
            }
            
            if (!string.IsNullOrEmpty(invoice.VendorBtwNumber))
            {
                promptBuilder.AppendLine($"BTW/VAT Number: {invoice.VendorBtwNumber}");
            }
            
            // Add invoice date if available
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Date: {invoice.InvoiceDate.Value.ToString("yyyy-MM-dd")}");
            }
            
            // Add line items
            promptBuilder.AppendLine("\nItems:");
            foreach (var item in invoice.LineItems)
            {
                promptBuilder.AppendLine($"- {item.Description}: {item.Quantity} x €{item.UnitPrice:N2} = €{item.TotalPrice:N2}");
            }
            
            // Add total amount and VAT
            promptBuilder.AppendLine($"\nTotal Amount: €{invoice.TotalAmount:N2}");
            promptBuilder.AppendLine($"VAT Amount: €{invoice.VatAmount:N2}");
            
            promptBuilder.AppendLine("\nPlease check for inconsistencies in the invoice, such as mismatched totals, suspicious item descriptions, unusual pricing, missing important information, or other indicators of potential fraud.");
            
            return promptBuilder.ToString();
        }
        
        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            _logger.LogInformation("Calling Gemini API");
            
            try
            {
                // Create client
                var client = new PredictionServiceClientBuilder
                {
                    Endpoint = $"{_location}-aiplatform.googleapis.com:443",
                }.Build();
                
                // Format the JSON for the request
                var requestContent = @"{
                    ""contents"": [
                        {
                            ""role"": ""user"",
                            ""parts"": [
                                {
                                    ""text"": """ + prompt.Replace("\"", "\\\"") + @"""
                                }
                            ]
                        }
                    ]
                }";
                
                var parametersContent = @"{
                    ""temperature"": 0.2,
                    ""maxOutputTokens"": 2048,
                    ""topP"": 0.8,
                    ""topK"": 40
                }";
                
                // Create the request
                var request = new PredictRequest
                {
                    Endpoint = $"projects/{_projectId}/locations/{_location}/publishers/google/models/{_modelId}",
                };
                
                // Add JSON content using Value class
                var instanceValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(requestContent);
                request.Instances.Add(instanceValue);
                
                var parametersValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(parametersContent);
                request.Parameters = parametersValue;
                
                // Make the request
                var response = await client.PredictAsync(request);
                
                // Extract the text content from the response
                var jsonResponse = response.Predictions[0].ToString();
                var responseObject = JsonDocument.Parse(jsonResponse).RootElement;
                
                var contentArray = responseObject.GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();
                
                return contentArray ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }
        
        private bool ParseHomeImprovementResponse(string responseText, ValidationResult result)
        {
            try
            {
                // Extract JSON from the response (in case Gemini returns additional text)
                var jsonMatch = System.Text.RegularExpressions.Regex.Match(responseText, @"\{.*\}");
                if (jsonMatch.Success)
                {
                    var jsonResponse = jsonMatch.Value;
                    var response = JsonSerializer.Deserialize<GeminiHomeImprovementResponse>(jsonResponse);
                    
                    if (response != null)
                    {
                        // Add reasoning as an info issue
                        result.AddIssue(ValidationSeverity.Info, 
                            $"Gemini assessment: {response.Reasoning} (Confidence: {response.Confidence:P0})");
                        
                        return response.IsHomeImprovement;
                    }
                }
                
                // If we can't parse JSON, check for keywords
                return responseText.Contains("home improvement", StringComparison.OrdinalIgnoreCase) && 
                       !responseText.Contains("not related to home improvement", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini home improvement response");
                // Default to true to avoid false negatives
                return true;
            }
        }
        
        private bool ParseFraudDetectionResponse(string responseText)
        {
            try
            {
                // Extract JSON from the response
                var jsonMatch = System.Text.RegularExpressions.Regex.Match(responseText, @"\{.*\}");
                if (jsonMatch.Success)
                {
                    var jsonResponse = jsonMatch.Value;
                    var response = JsonSerializer.Deserialize<GeminiFraudResponse>(jsonResponse);
                    
                    if (response != null)
                    {
                        _logger.LogInformation("Fraud detection result: {Result} with confidence {Confidence}. Reasoning: {Reasoning}", 
                            response.PossibleFraud, response.Confidence, response.Reasoning);
                        
                        return response.PossibleFraud;
                    }
                }
                
                // If we can't parse JSON, check for keywords
                return responseText.Contains("suspicious", StringComparison.OrdinalIgnoreCase) || 
                       responseText.Contains("fraud", StringComparison.OrdinalIgnoreCase) ||
                       responseText.Contains("inconsistent", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini fraud response");
                // Default to false to avoid false positives
                return false;
            }
        }
        
        private class GeminiHomeImprovementResponse
        {
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
        }
        
        private class GeminiFraudResponse
        {
            [JsonPropertyName("possibleFraud")]
            public bool PossibleFraud { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; } = string.Empty;
        }
    }
}
