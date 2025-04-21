using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Services
{
    public class PromptService
    {
        private readonly string _promptsDirectory;
        private readonly Dictionary<string, Prompt> _loadedPrompts = new Dictionary<string, Prompt>();
        private readonly ILogger<PromptService> _logger;

        public PromptService(IConfiguration configuration, ILogger<PromptService> logger)
        {
            _promptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["PromptSettings:DirectoryPath"]); 
            _logger = logger;
            LoadAllPromptsOnStartup();
        }

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

        public Prompt GetPrompt(string promptName)
        {
            if (_loadedPrompts.TryGetValue(promptName, out var prompt))
            {
                return prompt;
            }
            _logger.LogDebug($"Prompt '{promptName}' not found in loaded prompts.");
            return null;
        }

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

        public void ReloadPrompts()
        {
            _logger.LogInformation("Reloading prompts.");
            _loadedPrompts.Clear();
            LoadAllPromptsOnStartup();
            _logger.LogInformation("Prompts reloaded.");
        }
    }

    public static class PromptServiceExtensions
    {
        public static IServiceCollection AddPromptService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<PromptService>();
            return services;
        }
    }
}
