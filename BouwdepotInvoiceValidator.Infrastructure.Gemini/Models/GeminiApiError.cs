using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Google.Models
{
    internal class GeminiApiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}