using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    // Error Structure (Simplified - check actual API error format)
    internal class GeminiApiErrorResponse
    {
        [JsonPropertyName("error")]
        public GeminiApiError? Error { get; set; }
    }
}