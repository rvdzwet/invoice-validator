using System.Text;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    public class Prompt
    {
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }

        [JsonPropertyName("template")]
        public Template Template { get; set; }

        [JsonPropertyName("examples")]
        public List<Example> Examples { get; set; }

        internal string Render(params KeyValuePair<string, string>[] variables)
        {
            var sb = new StringBuilder();

            // Render the role
            if (!string.IsNullOrEmpty(Template?.Role))
            {
                sb.AppendLine($"Role: {Template.Role}");
            }

            // Render the task
            if (!string.IsNullOrEmpty(Template?.Task))
            {
                sb.AppendLine($"Task: {Template.Task}");
            }

            // Render the instructions
            if (Template?.Instructions != null && Template.Instructions.Any())
            {
                sb.AppendLine("Instructions:");
                for (int i = 0; i < Template.Instructions.Count; i++)
                {
                    string instruction = Template.Instructions[i];
                    if (variables != null && variables.Any())
                    {
                        foreach (var variable in variables)
                        {
                            instruction = instruction.Replace($"{{{variable.Key}}}", variable.Value);
                        }
                    }
                    sb.AppendLine($"{i + 1}. {instruction}");
                }
            }

            // Output format is now handled by the DynamicPromptService

            return sb.ToString().TrimEnd();
        }
    }

    public class Metadata
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }
    }

    public class Template
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("task")]
        public string Task { get; set; }

        [JsonPropertyName("instructions")]
        public List<string> Instructions { get; set; }

        // OutputFormat property removed as it's now handled by the DynamicPromptService
    }

    public class Example
    {
        [JsonPropertyName("input")]
        public string Input { get; set; }

        [JsonPropertyName("output")]
        public Dictionary<string, object> Output { get; set; }
    }
}
