using System.Text;

namespace BouwdepotInvoiceValidator.Models.Prompts
{
    public class PromptTemplate
    {
        public string Role { get; set; } = string.Empty;
        public string Task { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new List<string>();
        public string OutputFormat { get; set; } = string.Empty;
        public List<Example> Examples { get; set; } = new List<Example>();
        public string Version { get; set; } = "1.0";
        
        public string Build(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();
            
            // Add role and task
            sb.AppendLine($"### ROLE ###");
            sb.AppendLine(Role);
            sb.AppendLine();
            
            sb.AppendLine($"### TASK ###");
            sb.AppendLine(Task);
            sb.AppendLine();
            
            // Add context if provided
            if (parameters.ContainsKey("context"))
            {
                sb.AppendLine($"### CONTEXT ###");
                sb.AppendLine(parameters["context"]);
                sb.AppendLine();
            }
            
            // Add instructions
            sb.AppendLine($"### INSTRUCTIONS ###");
            foreach (var instruction in Instructions)
            {
                // Replace placeholders with parameter values
                var processedInstruction = instruction;
                foreach (var param in parameters)
                {
                    processedInstruction = processedInstruction.Replace($"{{{param.Key}}}", param.Value);
                }
                sb.AppendLine(processedInstruction);
            }
            sb.AppendLine();
            
            // Add examples if available
            if (Examples != null && Examples.Count > 0)
            {
                sb.AppendLine($"### EXAMPLES ###");
                foreach (var example in Examples)
                {
                    sb.AppendLine($"Input: {example.Input}");
                    sb.AppendLine($"Output: {example.Output}");
                    sb.AppendLine();
                }
            }
            
            // Add output format
            sb.AppendLine($"### OUTPUT FORMAT ###");
            sb.AppendLine(OutputFormat);
            
            return sb.ToString();
        }
    }

    public class Example
    {
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
    }
}
