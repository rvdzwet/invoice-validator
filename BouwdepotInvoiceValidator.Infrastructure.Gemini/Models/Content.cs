using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Google.Models
{
    internal class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();

        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";
    }
}
