using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    internal class SafetyRating
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; } // e.g., "HARM_CATEGORY_SEXUALLY_EXPLICIT"

        [JsonPropertyName("probability")]
        public string? Probability { get; set; } // e.g., "NEGLIGIBLE", "LOW", "MEDIUM", "HIGH"
    }
}