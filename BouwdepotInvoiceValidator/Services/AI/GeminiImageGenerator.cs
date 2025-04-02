using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Service for generating images using Google's Gemini AI
    /// </summary>
    public class GeminiImageGenerator
    {
        private readonly ILogger<GeminiImageGenerator> _logger;
        private readonly string _apiKey;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxParallelRequests;
        private readonly HttpClient _httpClient;
        private readonly int _timeoutSeconds;
        private readonly IMemoryCache _imageCache;
        private readonly int _imageCacheExpirationMinutes;
        private readonly string _diskCachePath;
        private readonly bool _enableDiskCache;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly int _defaultImageQuality;
        private readonly bool _enableSmartSizing;

        /// <summary>
        /// Creates a new instance of the GeminiImageGenerator
        /// </summary>
        public GeminiImageGenerator(
            ILogger<GeminiImageGenerator> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            
            // Load configuration
            var geminiConfig = configuration.GetSection("Gemini");
            _apiKey = geminiConfig["ApiKey"] ?? throw new ArgumentException("Gemini API key is required");
            _maxParallelRequests = geminiConfig.GetValue<int>("MaxParallelRequests", 5); // Increased from 3 to 5
            _timeoutSeconds = geminiConfig.GetValue<int>("ImageGenerationTimeoutSeconds", 25); // Reduced from 30 to 25
            
            // Enhanced caching configuration
            var cacheConfig = configuration.GetSection("Caching") ?? configuration.GetSection("AI:Caching");
            _imageCacheExpirationMinutes = cacheConfig?.GetValue<int>("ImageCacheExpirationMinutes") ?? 60;
            _enableDiskCache = cacheConfig?.GetValue<bool>("EnableDiskCache") ?? true;
            _diskCachePath = cacheConfig?.GetValue<string>("DiskCachePath") ?? "Cache/Images";
            _defaultImageQuality = cacheConfig?.GetValue<int>("DefaultImageQuality") ?? 85;
            _enableSmartSizing = cacheConfig?.GetValue<bool>("EnableSmartSizing") ?? true;
            
            // Ensure disk cache directory exists if enabled
            if (_enableDiskCache)
            {
                Directory.CreateDirectory(_diskCachePath);
            }
            
            // Initialize the semaphore to control parallel requests
            _semaphore = new SemaphoreSlim(_maxParallelRequests, _maxParallelRequests);
            
            // Initialize HTTP client with optimized settings
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep connection alive for performance
            
            // Initialize memory cache with options
            var cacheOptions = new MemoryCacheOptions
            {
                SizeLimit = 100 * 1024 * 1024, // 100 MB size limit
                ExpirationScanFrequency = TimeSpan.FromMinutes(10) // Scan every 10 minutes for expired items
            };
            _imageCache = new MemoryCache(cacheOptions);
            
            // Setup Polly retry policy for resilience
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3, // Number of retries
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Retry {RetryCount} after {RetryTimeSpan}s delay due to: {ExceptionMessage}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception.Message);
                    });
            
            _logger.LogInformation(
                "Enhanced GeminiImageGenerator initialized with max parallel requests: {MaxParallelRequests}, " +
                "timeout: {TimeoutSeconds}s, caching: {CacheEnabled}, disk cache: {DiskCacheEnabled}",
                _maxParallelRequests,
                _timeoutSeconds,
                true,
                _enableDiskCache);
        }

        /// <summary>
        /// Generates an image from text prompt with optimizations for speed and quality
        /// </summary>
        /// <param name="prompt">The text prompt to generate the image from</param>
        /// <param name="width">Width of the generated image</param>
        /// <param name="height">Height of the generated image</param>
        /// <param name="useCache">Whether to use the cache for this request</param>
        /// <param name="priority">Priority of the request (higher values get processed first)</param>
        /// <returns>The generated image as a byte array</returns>
        public async Task<byte[]> GenerateImageFromTextAsync(
            string prompt, 
            int width = 1024, 
            int height = 1024,
            bool useCache = true,
            int priority = 0)
        {
            // Optimize dimensions if smart sizing is enabled
            if (_enableSmartSizing)
            {
                OptimizeImageDimensions(ref width, ref height);
            }
            
            // Create a cache key from prompt and dimensions
            string cacheKey = $"{prompt}_{width}_{height}";
            string diskCacheFilePath = _enableDiskCache ? Path.Combine(_diskCachePath, GetSHA256Hash(cacheKey) + ".png") : null;
            
            // Check memory cache first if enabled
            if (useCache)
            {
                // Try memory cache first (fastest)
                if (_imageCache.TryGetValue(cacheKey, out byte[] cachedImage))
                {
                    _logger.LogDebug("Using memory-cached image for prompt: {PromptPreview}", GetPromptPreview(prompt));
                    return cachedImage;
                }
                
                // Then try disk cache if enabled
                if (_enableDiskCache && File.Exists(diskCacheFilePath))
                {
                    try
                    {
                        byte[] diskCachedImage = await File.ReadAllBytesAsync(diskCacheFilePath);
                        
                        // Cache in memory for future requests
                        CacheImageInMemory(cacheKey, diskCachedImage);
                        
                        _logger.LogDebug("Using disk-cached image for prompt: {PromptPreview}", GetPromptPreview(prompt));
                        return diskCachedImage;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading image from disk cache, will generate new image");
                        // Continue with generation if disk cache read fails
                    }
                }
            }
            
            // Prepare cancellation token with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            
            // Track request start time for performance monitoring
            var startTime = DateTime.UtcNow;
            
            // Optimize prompt for better performance
            string optimizedPrompt = OptimizePrompt(prompt);
            
            try
            {
                // Wait for a semaphore slot to become available with timeout
                // Use CancellationToken to avoid deadlocks
                if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(10), cts.Token))
                {
                    _logger.LogWarning("Timeout waiting for semaphore slot, too many concurrent requests");
                    throw new TimeoutException("Timeout waiting for available processing slot");
                }
                
                _logger.LogInformation(
                    "Generating image (priority: {Priority}, size: {Width}x{Height}): {PromptPreview}",
                    priority,
                    width, 
                    height, 
                    GetPromptPreview(optimizedPrompt));
                
                // Execute with retry policy
                byte[] imageData = await _retryPolicy.ExecuteAsync(async () => 
                {
                    // Use the Gemini image generation API
                    var requestUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent";
                    requestUrl += $"?key={_apiKey}";
                    
                    var requestData = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new
                                    {
                                        text = optimizedPrompt
                                    }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.3, // Lower temperature for faster generation and consistent results
                            topP = 1,
                            maxOutputTokens = 2048,
                            responseMimeType = "image/png",
                            imageParameters = new
                            {
                                width = width,
                                height = height,
                                quality = _defaultImageQuality // Control image quality to balance size and quality
                            }
                        },
                        // Provide priority hint to the API if supported
                        priority = priority > 0 ? "high" : "normal"
                    };
                    
                    var requestJson = JsonSerializer.Serialize(requestData);
                    var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    
                    // Use HTTP compression to speed up transfer
                    _httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, br");
                    
                    var response = await _httpClient.PostAsync(requestUrl, requestContent, cts.Token);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            "Error from Gemini API: {StatusCode}, {Content}", 
                            response.StatusCode, 
                            errorContent);
                        
                        throw new Exception($"Gemini API returned error status: {response.StatusCode}");
                    }
                    
                    var responseContent = await response.Content.ReadAsByteArrayAsync();
                    
                    // In a real implementation, we would parse the response to extract the image content
                    // This mock implementation returns a placeholder image
                    // For demonstration, we'll create a mock image of the requested dimensions
                    
                    // MOCK: Create a placeholder image - in production code this would be the actual image from Gemini
                    return GeneratePlaceholderImage(width, height);
                });
                
                // Calculate and log the generation time
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Image generation completed in {Duration}ms (size: {Width}x{Height}, {ImageSize}KB)",
                    duration.TotalMilliseconds,
                    width,
                    height,
                    (int)(imageData.Length / 1024));
                
                // Cache the result if caching is enabled
                if (useCache)
                {
                    // Cache in memory
                    CacheImageInMemory(cacheKey, imageData);
                    
                    // Cache to disk if enabled
                    if (_enableDiskCache)
                    {
                        // Write to disk asynchronously to not block the response
                        _ = Task.Run(async () => 
                        {
                            try 
                            {
                                await File.WriteAllBytesAsync(diskCacheFilePath, imageData);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to write image to disk cache");
                            }
                        });
                    }
                }
                
                return imageData;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Image generation timed out after {Timeout} seconds for prompt: {PromptPreview}",
                    _timeoutSeconds,
                    GetPromptPreview(prompt));
                
                throw new TimeoutException($"Image generation timed out after {_timeoutSeconds} seconds");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error generating image from text prompt: {PromptPreview}", 
                    GetPromptPreview(prompt));
                
                throw;
            }
            finally
            {
                // Always release the semaphore
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// Generates multiple images in parallel with optimized batch processing and priority queueing
        /// </summary>
        /// <param name="prompts">List of text prompts</param>
        /// <param name="width">Width of the generated images</param>
        /// <param name="height">Height of the generated images</param>
        /// <param name="useCache">Whether to use the cache for these requests</param>
        /// <param name="batchSize">Number of images to process in a single API call if supported</param>
        /// <returns>Dictionary mapping prompts to their generated images</returns>
        public async Task<Dictionary<string, byte[]>> GenerateImagesInParallelAsync(
            IEnumerable<string> prompts,
            int width = 1024,
            int height = 1024,
            bool useCache = true,
            int batchSize = 4)
        {
            // Convert to list for easier manipulation
            var promptList = prompts.ToList();
            
            _logger.LogInformation(
                "Starting optimized parallel image generation for {Count} prompts (max concurrency: {MaxConcurrency}, batch size: {BatchSize})",
                promptList.Count,
                _maxParallelRequests,
                batchSize);
            
            var startTime = DateTime.UtcNow;
            var result = new ConcurrentDictionary<string, byte[]>();
            
            // Check cache first for all prompts to avoid unnecessary API calls
            if (useCache)
            {
                int cacheHits = 0;
                foreach (var prompt in promptList.ToList()) // Create a copy to safely remove items
                {
                    string cacheKey = $"{prompt}_{width}_{height}";
                    
                    // Check memory cache
                    if (_imageCache.TryGetValue(cacheKey, out byte[] cachedImage))
                    {
                        result[prompt] = cachedImage;
                        promptList.Remove(prompt);
                        cacheHits++;
                        continue;
                    }
                    
                    // Check disk cache if enabled
                    if (_enableDiskCache)
                    {
                        string diskCacheFilePath = Path.Combine(_diskCachePath, GetSHA256Hash(cacheKey) + ".png");
                        if (File.Exists(diskCacheFilePath))
                        {
                            try
                            {
                                byte[] diskCachedImage = await File.ReadAllBytesAsync(diskCacheFilePath);
                                result[prompt] = diskCachedImage;
                                
                                // Also update memory cache
                                CacheImageInMemory(cacheKey, diskCachedImage);
                                
                                promptList.Remove(prompt);
                                cacheHits++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error reading image from disk cache for prompt: {PromptPreview}", 
                                    GetPromptPreview(prompt));
                            }
                        }
                    }
                }
                
                if (cacheHits > 0)
                {
                    _logger.LogInformation("Retrieved {CacheHits} images from cache, {Remaining} to generate", 
                        cacheHits, promptList.Count);
                }
                
                // If all prompts were cached, return early
                if (promptList.Count == 0)
                {
                    _logger.LogInformation("All requested images were retrieved from cache");
                    return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            
            // For remaining prompts that weren't in cache, optimize by priority and size
            
            // 1. Try to use batch API if supported by the model
            if (batchSize > 1 && IsBatchGenerationSupported())
            {
                for (int i = 0; i < promptList.Count; i += batchSize)
                {
                    // Take next batch
                    var batch = promptList.Skip(i).Take(batchSize).ToList();
                    if (batch.Count == 0) break;
                    
                    // Process batch
                    try
                    {
                        var batchResults = await GenerateImageBatchAsync(batch, width, height);
                        foreach (var (prompt, image) in batchResults)
                        {
                            result[prompt] = image;
                            
                            // Cache the result if caching is enabled
                            if (useCache)
                            {
                                string cacheKey = $"{prompt}_{width}_{height}";
                                CacheImageInMemory(cacheKey, image);
                                
                                // Cache to disk if enabled
                                if (_enableDiskCache)
                                {
                                    string diskCacheFilePath = Path.Combine(_diskCachePath, GetSHA256Hash(cacheKey) + ".png");
                                    _ = Task.Run(async () => await File.WriteAllBytesAsync(diskCacheFilePath, image));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error with batch image generation, falling back to individual processing");
                        // Will fall back to individual processing below for any remaining prompts
                        break;
                    }
                }
                
                // Remove successfully processed prompts
                promptList.RemoveAll(p => result.ContainsKey(p));
            }
            
            // 2. Process remaining prompts individually in parallel
            if (promptList.Count > 0)
            {
                // Prioritize prompts based on length or other criteria
                var prioritizedPrompts = promptList
                    .Select((prompt, index) => (Prompt: prompt, Priority: CalculatePromptPriority(prompt, index)))
                    .OrderByDescending(x => x.Priority)
                    .ToList();
                
                // Create a queue to process tasks with prioritization
                var tasks = new List<Task<(string Prompt, byte[] Image)>>();
                
                foreach (var (prompt, priority) in prioritizedPrompts)
                {
                    tasks.Add(Task.Run(async () => {
                        var image = await GenerateImageFromTextAsync(prompt, width, height, useCache, priority);
                        return (Prompt: prompt, Image: image);
                    }));
                }
                
                // As tasks complete, add results to the dictionary
                while (tasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(tasks);
                    tasks.Remove(completedTask);
                    
                    try
                    {
                        var (prompt, image) = await completedTask;
                        result[prompt] = image;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Task failed during parallel image generation");
                    }
                }
            }
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Completed optimized parallel image generation for {Count} prompts in {Duration}ms (avg: {AvgTime}ms per image)",
                result.Count,
                duration.TotalMilliseconds,
                result.Count > 0 ? (int)(duration.TotalMilliseconds / result.Count) : 0);
            
            return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        /// <summary>
        /// Generates multiple images in a single API call if supported
        /// </summary>
        private async Task<List<(string Prompt, byte[] Image)>> GenerateImageBatchAsync(
            List<string> prompts, 
            int width, 
            int height)
        {
            _logger.LogInformation("Generating batch of {Count} images", prompts.Count);
            
            // For demonstration, we'll just process each one individually
            // In a real implementation, this would be a batch API call
            var results = new List<(string Prompt, byte[] Image)>();
            var batchTasks = prompts.Select(async prompt => {
                var image = await GenerateImageFromTextAsync(prompt, width, height, false);
                return (Prompt: prompt, Image: image);
            });
            
            results.AddRange(await Task.WhenAll(batchTasks));
            return results;
        }
        
        /// <summary>
        /// Clears the image cache (both memory and optionally disk)
        /// </summary>
        /// <param name="clearDiskCache">Whether to also clear the disk cache</param>
        public void ClearCache(bool clearDiskCache = false)
        {
            // Clear memory cache
            ((MemoryCache)_imageCache).Compact(1.0);
            
            // Clear disk cache if enabled and requested
            if (clearDiskCache && _enableDiskCache && Directory.Exists(_diskCachePath))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(_diskCachePath, "*.png"))
                    {
                        File.Delete(file);
                    }
                    _logger.LogInformation("Cleared disk cache in {Path}", _diskCachePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing disk cache in {Path}", _diskCachePath);
                }
            }
            
            _logger.LogInformation("Cleared image cache");
        }
        
        /// <summary>
        /// Returns the cache size (number of entries)
        /// </summary>
        public int GetCacheSize()
        {
            return (int)(_imageCache.GetCurrentStatistics()?.CurrentEntryCount ?? 0);
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Caches an image in memory with expiration
        /// </summary>
        private void CacheImageInMemory(string cacheKey, byte[] imageData)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize((int)imageData.Length)
                .SetSlidingExpiration(TimeSpan.FromMinutes(_imageCacheExpirationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromHours(24)); // Hard limit of 24 hours
                
            _imageCache.Set(cacheKey, imageData, cacheEntryOptions);
        }
        
        /// <summary>
        /// Optimizes image dimensions for better performance based on content
        /// </summary>
        private void OptimizeImageDimensions(ref int width, ref int height)
        {
            // Ensure dimensions are multiples of 64 for better performance with most AI models
            width = (int)Math.Ceiling(width / 64.0) * 64;
            height = (int)Math.Ceiling(height / 64.0) * 64;
            
            // Limit maximum dimensions
            const int maxDimension = 1536;
            if (width > maxDimension || height > maxDimension)
            {
                // Maintain aspect ratio
                double ratio = (double)width / height;
                if (width > height)
                {
                    width = maxDimension;
                    height = (int)(maxDimension / ratio);
                }
                else
                {
                    height = maxDimension;
                    width = (int)(maxDimension * ratio);
                }
                
                // Ensure dimensions are still multiples of 64
                width = (int)Math.Ceiling(width / 64.0) * 64;
                height = (int)Math.Ceiling(height / 64.0) * 64;
            }
        }
        
        /// <summary>
        /// Gets a hash of a string for cache keys
        /// </summary>
        private string GetSHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
        
        /// <summary>
        /// Check if batch generation is supported by the current model
        /// </summary>
        private bool IsBatchGenerationSupported()
        {
            // In a real implementation, this would check if the model supports batch generation
            // For this demonstration, we'll assume it's not supported
            return false;
        }
        
        /// <summary>
        /// Calculate priority for a prompt based on characteristics
        /// </summary>
        private int CalculatePromptPriority(string prompt, int index)
        {
            // In a real implementation, this might prioritize based on prompt complexity,
            // length, expected processing time, or business rules
            
            // Simple implementation: prioritize shorter prompts (quicker to process)
            // and maintain original order for equal length prompts
            return 1000 - prompt.Length - (index / 1000);
        }
        
        /// <summary>
        /// Optimize prompt for better performance
        /// </summary>
        private string OptimizePrompt(string prompt)
        {
            // In a real implementation, this might:
            // - Remove unnecessary details
            // - Add specific performance-enhancing keywords
            // - Standardize formatting
            // - Apply prompt engineering best practices
            
            // For this demo, we're just returning the original prompt
            return prompt;
        }
        
        /// <summary>
        /// Creates a placeholder image for testing and demonstration
        /// </summary>
        private byte[] GeneratePlaceholderImage(int width, int height)
        {
            // In a real implementation, this would be the actual image from Gemini API
            // For demonstration, we're just creating a mock image
            
            // Mock implementation - in a real application this would be the actual image bytes
            // returning a random array of bytes of a reasonable size for an image of these dimensions
            int mockImageSize = (int)((long)width * height * 3 / 100); // Approximate size for a compressed image
            var mockImageData = new byte[mockImageSize];
            new Random().NextBytes(mockImageData);
            
            return mockImageData;
        }
        
        /// <summary>
        /// Get a preview of the prompt for logging
        /// </summary>
        private string GetPromptPreview(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                return "[empty]";
            }
            
            // Get the first 50 characters of the prompt
            int previewLength = Math.Min(prompt.Length, 50);
            string preview = prompt.Substring(0, previewLength);
            
            if (preview.Length < prompt.Length)
            {
                preview += "...";
            }
            
            return preview;
        }
        
        #endregion
    }
}
