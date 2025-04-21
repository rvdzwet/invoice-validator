using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    /// <summary>
    /// Configuration for text generation in Gemini API requests
    /// </summary>
    internal class GenerationConfig
    {
        /// <summary>
        /// Controls randomness in the model's response. 
        /// Values close to 0.0 will produce more deterministic responses,
        /// while values close to 1.0 will produce more random/creative responses.
        /// Range: 0.0 to 1.0
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        
        /// <summary>
        /// Defines the nucleus sampling probability threshold.
        /// The model will only consider tokens with combined probability > topP.
        /// Setting to 0.0 disables nucleus sampling, making generation fully deterministic.
        /// Range: 0.0 to 1.0
        /// </summary>
        [JsonPropertyName("topP")]
        public double? TopP { get; set; }
        
        /// <summary>
        /// Limits token selection to the topK most likely next tokens.
        /// Setting to 1 means only the most likely token is considered, increasing determinism.
        /// </summary>
        [JsonPropertyName("topK")]
        public int? TopK { get; set; }
        
        /// <summary>
        /// Number of candidate responses to generate.
        /// For deterministic results, this should be set to 1.
        /// </summary>
        [JsonPropertyName("candidateCount")]
        public int? CandidateCount { get; set; }
        
        /// <summary>
        /// Maximum number of tokens that can be generated in the response.
        /// </summary>
        [JsonPropertyName("maxOutputTokens")]
        public int? MaxOutputTokens { get; set; }
        
        /// <summary>
        /// Whether to include stop sequences in the output.
        /// </summary>
        [JsonPropertyName("stopSequences")]
        public List<string>? StopSequences { get; set; }
    }
}
