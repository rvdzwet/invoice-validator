using System.Text.Json;
using BouwdepotInvoiceValidator.Models.Prompts;

namespace BouwdepotInvoiceValidator.Services.Prompts
{
    public class PromptFileService
    {
        private readonly ILogger<PromptFileService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly Dictionary<string, PromptTemplate> _loadedTemplates = new();
        
        public PromptFileService(ILogger<PromptFileService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }
        
        public async Task InitializeAsync()
        {
            var promptsDirectory = Path.Combine(_environment.ContentRootPath, "Prompts");
            
            if (!Directory.Exists(promptsDirectory))
            {
                _logger.LogInformation("Creating prompts directory: {Directory}", promptsDirectory);
                Directory.CreateDirectory(promptsDirectory);
            }
            
            await LoadPromptsFromDirectoryAsync(promptsDirectory);
        }
        
        private async Task LoadPromptsFromDirectoryAsync(string directory)
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogWarning("Prompts directory not found: {Directory}", directory);
                return;
            }
            
            // Load all JSON files in this directory and subdirectories
            foreach (var file in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var promptFile = JsonSerializer.Deserialize<PromptFile>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (promptFile != null && !string.IsNullOrEmpty(promptFile.Metadata.Name))
                    {
                        var template = new PromptTemplate
                        {
                            Role = promptFile.Template.Role,
                            Task = promptFile.Template.Task,
                            Instructions = promptFile.Template.Instructions,
                            OutputFormat = JsonSerializer.Serialize(promptFile.Template.OutputFormat, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }),
                            Examples = promptFile.Examples?.Select(e => new Example
                            {
                                Input = e.Input,
                                Output = JsonSerializer.Serialize(e.Output, new JsonSerializerOptions
                                {
                                    WriteIndented = true
                                })
                            }).ToList() ?? new List<Example>(),
                            Version = promptFile.Metadata.Version
                        };
                        
                        _loadedTemplates[promptFile.Metadata.Name] = template;
                        _logger.LogInformation("Loaded prompt template: {Name} (v{Version}) from {File}", 
                            promptFile.Metadata.Name, promptFile.Metadata.Version, Path.GetFileName(file));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading prompt file: {File}", file);
                }
            }
        }
        
        public PromptTemplate? GetTemplate(string name)
        {
            if (_loadedTemplates.TryGetValue(name, out var template))
            {
                return template;
            }
            
            _logger.LogWarning("Template not found: {Name}", name);
            return null;
        }
        
        public async Task ReloadTemplatesAsync()
        {
            _loadedTemplates.Clear();
            await InitializeAsync();
        }
        
        public async Task SaveTemplateAsync(string category, string name, PromptFile promptFile)
        {
            var promptsDirectory = Path.Combine(_environment.ContentRootPath, "Prompts");
            var categoryDirectory = Path.Combine(promptsDirectory, category);
            
            if (!Directory.Exists(categoryDirectory))
            {
                Directory.CreateDirectory(categoryDirectory);
            }
            
            var filePath = Path.Combine(categoryDirectory, $"{name}.json");
            
            var json = JsonSerializer.Serialize(promptFile, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("Saved prompt template: {Name} to {File}", promptFile.Metadata.Name, filePath);
            
            // Reload the template
            await ReloadTemplatesAsync();
        }
    }
}
