using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Infrastructure.Providers.Google.Models
{
    internal class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();

        // Optional: Add Role ("user" or "model") if building conversational history
        // [JsonPropertyName("role")]
        // public string Role { get; set; } = "user";
    }
}