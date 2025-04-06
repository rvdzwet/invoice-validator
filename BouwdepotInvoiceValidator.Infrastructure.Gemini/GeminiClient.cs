using BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models;
using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google
{
    /// <summary>
    /// Client for interacting with the Gemini API
    /// </summary>
    internal class GeminiClient : ILLMProvider // Implementing the exact interface
    {
        private readonly ILogger<GeminiClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        // Constructor remains the same
        public GeminiClient(
            ILogger<GeminiClient> logger,
            HttpClient httpClient,
            IOptions<GeminiOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Basic validation of options (more complex validation can be added)
            if (string.IsNullOrWhiteSpace(_options.ApiBaseUrl) || !Uri.TryCreate(_options.ApiBaseUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid ApiBaseUrl in GeminiOptions", nameof(options));
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new ArgumentException("ApiKey is required in GeminiOptions", nameof(options));
            if (string.IsNullOrWhiteSpace(_options.DefaultTextModel))
                throw new ArgumentException("DefaultTextModel is required in GeminiOptions", nameof(options));
            if (string.IsNullOrWhiteSpace(_options.DefaultMultimodalModel))
                throw new ArgumentException("DefaultMultimodalModel is required in GeminiOptions", nameof(options));

            // Configure the HTTP client
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
            }
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            // Configure JSON serializer options
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        // Helper methods (GetApiUrl, ConvertStreamToBase64Async, ProcessApiResponseAsync, HandleApiErrorAsync, CleanJsonResponseText)
        // remain the same as in the previous version, but internal calls will use CancellationToken.None where needed.

        private string GetApiUrl(string modelName)
        {
            string baseUrl = _options.ApiBaseUrl.TrimEnd('/') + "/";
            string relativeUrl = $"models/{modelName}:generateContent";
            if (_httpClient.BaseAddress != null)
            {
                return relativeUrl;
            }
            else
            {
                return new Uri(new Uri(baseUrl), relativeUrl).ToString();
            }
        }

        private async Task<string> ConvertStreamToBase64Async(Stream stream)
        {
            if (!stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(stream));
            if (stream.CanSeek) stream.Position = 0;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        // Modified ProcessApiResponseAsync to accept CancellationToken internally
        private async Task<string> ProcessApiResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                await HandleApiErrorAsync(response, cancellationToken); // Pass token
            }

            try
            {
                var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(_jsonSerializerOptions, cancellationToken); // Pass token

                var firstCandidate = geminiResponse?.Candidates?.FirstOrDefault();
                var firstPart = firstCandidate?.Content?.Parts?.FirstOrDefault();

                if (!string.IsNullOrEmpty(firstPart?.Text))
                {
                    if (firstCandidate?.FinishReason != null && firstCandidate.FinishReason != "STOP")
                    {
                        _logger.LogWarning("Gemini API call finished with reason: {FinishReason}", firstCandidate.FinishReason);
                    }
                    if (firstCandidate?.SafetyRatings?.Any(sr => sr.Probability != "NEGLIGIBLE" && sr.Probability != "LOW") ?? false)
                    {
                        _logger.LogWarning("Gemini response flagged with safety concerns: {SafetyRatings}", firstCandidate.SafetyRatings);
                    }
                    return firstPart.Text;
                }
                else
                {
                    string rawContent = await response.Content.ReadAsStringAsync(cancellationToken); // Pass token
                    _logger.LogError("Gemini API response did not contain the expected text part. Raw response: {RawResponse}", rawContent);
                    throw new InvalidOperationException("Gemini API response did not contain the expected text part.");
                }
            }
            catch (JsonException jsonEx)
            {
                string rawContent = "Failed to read raw content after JsonException.";
                try { rawContent = await response.Content.ReadAsStringAsync(cancellationToken); } // Pass token, try reading
                catch { /* Ignore secondary exception */ }
                _logger.LogError(jsonEx, "Failed to deserialize Gemini API response. Raw content: {RawContent}", rawContent);
                throw new InvalidOperationException("Failed to deserialize Gemini API response.", jsonEx);
            }
        }

        // Modified HandleApiErrorAsync to accept CancellationToken internally
        private async Task HandleApiErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string errorContent = "Failed to read error content.";
            try { errorContent = await response.Content.ReadAsStringAsync(cancellationToken); } // Pass token
            catch { /* Ignore secondary exception */ }

            string errorMessage = $"Gemini API request failed with status code {response.StatusCode}.";

            try
            {
                var errorResponse = JsonSerializer.Deserialize<GeminiApiErrorResponse>(errorContent, _jsonSerializerOptions);
                if (!string.IsNullOrWhiteSpace(errorResponse?.Error?.Message))
                {
                    errorMessage += $" Error: {errorResponse.Error.Message} (Status: {errorResponse.Error.Status}, Code: {errorResponse.Error.Code})";
                }
                else { errorMessage += $" Raw error content: {errorContent}"; }
            }
            catch (JsonException) { errorMessage += $" Raw error content: {errorContent}"; }

            _logger.LogError(errorMessage);
            throw new HttpRequestException(errorMessage);
        }

        private string CleanJsonResponseText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            text = text.Trim();
            if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) text = text.Substring(7).TrimStart();
            else if (text.StartsWith("```")) text = text.Substring(3).TrimStart();
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3).TrimEnd();
            return text;
        }


        // --- Interface Methods Implementation (Corrected Signatures) ---

        /// <inheritdoc/>
        public async Task<string> SendTextPromptAsync(string prompt) // No CancellationToken
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            string modelName = _options.DefaultTextModel;
            string apiUrl = GetApiUrl(modelName);
            _logger.LogInformation("Sending text prompt to Gemini API [{ModelName}] at {ApiUrl}", modelName, apiUrl);

            var request = new GeminiRequest { Contents = new List<Content> { new Content { Parts = new List<Part> { new Part { Text = prompt } } } } };

            try
            {
                // Using CancellationToken.None for the internal async call
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonSerializerOptions, CancellationToken.None);
                return await ProcessApiResponseAsync(response, CancellationToken.None); // Pass None
            }
            catch (HttpRequestException httpEx) { _logger.LogError(httpEx, "HTTP error sending text prompt to Gemini API."); throw; }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken == CancellationToken.None) // Check if timeout related since no external token
            {
                _logger.LogError(ex, "Gemini API call timed out after {Timeout} seconds (TaskCanceledException).", _options.TimeoutSeconds);
                throw new TimeoutException($"Gemini API call timed out after {_options.TimeoutSeconds} seconds.", ex);
            }
            // No specific catch for external cancellation needed here
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error sending text prompt to Gemini API."); throw; }
        }

        /// <inheritdoc/>
        public async Task<string> SendMultimodalPromptAsync(
            string prompt,
            IEnumerable<Stream> imageStreams,
            IEnumerable<string> mimeTypes) // No CancellationToken
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (imageStreams == null) throw new ArgumentNullException(nameof(imageStreams));
            if (mimeTypes == null) throw new ArgumentNullException(nameof(mimeTypes));

            var imageStreamList = imageStreams.ToList();
            var mimeTypeList = mimeTypes.ToList();

            if (imageStreamList.Count != mimeTypeList.Count) throw new ArgumentException("Number of image streams must match number of MIME types.");
            if (!imageStreamList.Any()) throw new ArgumentException("At least one image stream is required for multimodal prompts.", nameof(imageStreams));

            string modelName = _options.DefaultMultimodalModel;
            string apiUrl = GetApiUrl(modelName);
            _logger.LogInformation("Sending multimodal prompt ({ImageCount} images) to Gemini API [{ModelName}] at {ApiUrl}", imageStreamList.Count, modelName, apiUrl);

            var parts = new List<Part> { new Part { Text = prompt } };
            for (int i = 0; i < imageStreamList.Count; i++)
            {
                string base64Image = await ConvertStreamToBase64Async(imageStreamList[i]); // This doesn't take a token
                parts.Add(new Part { InlineData = new InlineData { MimeType = mimeTypeList[i], Data = base64Image } });
            }
            var request = new GeminiRequest { Contents = new List<Content> { new Content { Parts = parts } } };

            try
            {
                // Using CancellationToken.None for the internal async call
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonSerializerOptions, CancellationToken.None);
                return await ProcessApiResponseAsync(response, CancellationToken.None); // Pass None
            }
            catch (HttpRequestException httpEx) { _logger.LogError(httpEx, "HTTP error sending multimodal prompt to Gemini API."); throw; }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken == CancellationToken.None)
            {
                _logger.LogError(ex, "Gemini API call timed out after {Timeout} seconds (TaskCanceledException).", _options.TimeoutSeconds);
                throw new TimeoutException($"Gemini API call timed out after {_options.TimeoutSeconds} seconds.", ex);
            }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error sending multimodal prompt to Gemini API."); throw; }
        }

        /// <inheritdoc/>
        public async Task<TResponse> SendStructuredPromptAsync<TRequest, TResponse>(
            string prompt,
            TRequest requestObject) // No CancellationToken
            where TRequest : class
            where TResponse : class, new() // Added 'new()' constraint back
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (requestObject == null) throw new ArgumentNullException(nameof(requestObject));

            string requestJson = JsonSerializer.Serialize(requestObject, _jsonSerializerOptions);
            string fullPrompt = $"{prompt}\n\nInput data:\n```json\n{requestJson}\n```\n\nPlease provide the response formatted as a JSON object matching the expected structure.";

            string modelName = _options.DefaultTextModel;
            string apiUrl = GetApiUrl(modelName);
            _logger.LogInformation("Sending structured prompt to Gemini API [{ModelName}] at {ApiUrl}", modelName, apiUrl);

            var request = new GeminiRequest { Contents = new List<Content> { new Content { Parts = new List<Part> { new Part { Text = fullPrompt } } } } };

            try
            {
                // Using CancellationToken.None for the internal async calls
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonSerializerOptions, CancellationToken.None);
                string responseText = await ProcessApiResponseAsync(response, CancellationToken.None); // Pass None
                responseText = CleanJsonResponseText(responseText);

                var structuredResponse = JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Gemini response text into {ResponseType}. Response text: {ResponseText}", typeof(TResponse).Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize response into {typeof(TResponse).Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx) { _logger.LogError(jsonEx, "Failed to deserialize structured response from Gemini API text output into {ResponseType}. Ensure the model returned valid JSON.", typeof(TResponse).Name); throw new InvalidOperationException($"Failed to deserialize response into {typeof(TResponse).Name}.", jsonEx); }
            catch (HttpRequestException httpEx) { _logger.LogError(httpEx, "HTTP error sending structured prompt to Gemini API."); throw; }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken == CancellationToken.None)
            {
                _logger.LogError(ex, "Gemini API call timed out after {Timeout} seconds (TaskCanceledException).", _options.TimeoutSeconds);
                throw new TimeoutException($"Gemini API call timed out after {_options.TimeoutSeconds} seconds.", ex);
            }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error sending structured prompt to Gemini API."); throw; }
        }


        /// <inheritdoc/>
        public async Task<TResponse> SendMultimodalStructuredPromptAsync<TRequest, TResponse>(
            string prompt,
            TRequest requestObject,
            IEnumerable<Stream> imageStreams,
            IEnumerable<string> mimeTypes) // No CancellationToken
            where TRequest : class
            where TResponse : class, new() // Added 'new()' constraint back
        {
            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));
            if (requestObject == null) throw new ArgumentNullException(nameof(requestObject));
            if (imageStreams == null) throw new ArgumentNullException(nameof(imageStreams));
            if (mimeTypes == null) throw new ArgumentNullException(nameof(mimeTypes));

            var imageStreamList = imageStreams.ToList();
            var mimeTypeList = mimeTypes.ToList();

            if (imageStreamList.Count != mimeTypeList.Count) throw new ArgumentException("Number of image streams must match number of MIME types.");

            string modelName = _options.DefaultMultimodalModel;
            string apiUrl = GetApiUrl(modelName);
            _logger.LogInformation("Sending multimodal structured prompt ({ImageCount} images) to Gemini API [{ModelName}] at {ApiUrl}", imageStreamList.Count, modelName, apiUrl);

            string requestJson = JsonSerializer.Serialize(requestObject, _jsonSerializerOptions);
            string fullPrompt = $"{prompt}\n\nInput data:\n```json\n{requestJson}\n```\n\nPlease provide the response formatted as a JSON object matching the expected structure.";

            var parts = new List<Part> { new Part { Text = fullPrompt } };
            for (int i = 0; i < imageStreamList.Count; i++)
            {
                string base64Image = await ConvertStreamToBase64Async(imageStreamList[i]); // No token
                parts.Add(new Part { InlineData = new InlineData { MimeType = mimeTypeList[i], Data = base64Image } });
            }
            var request = new GeminiRequest { Contents = new List<Content> { new Content { Parts = parts } } };

            try
            {
                // Using CancellationToken.None for the internal async calls
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonSerializerOptions, CancellationToken.None);
                string responseText = await ProcessApiResponseAsync(response, CancellationToken.None); // Pass None
                responseText = CleanJsonResponseText(responseText);

                var structuredResponse = JsonSerializer.Deserialize<TResponse>(responseText, _jsonSerializerOptions);
                if (structuredResponse == null)
                {
                    _logger.LogError("Failed to deserialize Gemini response text into {ResponseType}. Response text: {ResponseText}", typeof(TResponse).Name, responseText);
                    throw new InvalidOperationException($"Could not deserialize response into {typeof(TResponse).Name}.");
                }
                return structuredResponse;
            }
            catch (JsonException jsonEx) { _logger.LogError(jsonEx, "Failed to deserialize structured response from Gemini API text output into {ResponseType}. Ensure the model returned valid JSON.", typeof(TResponse).Name); throw new InvalidOperationException($"Failed to deserialize response into {typeof(TResponse).Name}.", jsonEx); }
            catch (HttpRequestException httpEx) { _logger.LogError(httpEx, "HTTP error sending multimodal structured prompt to Gemini API."); throw; }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken == CancellationToken.None)
            {
                _logger.LogError(ex, "Gemini API call timed out after {Timeout} seconds (TaskCanceledException).", _options.TimeoutSeconds);
                throw new TimeoutException($"Gemini API call timed out after {_options.TimeoutSeconds} seconds.", ex);
            }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error sending multimodal structured prompt to Gemini API."); throw; }
        }
    }
}