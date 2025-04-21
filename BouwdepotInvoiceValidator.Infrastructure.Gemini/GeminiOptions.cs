using System.ComponentModel.DataAnnotations;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google
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
        public string ApiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/"; // Or v1 depending on stability preference

        /// <summary>
        /// The API key for authenticating with the Gemini API
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ApiKey { get; set; } = string.Empty; // Ensure default is invalid if Required

        /// <summary>
        /// The default model to use for text-only prompts if ModelName is not specified.
        /// Examples: "gemini-1.5-pro-latest", "gemini-1.5-flash-latest", "gemini-pro".
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultTextModel { get; set; } = "gemini-2.0-flash"; // Updated default

        /// <summary>
        /// The default model to use for multimodal prompts if ModelName is not specified.
        /// Example: "gemini-1.5-pro-latest", "gemini-1.5-flash-latest".
        /// Note: Older models like "gemini-pro-vision" might be specific to older API versions.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultMultimodalModel { get; set; } = "gemini-2.0-flash"; // Updated default, as 1.5 models handle multimodality

        /// <summary>
        /// The maximum number of retries for failed API calls
        /// </summary>
        [Range(0, 10)] // Example range
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// The timeout for API calls in seconds
        /// </summary>
        [Range(5, 300)] // Example range
        public int TimeoutSeconds { get; set; } = 60; // Increased default timeout slightly

        /// <summary>
        /// Controls randomness in the model's response. 
        /// Values close to 0.0 will produce more deterministic responses,
        /// while values close to 1.0 will produce more random/creative responses.
        /// Range: 0.0 to 1.0
        /// </summary>
        [Range(0.0, 1.0)]
        public double Temperature { get; set; } = 0.0; // Default to most deterministic
        
        /// <summary>
        /// Defines the nucleus sampling probability threshold.
        /// The model will only consider tokens with combined probability > topP.
        /// Setting to 0.0 disables nucleus sampling, making generation fully deterministic.
        /// Range: 0.0 to 1.0
        /// </summary>
        [Range(0.0, 1.0)]
        public double TopP { get; set; } = 0.0; // Default to most deterministic
        
        /// <summary>
        /// Limits token selection to the topK most likely next tokens.
        /// Setting to 1 means only the most likely token is considered, increasing determinism.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int TopK { get; set; } = 1; // Default to most deterministic
        
        /// <summary>
        /// Number of candidate responses to generate.
        /// For deterministic results, this should be set to 1.
        /// </summary>
        [Range(1, 10)]
        public int CandidateCount { get; set; } = 1; // Default to most deterministic
    }
}
