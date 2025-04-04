using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Models.Prompts
{
    public class PromptFile
    {
        public PromptMetadata Metadata { get; set; } = new PromptMetadata();
        public PromptTemplateContent Template { get; set; } = new PromptTemplateContent();
        public List<PromptExample> Examples { get; set; } = new List<PromptExample>();
    }

    public class PromptMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public string Description { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }

    public class PromptTemplateContent
    {
        public string Role { get; set; } = string.Empty;
        public string Task { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new List<string>();
        
        [JsonPropertyName("outputFormat")]
        public object OutputFormat { get; set; } = new object();
    }

    public class PromptExample
    {
        public string Input { get; set; } = string.Empty;
        
        [JsonPropertyName("output")]
        public object Output { get; set; } = new object();
    }
}
