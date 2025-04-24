using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Ollama.Models
{
    /// <summary>
    /// Represents the request body for the Ollama /api/chat endpoint.
    /// </summary>
    internal class OllamaChatRequest
    {
        /// <summary>
        /// The model name to use (e.g., "llama3", "llava").
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The list of messages constituting the conversation history and the current prompt.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<OllamaChatMessage> Messages { get; set; } = new();

        /// <summary>
        /// Whether to stream the response. We'll set this to false for simplicity.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// Specifies the format to return a response in. Set to "json" for structured output.
        /// </summary>
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; set; } // "json" or null

        /// <summary>
        /// Additional model parameters.
        /// </summary>
        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OllamaRequestOptions? Options { get; set; }

        /// <summary>
        /// Controls how long the model will stay loaded into memory following the request.
        /// </summary>
        [JsonPropertyName("keep_alive")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? KeepAlive { get; set; }
    }

    /// <summary>
    /// Optional parameters for the Ollama request.
    /// </summary>
    internal class OllamaRequestOptions
    {
        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Temperature { get; set; }

        [JsonPropertyName("top_p")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? TopP { get; set; }

        [JsonPropertyName("top_k")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TopK { get; set; }

        // Add other parameters like num_predict, seed, stop, etc., if needed
    }
}
