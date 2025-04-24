using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Schema
{
    /// <summary>
    /// Service for working with prompts that can dynamically generate JSON schemas from POCO classes
    /// </summary>
    public class DynamicPromptService
    {
        private readonly string _promptsDirectory;
        private readonly Dictionary<string, Prompt> _loadedPrompts = new Dictionary<string, Prompt>();
        private readonly ILogger<DynamicPromptService> _logger;
        private readonly IJsonSchemaGenerator _schemaGenerator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPromptService"/> class
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <param name="logger">The logger</param>
        /// <param name="schemaGenerator">The JSON schema generator</param>
        public DynamicPromptService(
            IConfiguration configuration, 
            ILogger<DynamicPromptService> logger,
            IJsonSchemaGenerator schemaGenerator)
        {
            _promptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["PromptSettings:DirectoryPath"]); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
            LoadAllPromptsOnStartup();
        }
        
        /// <summary>
        /// Loads all prompts from the prompts directory
        /// </summary>
        private void LoadAllPromptsOnStartup()
        {
            if (string.IsNullOrEmpty(_promptsDirectory) || !Directory.Exists(_promptsDirectory))
            {
                _logger.LogWarning($"Prompt directory '{_promptsDirectory}' not found or configured.");
                return;
            }

            string[] promptFiles = Directory.GetFiles(_promptsDirectory, "*.json", SearchOption.AllDirectories);

            foreach (var filePath in promptFiles)
            {
                try
                {
                    string relativePath = Path.GetRelativePath(_promptsDirectory, filePath);
                    string promptName = Path.GetFileNameWithoutExtension(relativePath).Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');

                    Prompt prompt = GetPromptFromFileInternal(filePath);
                    if (prompt != null && !_loadedPrompts.ContainsKey(promptName))
                    {
                        _loadedPrompts.Add(prompt.Metadata.Name, prompt);
                        _logger.LogInformation($"Loaded prompt: {promptName} (from {relativePath})");
                    }
                    else if (prompt != null && _loadedPrompts.ContainsKey(promptName))
                    {
                        _logger.LogWarning($"Duplicate prompt name '{promptName}' found in '{relativePath}'. The first loaded prompt with this name will be used.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error loading prompt from '{filePath}'");
                }
            }
        }
        
        /// <summary>
        /// Gets a prompt by name
        /// </summary>
        /// <param name="promptName">The name of the prompt</param>
        /// <returns>The prompt, or null if not found</returns>
        public Prompt GetPrompt(string promptName)
        {
            if (_loadedPrompts.TryGetValue(promptName, out var prompt))
            {
                return prompt;
            }
            _logger.LogDebug($"Prompt '{promptName}' not found in loaded prompts.");
            return null;
        }
        
        /// <summary>
        /// Gets a prompt from a file
        /// </summary>
        /// <param name="filePath">The path to the prompt file</param>
        /// <returns>The prompt, or null if not found</returns>
        private Prompt GetPromptFromFileInternal(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"Prompt file not found at path: {filePath}");
                    return null;
                }

                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Prompt>(jsonString);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error deserializing prompt file '{filePath}'");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading prompt file '{filePath}'");
                return null;
            }
        }
        
        /// <summary>
        /// Reloads all prompts from the prompts directory
        /// </summary>
        public void ReloadPrompts()
        {
            _logger.LogInformation("Reloading prompts.");
            _loadedPrompts.Clear();
            LoadAllPromptsOnStartup();
            _logger.LogInformation("Prompts reloaded.");
        }
        
        /// <summary>
        /// Builds a prompt with a dynamic schema generated from a POCO class
        /// </summary>
        /// <typeparam name="T">The type to generate a schema for</typeparam>
        /// <param name="promptName">The name of the base prompt to use</param>
        /// <returns>The prompt with the dynamic schema</returns>
        public string BuildPromptWithDynamicSchema<T>(string promptName)
        {
            try
            {
                var prompt = GetPrompt(promptName);
                if (prompt == null)
                {
                    _logger.LogWarning($"Prompt '{promptName}' not found.");
                    return null;
                }
                
                // Generate schema and example JSON from the POCO class
                var schema = _schemaGenerator.GenerateSchema<T>();
                var exampleJson = _schemaGenerator.GenerateExampleJson<T>();
                
                // Build the prompt with the dynamic schema
                var sb = new StringBuilder();
                
                // Add the role
                if (!string.IsNullOrEmpty(prompt.Template?.Role))
                {
                    sb.AppendLine(prompt.Template.Role);
                    sb.AppendLine();
                }
                
                // Add the task
                if (!string.IsNullOrEmpty(prompt.Template?.Task))
                {
                    sb.AppendLine(prompt.Template.Task);
                    sb.AppendLine();
                }
                
                // Add the instructions
                if (prompt.Template?.Instructions != null && prompt.Template.Instructions.Count > 0)
                {
                    foreach (var instruction in prompt.Template.Instructions)
                    {
                        sb.AppendLine(instruction);
                    }
                    sb.AppendLine();
                }
                
                // Add the dynamic schema
                sb.AppendLine("IMPORTANT: Your response MUST be a valid JSON object with the exact structure shown in the following JSON schema:");
                sb.AppendLine("```json");
                sb.AppendLine(schema);
                sb.AppendLine("```");
                sb.AppendLine();
                
                // Add an example
                sb.AppendLine("Here's an example of a valid response:");
                sb.AppendLine("```json");
                sb.AppendLine(exampleJson);
                sb.AppendLine("```");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building prompt with dynamic schema for type {typeof(T).Name}");
                throw;
            }
        }
        
        /// <summary>
        /// Builds a prompt with a dynamic schema generated from a POCO class
        /// </summary>
        /// <param name="promptName">The name of the base prompt to use</param>
        /// <param name="type">The type to generate a schema for</param>
        /// <returns>The prompt with the dynamic schema</returns>
        public string BuildPromptWithDynamicSchema(string promptName, Type type)
        {
            try
            {
                var prompt = GetPrompt(promptName);
                if (prompt == null)
                {
                    _logger.LogWarning($"Prompt '{promptName}' not found.");
                    return null;
                }
                
                // Generate schema and example JSON from the POCO class
                var schema = _schemaGenerator.GenerateSchema(type);
                var exampleJson = _schemaGenerator.GenerateExampleJson(type);
                
                // Build the prompt with the dynamic schema
                var sb = new StringBuilder();
                
                // Add the role
                if (!string.IsNullOrEmpty(prompt.Template?.Role))
                {
                    sb.AppendLine(prompt.Template.Role);
                    sb.AppendLine();
                }
                
                // Add the task
                if (!string.IsNullOrEmpty(prompt.Template?.Task))
                {
                    sb.AppendLine(prompt.Template.Task);
                    sb.AppendLine();
                }
                
                // Add the instructions
                if (prompt.Template?.Instructions != null && prompt.Template.Instructions.Count > 0)
                {
                    foreach (var instruction in prompt.Template.Instructions)
                    {
                        sb.AppendLine(instruction);
                    }
                    sb.AppendLine();
                }
                
                // Add the dynamic schema
                sb.AppendLine("IMPORTANT: Your response MUST be a valid JSON object with the exact structure shown in the following JSON schema:");
                sb.AppendLine("```json");
                sb.AppendLine(schema);
                sb.AppendLine("```");
                sb.AppendLine();
                
                // Add an example
                sb.AppendLine("Here's an example of a valid response:");
                sb.AppendLine("```json");
                sb.AppendLine(exampleJson);
                sb.AppendLine("```");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building prompt with dynamic schema for type {type.Name}");
                throw;
            }
        }
        
        /// <summary>
        /// Builds a prompt with a dynamic schema generated from a POCO class and additional variables
        /// </summary>
        /// <typeparam name="T">The type to generate a schema for</typeparam>
        /// <param name="promptName">The name of the base prompt to use</param>
        /// <param name="variables">Additional variables to include in the prompt</param>
        /// <returns>The prompt with the dynamic schema</returns>
        public string BuildPromptWithDynamicSchema<T>(string promptName, Dictionary<string, string> variables)
        {
            try
            {
                var prompt = GetPrompt(promptName);
                if (prompt == null)
                {
                    _logger.LogWarning($"Prompt '{promptName}' not found.");
                    return null;
                }
                
                // Generate schema and example JSON from the POCO class
                var schema = _schemaGenerator.GenerateSchema<T>();
                var exampleJson = _schemaGenerator.GenerateExampleJson<T>();
                
                // Build the prompt with the dynamic schema
                var sb = new StringBuilder();
                
                // Add the role
                if (!string.IsNullOrEmpty(prompt.Template?.Role))
                {
                    var role = prompt.Template.Role;
                    foreach (var variable in variables)
                    {
                        role = role.Replace($"{{{variable.Key}}}", variable.Value);
                    }
                    sb.AppendLine(role);
                    sb.AppendLine();
                }
                
                // Add the task
                if (!string.IsNullOrEmpty(prompt.Template?.Task))
                {
                    var task = prompt.Template.Task;
                    foreach (var variable in variables)
                    {
                        task = task.Replace($"{{{variable.Key}}}", variable.Value);
                    }
                    sb.AppendLine(task);
                    sb.AppendLine();
                }
                
                // Add the instructions
                if (prompt.Template?.Instructions != null && prompt.Template.Instructions.Count > 0)
                {
                    foreach (var instruction in prompt.Template.Instructions)
                    {
                        var processedInstruction = instruction;
                        foreach (var variable in variables)
                        {
                            processedInstruction = processedInstruction.Replace($"{{{variable.Key}}}", variable.Value);
                        }
                        sb.AppendLine(processedInstruction);
                    }
                    sb.AppendLine();
                }
                
                // Add the dynamic schema
                sb.AppendLine("IMPORTANT: Your response MUST be a valid JSON object with the exact structure shown in the following JSON schema:");
                sb.AppendLine("```json");
                sb.AppendLine(schema);
                sb.AppendLine("```");
                sb.AppendLine();
                
                // Add an example
                sb.AppendLine("Here's an example of a valid response:");
                sb.AppendLine("```json");
                sb.AppendLine(exampleJson);
                sb.AppendLine("```");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building prompt with dynamic schema for type {typeof(T).Name}");
                throw;
            }
        }
    }
}
