using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BouwdepotInvoiceValidator.Infrastructure.Ollama.Models; // Added using for models
using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Ollama
{
    /// <summary>
    /// Client for interacting with the Ollama API.
    /// </summary>
    internal class OllamaClient : ILLMProvider
    {
        private readonly ILogger<OllamaClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OllamaClient(
            ILogger<OllamaClient> logger,
            HttpClient httpClient,
            IOptions<OllamaOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Basic validation of options
            if (string.IsNullOrWhiteSpace(_options.ApiBaseUrl) || !Uri.TryCreate(_options.ApiBaseUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid ApiBaseUrl in OllamaOptions", nameof(options));
            if (string.IsNullOrWhiteSpace(_options.DefaultTextModel))
                throw new ArgumentException("DefaultTextModel is required in OllamaOptions", nameof(options));
            if (string.IsNullOrWhiteSpace(_options.DefaultMultimodalModel))
                throw new ArgumentException("DefaultMultimodalModel is required in OllamaOptions", nameof(options));

            // Configure the HTTP client
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
            }
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            // Configure JSON serializer options
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Adjust if Ollama uses snake_case
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Adjust serializer if Ollama uses snake_case (it often does)
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // Changed to SnakeCaseLower
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _logger.LogInformation("OllamaClient initialized. Base URL: {BaseUrl}, Text Model: {TextModel}, Multimodal Model: {MultimodalModel}",
                _options.ApiBaseUrl, _options.DefaultTextModel, _options.DefaultMultimodalModel);
        }

        public string GetTextModelName() => _options.DefaultTextModel;

        public string GetMultimodalModelName() => _options.DefaultMultimodalModel;

        public ConversationContext CreateConversationContext()
        {
            // Ollama's /api/chat endpoint supports context implicitly by passing the message history.
            // We can still use our ConversationContext class to manage this history externally.
            return new ConversationContext();
        }

        // --- Interface Methods Implementation ---

        public async Task<string> SendTextPromptAsync(string prompt, ConversationContext? conversationContext = null)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            // Add user message to context *before* sending, so it's included in the request
            conversationContext?.AddUserMessage(prompt, null); // Assuming stepName is null for simple text prompts

            return await SendAndProcessChatRequestAsync(
                modelName: GetTextModelName(),
                prompt: prompt,
                conversationContext: conversationContext,
                expectJsonResponse: false);
        }

        public async Task<string> SendMultimodalPromptAsync(
            string prompt,
            IEnumerable<Stream> imageStreams,
            IEnumerable<string> mimeTypes, // mimeTypes are not directly used by Ollama API, but we keep the signature
            ConversationContext? conversationContext = null)
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (imageStreams == null || !imageStreams.Any()) throw new ArgumentException("At least one image stream is required.", nameof(imageStreams));

            // Add user message to context *before* sending
            conversationContext?.AddUserMessage(prompt, null); // Assuming stepName is null

            return await SendAndProcessChatRequestAsync(
                modelName: GetMultimodalModelName(),
                prompt: prompt,
                imageStreams: imageStreams,
                conversationContext: conversationContext,
                expectJsonResponse: false);
        }

        public async Task<TResponse> SendStructuredPromptAsync<TResponse>(
            string prompt,
            ConversationContext? conversationContext = null,
            string? stepName = null)
            where TResponse : class, new()
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            // Add user message to context *before* sending
            conversationContext?.AddUserMessage(prompt, stepName);

            string responseText = await SendAndProcessChatRequestAsync(
                modelName: GetTextModelName(), // Use text model for structured JSON output
                prompt: prompt,
                conversationContext: conversationContext,
                expectJsonResponse: true); // Request JSON format

            responseText = CleanJsonResponseText(responseText);

            try
            {
                var structuredResponse = JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Ollama JSON response into {ResponseType}. Cleaned Response: {ResponseText}", typeof(TResponse).Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize Ollama response into {typeof(TResponse).Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error for {ResponseType}. Cleaned Response: {ResponseText}", typeof(TResponse).Name, responseText);
                throw new InvalidOperationException($"Failed to deserialize Ollama JSON response into {typeof(TResponse).Name}.", jsonEx);
            }
        }

        public async Task<object> SendStructuredPromptAsync(
            Type responseType,
            string prompt,
            ConversationContext? conversationContext = null,
            string? stepName = null)
        {
            if (responseType == null) throw new ArgumentNullException(nameof(responseType));
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            // Add user message to context *before* sending
            conversationContext?.AddUserMessage(prompt, stepName);

            string responseText = await SendAndProcessChatRequestAsync(
                modelName: GetTextModelName(),
                prompt: prompt,
                conversationContext: conversationContext,
                expectJsonResponse: true);

            responseText = CleanJsonResponseText(responseText);

            try
            {
                var structuredResponse = JsonSerializer.Deserialize(responseText, responseType, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Ollama JSON response into {ResponseType}. Cleaned Response: {ResponseText}", responseType.Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize Ollama response into {responseType.Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error for {ResponseType}. Cleaned Response: {ResponseText}", responseType.Name, responseText);
                throw new InvalidOperationException($"Failed to deserialize Ollama JSON response into {responseType.Name}.", jsonEx);
            }
        }

        public async Task<TResponse> SendMultimodalStructuredPromptAsync<TResponse>(
            string prompt,
            IEnumerable<Stream> imageStreams,
            IEnumerable<string> mimeTypes, // Not used by Ollama API
            ConversationContext? conversationContext = null,
            string? stepName = null)
            where TResponse : class, new()
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (imageStreams == null || !imageStreams.Any()) throw new ArgumentException("At least one image stream is required.", nameof(imageStreams));

            // Add user message to context *before* sending
            conversationContext?.AddUserMessage(prompt, stepName);

            string responseText = await SendAndProcessChatRequestAsync(
                modelName: GetMultimodalModelName(), // Use multimodal model
                prompt: prompt,
                imageStreams: imageStreams,
                conversationContext: conversationContext,
                expectJsonResponse: true); // Request JSON format

            responseText = CleanJsonResponseText(responseText);

            try
            {
                var structuredResponse = JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Ollama multimodal JSON response into {ResponseType}. Cleaned Response: {ResponseText}", typeof(TResponse).Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize Ollama multimodal response into {typeof(TResponse).Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error for multimodal {ResponseType}. Cleaned Response: {ResponseText}", typeof(TResponse).Name, responseText);
                throw new InvalidOperationException($"Failed to deserialize Ollama multimodal JSON response into {typeof(TResponse).Name}.", jsonEx);
            }
        }

        public async Task<object> SendMultimodalStructuredPromptAsync(
            Type responseType,
            string prompt,
            IEnumerable<Stream> imageStreams,
            IEnumerable<string> mimeTypes, // Not used by Ollama API
            ConversationContext? conversationContext = null,
            string? stepName = null)
        {
            if (responseType == null) throw new ArgumentNullException(nameof(responseType));
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (imageStreams == null || !imageStreams.Any()) throw new ArgumentException("At least one image stream is required.", nameof(imageStreams));

            // Add user message to context *before* sending
            conversationContext?.AddUserMessage(prompt, stepName);

            string responseText = await SendAndProcessChatRequestAsync(
                modelName: GetMultimodalModelName(),
                prompt: prompt,
                imageStreams: imageStreams,
                conversationContext: conversationContext,
                expectJsonResponse: true);

            responseText = CleanJsonResponseText(responseText);

            try
            {
                var structuredResponse = JsonSerializer.Deserialize(responseText, responseType, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Ollama multimodal JSON response into {ResponseType}. Cleaned Response: {ResponseText}", responseType.Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize Ollama multimodal response into {responseType.Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error for multimodal {ResponseType}. Cleaned Response: {ResponseText}", responseType.Name, responseText);
                throw new InvalidOperationException($"Failed to deserialize Ollama multimodal JSON response into {responseType.Name}.", jsonEx);
            }
        }

        // --- Helper Methods ---

        private async Task<string> ConvertStreamToBase64Async(Stream stream)
        {
            if (!stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(stream));
            if (stream.CanSeek) stream.Position = 0; // Reset stream position if possible
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private string CleanJsonResponseText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            text = text.Trim();
            // Remove potential markdown code blocks
            if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) text = text.Substring(7).TrimStart();
            else if (text.StartsWith("```")) text = text.Substring(3).TrimStart();
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3).TrimEnd();
            return text;
        }

        private async Task<string> SendAndProcessChatRequestAsync(
            string modelName,
            string prompt,
            IEnumerable<Stream>? imageStreams = null,
            ConversationContext? conversationContext = null,
            bool expectJsonResponse = false)
        {
            string apiUrl = "/api/chat"; // Relative URL for the chat endpoint
            _logger.LogInformation("Sending request to Ollama API [{ModelName}] at {ApiUrl}. Expecting JSON: {ExpectJson}", modelName, apiUrl, expectJsonResponse);

            var request = new Models.OllamaChatRequest
            {
                Model = modelName,
                Stream = false, // We handle the full response
                Format = expectJsonResponse ? "json" : null,
                KeepAlive = _options.KeepAlive,
                Options = new Models.OllamaRequestOptions
                {
                    Temperature = _options.Temperature,
                    TopP = _options.TopP,
                    TopK = _options.TopK
                    // Add other options if needed
                }
            };

            // Build message history from context
            if (conversationContext != null)
            {
                foreach (var message in conversationContext.Messages)
                {
                    // Assuming ConversationContext stores roles compatible with Ollama ("user", "assistant")
                    request.Messages.Add(new Models.OllamaChatMessage { Role = message.Role, Content = message.Content });
                }
            }

            // Prepare current user message
            var currentUserMessage = new Models.OllamaChatMessage { Role = "user", Content = prompt };

            // Add images if provided
            if (imageStreams != null)
            {
                currentUserMessage.Images = new List<string>();
                foreach (var stream in imageStreams)
                {
                    currentUserMessage.Images.Add(await ConvertStreamToBase64Async(stream));
                }
                _logger.LogInformation("Included {ImageCount} images in the request.", currentUserMessage.Images.Count);
            }

            request.Messages.Add(currentUserMessage);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonSerializerOptions, CancellationToken.None);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ollama API request failed with status code {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
                    // Try parsing Ollama's specific error format
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Models.OllamaChatResponse>(errorContent, _jsonSerializerOptions);
                        if (!string.IsNullOrWhiteSpace(errorResponse?.Error))
                        {
                            throw new HttpRequestException($"Ollama API error: {errorResponse.Error}", null, response.StatusCode);
                        }
                    } catch (JsonException) { /* Ignore if error content is not the expected JSON */ }
                    // Fallback generic error
                    response.EnsureSuccessStatusCode(); // This will throw HttpRequestException
                }

                var ollamaResponse = await response.Content.ReadFromJsonAsync<Models.OllamaChatResponse>(_jsonSerializerOptions);

                if (ollamaResponse == null || !ollamaResponse.Done || ollamaResponse.Message == null || string.IsNullOrEmpty(ollamaResponse.Message.Content))
                {
                    string rawResponse = await response.Content.ReadAsStringAsync(); // Re-read as string for logging
                    _logger.LogError("Ollama API response was incomplete or invalid. Raw response: {RawResponse}", rawResponse);
                    throw new InvalidOperationException("Ollama API response was incomplete or invalid.");
                }

                // Log usage stats if available
                if (ollamaResponse.PromptEvalCount.HasValue && ollamaResponse.EvalCount.HasValue)
                {
                    _logger.LogInformation("Ollama API Usage - Prompt Tokens: {PromptTokens}, Completion Tokens: {CompletionTokens}, Total Tokens: {TotalTokens}",
                        ollamaResponse.PromptEvalCount, ollamaResponse.EvalCount, ollamaResponse.PromptEvalCount + ollamaResponse.EvalCount);
                }

                string responseText = ollamaResponse.Message.Content;

                // Add assistant response to conversation context
                if (conversationContext != null)
                {
                    // Assuming ConversationContext has a method like AddAssistantMessage or similar
                    conversationContext.AddModelMessage(responseText); // Use the existing method name
                }

                return responseText;

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error sending request to Ollama API.");
                throw; // Re-throw preserving stack trace
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing Ollama API response.");
                throw new InvalidOperationException("Failed to deserialize Ollama API response.", jsonEx);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken == CancellationToken.None)
            {
                _logger.LogError(ex, "Ollama API call timed out after {Timeout} seconds.", _options.TimeoutSeconds);
                throw new TimeoutException($"Ollama API call timed out after {_options.TimeoutSeconds} seconds.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error interacting with Ollama API.");
                throw;
            }
        }

        // --- Interface Methods Implementation (To be filled using helpers) ---
        // ... (Existing stubs remain for now) ...
    }
}
