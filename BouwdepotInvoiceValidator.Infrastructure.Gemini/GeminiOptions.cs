using System.ComponentModel.DataAnnotations;

namespace BouwdepotInvoiceValidator.Infrastructure.Gemini
{
    /// <summary>
    /// Options for configuring the Gemini client
    /// </summary>
    public class GeminiOptions
    {
        /// <summary>
        /// The base URL of the Gemini API
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Url] // Basic URL format validation
        public string ApiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";

        /// <summary>
        /// The API key for authenticating with the Gemini API
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ApiKey { get; set; } = string.Empty; // Ensure default is invalid if Required

        /// <summary>
        /// The default model to use for text-only prompts
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultTextModel { get; set; } = "gemini-pro";

        /// <summary>
        /// The default model to use for multimodal prompts
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultMultimodalModel { get; set; } = "gemini-pro-vision";

        /// <summary>
        /// The maximum number of retries for failed API calls
        /// </summary>
        [Range(0, 10)] // Example range
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// The timeout for API calls in seconds
        /// </summary>
        [Range(5, 300)] // Example range
        public int TimeoutSeconds { get; set; } = 30;
    }
}