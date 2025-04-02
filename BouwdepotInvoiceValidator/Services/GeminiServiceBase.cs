using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Represents a message in a conversation with the Gemini API
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// Role of the message sender (user or model)
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Content parts of the message (text and/or images)
        /// </summary>
        [JsonPropertyName("parts")]
        public List<object> Parts { get; set; } = new List<object>();
        
        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Represents a conversation context that maintains message history
    /// </summary>
    public class ConversationContext
    {
        /// <summary>
        /// Unique identifier for the conversation
        /// </summary>
        public string ConversationId { get; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// List of messages in the conversation
        /// </summary>
        public List<ConversationMessage> Messages { get; } = new List<ConversationMessage>();
        
        /// <summary>
        /// When the conversation was created
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the conversation was last updated
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Custom metadata for the conversation
        /// </summary>
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Add a message to the conversation
        /// </summary>
        public void AddMessage(string role, List<object> parts)
        {
            Messages.Add(new ConversationMessage
            {
                Role = role,
                Parts = parts,
                Timestamp = DateTime.UtcNow
            });
            
            LastUpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Clear all messages from the conversation
        /// </summary>
        public void ClearMessages()
        {
            Messages.Clear();
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
    /// <summary>
    /// Base class for Gemini API services providing common functionality
    /// </summary>
    public abstract class GeminiServiceBase
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly string _apiKey;
        protected readonly string _projectId;
        protected readonly string _location;
        protected readonly string _modelId;
        
        /// <summary>
        /// Maximum number of messages to keep in conversation history
        /// </summary>
        protected readonly int _maxHistoryMessages;
        
        /// <summary>
        /// Whether to use conversation history by default
        /// </summary>
        protected readonly bool _useConversationHistory;
        
        /// <summary>
        /// Timeout for conversations in minutes
        /// </summary>
        protected readonly int _conversationTimeoutMinutes;
        
        /// <summary>
        /// Current active conversation context
        /// </summary>
        protected ConversationContext _currentConversation;
        
        /// <summary>
        /// Dictionary of all conversation contexts by ID
        /// </summary>
        protected readonly Dictionary<string, ConversationContext> _conversations = new Dictionary<string, ConversationContext>();

        protected GeminiServiceBase(ILogger logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Load configuration
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key is not configured.");
            _projectId = _configuration["Gemini:ProjectId"] ?? "your-project-id";
            _location = _configuration["Gemini:Location"] ?? "us-central1";
            _modelId = _configuration["Gemini:ModelId"] ?? "gemini-flash-2.0";
            
            // Load conversation configuration with defaults
            _maxHistoryMessages = _configuration.GetValue("Gemini:MaxHistoryMessages", 10);
            _useConversationHistory = _configuration.GetValue("Gemini:UseConversationHistory", false);
            _conversationTimeoutMinutes = _configuration.GetValue("Gemini:ConversationTimeoutMinutes", 30);
            
            _logger.LogInformation("Initialized GeminiServiceBase with model: {ModelId} in location: {Location}", _modelId, _location);
            
            // Create initial conversation context
            StartNewConversation();
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        
        /// <summary>
        /// Starts a new conversation context, making it the current active conversation
        /// </summary>
        /// <param name="metadata">Optional metadata for the conversation</param>
        /// <returns>The ID of the new conversation</returns>
        public string StartNewConversation(Dictionary<string, string> metadata = null)
        {
            _currentConversation = new ConversationContext();
            
            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    _currentConversation.Metadata[kvp.Key] = kvp.Value;
                }
            }
            
            _conversations[_currentConversation.ConversationId] = _currentConversation;
            
            _logger.LogInformation("Started new conversation: {ConversationId}", _currentConversation.ConversationId);
            
            return _currentConversation.ConversationId;
        }
        
        /// <summary>
        /// Switches to an existing conversation context
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>True if the conversation was found, false otherwise</returns>
        public bool SwitchConversation(string conversationId)
        {
            if (_conversations.TryGetValue(conversationId, out var conversation))
            {
                if ((DateTime.UtcNow - conversation.LastUpdatedAt).TotalMinutes > _conversationTimeoutMinutes)
                {
                    _logger.LogWarning("Conversation {ConversationId} has timed out after {Timeout} minutes of inactivity", 
                        conversationId, _conversationTimeoutMinutes);
                    return false;
                }
                
                _currentConversation = conversation;
                _logger.LogInformation("Switched to conversation: {ConversationId}", conversationId);
                return true;
            }
            
            _logger.LogWarning("Conversation not found: {ConversationId}", conversationId);
            return false;
        }
        
        /// <summary>
        /// Clears the message history for the current conversation
        /// </summary>
        public void ClearCurrentConversation()
        {
            _currentConversation.ClearMessages();
            _logger.LogInformation("Cleared message history for conversation: {ConversationId}", 
                _currentConversation.ConversationId);
        }
        
        /// <summary>
        /// Gets the current conversation context
        /// </summary>
        /// <returns>The current conversation context</returns>
        public ConversationContext GetCurrentConversation()
        {
            return _currentConversation;
        }
        
        /// <summary>
        /// Calls the Gemini API with the provided prompt
        /// </summary>
        public async Task<string> CallGeminiApiAsync(string prompt, string operation = "Generic Operation")
        {
            return await CallGeminiApiAsync(prompt, null, operation, _useConversationHistory);
        }
        
        /// <summary>
        /// Calls the Gemini API with the provided prompt and optional image data
        /// </summary>
        public async Task<string> CallGeminiApiAsync(string prompt, List<InvoicePageImage> images = null, string operation = "Generic Operation", bool useConversationHistory = false)
        {
            _logger.LogInformation("Calling Gemini API directly for operation: {Operation} with {ImageCount} images, UseConversationHistory={UseHistory}", 
                operation, images?.Count ?? 0, useConversationHistory);
            
            // Log the full prompt for debugging
            _logger.LogDebug("GEMINI INPUT PROMPT:\n{Prompt}", prompt);
            
            try
            {
                // Determine which model to use
                // Only use vision model if there are actually images present
                string modelName = _modelId;
                
                // Construct the Gemini API URL - using the API version v1beta for newer models
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";
                
                _logger.LogDebug("API URL: {ApiUrl}", apiUrl.Replace(_apiKey, "[REDACTED]"));
                
                // Create parts for the user message
                var userParts = CreateParts(prompt, images);
                
                // Add the user message to conversation history if using history
                if (useConversationHistory)
                {
                    _currentConversation.AddMessage("user", userParts);
                }
                
                // Prepare contents array based on whether to use conversation history
                object requestObj;
                
                if (useConversationHistory && _currentConversation.Messages.Count > 1)
                {
                    // Limit history to last N messages to avoid exceeding token limits
                    var historyToUse = _currentConversation.Messages
                        .Skip(Math.Max(0, _currentConversation.Messages.Count - _maxHistoryMessages))
                        .ToArray();
                    
                    _logger.LogDebug("Using conversation history with {Count} messages", historyToUse.Length);
                    
                    requestObj = new 
                    {
                        contents = historyToUse,
                        generationConfig = new
                        {
                            temperature = 0.2,
                            maxOutputTokens = 2048,
                            topP = 0.8,
                            topK = 40
                        }
                    };
                }
                else
                {
                    // Use single message without history
                    requestObj = new 
                    {
                        contents = new[]
                        {
                            new
                            {
                                role = "user",
                                parts = userParts
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.2,
                            maxOutputTokens = 2048,
                            topP = 0.8,
                            topK = 40
                        }
                    };
                }
                
                // Convert request to JSON
                string jsonRequest = JsonSerializer.Serialize(requestObj, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
                
                // Measure response time
                var startTime = DateTime.UtcNow;
                
                // Create HTTP request
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                };
                
                // Log sending request
                _logger.LogDebug("Sending request to Gemini API at {StartTime}", startTime);
                
                // Send request
                var httpResponse = await _httpClient.SendAsync(httpRequest);
                
                // Calculate duration
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;
                
                // Ensure success
                httpResponse.EnsureSuccessStatusCode();
                
                // Read response
                string responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Received Gemini API response in {Duration}ms", duration.TotalMilliseconds);
                _logger.LogDebug("Raw Gemini API response: {RawResponse}", responseContent);
                
                // Parse the JSON response to extract text
                string extractedText = string.Empty;
                
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    // Extract text from the first candidate
                    var candidates = doc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var content = candidates[0].GetProperty("content");
                        var parts = content.GetProperty("parts");
                        if (parts.GetArrayLength() > 0)
                        {
                            if (parts[0].TryGetProperty("text", out var textElement))
                            {
                                extractedText = textElement.GetString() ?? string.Empty;
                                
                                // Log the response from Gemini
                                _logger.LogDebug("GEMINI OUTPUT RESPONSE:\n{Response}", extractedText);
                                
                                // If using conversation history, save the model's response
                                if (useConversationHistory)
                                {
                                    var textPart = new List<object> { new { text = extractedText } };
                                    _currentConversation.AddMessage("model", textPart);
                                }
                                
                                // Further sanitize the content for JSON processing
                                return SanitizeJsonString(extractedText);
                            }
                        }
                    }
                }
                
                throw new InvalidOperationException("Could not extract text from Gemini API response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for operation {Operation}: {ErrorMessage}", operation, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Creates the parts array for the Gemini API request with parallel processing for images
        /// </summary>
        private List<object> CreateParts(string prompt, List<InvoicePageImage> images)
        {
            var parts = new List<object>();
            
            // Sanitize prompt but preserve basic structure
            string sanitizedPrompt = prompt;
            
            // Add log to check what's happening with the prompt
            _logger.LogDebug("Original prompt length: {Length}", prompt.Length);
            
            // Add text part
            parts.Add(new { text = sanitizedPrompt });
            
            // Add image parts if available, processing them in parallel
            if (images != null && images.Count > 0)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation("Processing {Count} images for request", images.Count);
                
                // Process images in parallel to speed up encoding
                var imagePartsTask = ProcessImagesInParallel(images);
                // Wait for all image processing to complete
                var imageParts = imagePartsTask.GetAwaiter().GetResult();
                
                // Add all processed image parts to the request
                parts.AddRange(imageParts);
                
                stopwatch.Stop();
                _logger.LogInformation("Processed {Count} images in {ElapsedMs}ms", 
                    imageParts.Count, stopwatch.ElapsedMilliseconds);
            }
            
            return parts;
        }
        
        /// <summary>
        /// Processes images in parallel to improve performance
        /// </summary>
        private async Task<List<object>> ProcessImagesInParallel(List<InvoicePageImage> images)
        {
            var results = new List<object>();
            if (images == null || images.Count == 0)
                return results;
                
            // Process single images directly
            if (images.Count == 1)
            {
                var image = images[0];
                if (image.ImageData != null && image.ImageData.Length > 0)
                {
                    results.Add(CreateImagePart(image));
                    _logger.LogDebug("Added single image {PageNumber} to request, optimized size used", 
                        image.PageNumber);
                }
                return results;
            }
            
            // For multiple images, process in parallel
            _logger.LogDebug("Starting parallel processing of {Count} images", images.Count);
            
            // Create a list to hold the tasks
            var tasks = new List<Task<object>>();
            
            // Create tasks for each image - processing them in parallel
            foreach (var image in images.Where(img => img.ImageData != null && img.ImageData.Length > 0))
            {
                tasks.Add(Task.Run(() => CreateImagePart(image)));
            }
            
            // Wait for all tasks to complete
            var processedParts = await Task.WhenAll(tasks);
            
            // Add all parts to results
            results.AddRange(processedParts);
            
            _logger.LogDebug("Completed parallel processing of {Count} images", results.Count);
            
            return results;
        }
        
        /// <summary>
        /// Creates an image part for the Gemini API request
        /// </summary>
        private object CreateImagePart(InvoicePageImage image)
        {
            // Get the optimized, encoded image string
            string encodedImage = image.Base64EncodedImage;
            
            _logger.LogDebug("Processed image {PageNumber}, original size: {OriginalSize} bytes", 
                image.PageNumber, image.ImageData.Length);
                
            // Return the formatted image part
            return new
            {
                inline_data = new
                {
                    mime_type = "image/png",
                    data = encodedImage
                }
            };
        }
        
        /// <summary>
        /// Pre-sanitizes raw JSON strings to prevent Protobuf parsing errors
        /// </summary>
        protected string PreSanitizeJson(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
                return jsonText;
                
            // Replace problematic Unicode characters that cause Protobuf parsing errors
            // U+000D (carriage return) is particularly problematic in string literals
            _logger.LogDebug("Pre-sanitizing JSON response to remove problematic characters");
            
            var sanitized = jsonText
                .Replace("\r", "")  // Remove carriage returns completely
                .Replace("\\r", "") // Remove escaped carriage returns
                .Replace("\u000d", ""); // Explicitly remove Unicode carriage returns
                
            return sanitized;
        }
        
        /// <summary>
        /// Sanitizes JSON string content to ensure valid JSON for further processing
        /// </summary>
        protected string SanitizeJsonString(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
                return jsonText;
                
            _logger.LogDebug("Sanitizing JSON string of length {Length}", jsonText.Length);

            // Remove carriage returns and other problematic control characters
            var sanitized = jsonText
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("\\r", " ")
                .Replace("\\n", " ")
                .Replace("\u000d", " ") // Explicit Unicode carriage return
                .Replace("\u000a", " "); // Line feed
            
            // Try to extract valid JSON if surrounded by other text
            var jsonMatch = Regex.Match(sanitized, @"\{.*\}");
            if (jsonMatch.Success)
            {
                _logger.LogDebug("Extracted valid JSON object from response");
                return jsonMatch.Value;
            }
            
            _logger.LogWarning("Could not extract valid JSON object from response");
            return sanitized;
        }
        
        /// <summary>
        /// Extracts and sanitizes JSON from text for deserialization
        /// </summary>
        /// <returns>Deserialized object or null if deserialization fails</returns>
        protected T? ExtractAndDeserializeJson<T>(string text) where T : class
        {
            _logger.LogDebug("Attempting to extract and deserialize JSON of type {Type}", typeof(T).Name);
            
            try
            {
                // Sanitize the response text
                text = SanitizeJsonString(text);
                
                // Extract JSON from the response
                var jsonMatch = Regex.Match(text, @"\{.*\}");
                if (jsonMatch.Success)
                {
                    var jsonResponse = jsonMatch.Value;
                    // Apply additional sanitization
                    jsonResponse = PreSanitizeJson(jsonResponse);
                    
                    _logger.LogDebug("Extracted JSON: {ExtractedJson}", jsonResponse);
                    
                    var result = JsonSerializer.Deserialize<T>(jsonResponse);
                    if (result != null)
                    {
                        _logger.LogInformation("Successfully deserialized JSON to {Type}", typeof(T).Name);
                        return result;
                    }
                }
                
                _logger.LogWarning("Failed to deserialize JSON to {Type}", typeof(T).Name);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting and deserializing JSON: {ErrorMessage}", ex.Message);
                return null;
            }
        }
    }
}
