using System.ComponentModel.DataAnnotations;

namespace BouwdepotInvoiceValidator.Infrastructure.Ollama
{
    /// <summary>
    /// Configuration options for the Ollama client.
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SectionName = "Ollama";

        /// <summary>
        /// Base URL for the Ollama API.
        /// Example: "http://localhost:11434"
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Url]
        public string ApiBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Default model to use for text-only prompts.
        /// Example: "llama3"
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultTextModel { get; set; } = string.Empty;

        /// <summary>
        /// Default model to use for multimodal prompts (text + images).
        /// Example: "llava"
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string DefaultMultimodalModel { get; set; } = string.Empty;

        /// <summary>
        /// Request timeout in seconds.
        /// </summary>
        [Range(1, 600)] // Example range: 1 second to 10 minutes
        public int TimeoutSeconds { get; set; } = 120; // Default to 2 minutes

        /// <summary>
        /// Controls how long the model stays loaded in memory after a request.
        /// Use "-1" for infinite, "0" to unload immediately, or a duration like "5m".
        /// </summary>
        public string KeepAlive { get; set; } = "5m"; // Default to 5 minutes

        // Optional generation parameters (if supported directly by the API endpoint used)
        // These might need adjustment based on the specific Ollama API endpoint behavior.

        /// <summary>
        /// Controls randomness: lower values make the model more deterministic. (Optional)
        /// </summary>
        public float? Temperature { get; set; } // Nullable if not always set

        /// <summary>
        /// Nucleus sampling: considers only the top P probability mass. (Optional)
        /// </summary>
        public float? TopP { get; set; } // Nullable if not always set

        /// <summary>
        /// Considers only the top K most likely tokens. (Optional)
        /// </summary>
        public int? TopK { get; set; } // Nullable if not always set
    }
}
