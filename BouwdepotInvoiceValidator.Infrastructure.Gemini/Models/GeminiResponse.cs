using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    // Response Structures
    internal class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }

        [JsonPropertyName("promptFeedback")]
        public PromptFeedback? PromptFeedback { get; set; }
    }
}