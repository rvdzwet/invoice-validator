using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Google.Models
{
    internal class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; } // e.g., "STOP", "MAX_TOKENS", "SAFETY"

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating>? SafetyRatings { get; set; }
    }
}