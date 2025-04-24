using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Ollama.Models
{
    /// <summary>
    /// Represents a single message in the Ollama chat history.
    /// </summary>
    internal class OllamaChatMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "system", "user", "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The text content of the message.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional list of Base64-encoded images for multimodal requests.
        /// </summary>
        [JsonPropertyName("images")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Images { get; set; }
    }
}
