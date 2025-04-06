using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    // Request Structures
    internal class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();

        // Optional: Add GenerationConfig if needed
        // [JsonPropertyName("generationConfig")]
        // public GenerationConfig GenerationConfig { get; set; }
    }
}