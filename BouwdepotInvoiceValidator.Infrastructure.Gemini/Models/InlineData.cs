using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Google.Models
{
    internal class InlineData
    {
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty; // Base64 encoded data
    }
}