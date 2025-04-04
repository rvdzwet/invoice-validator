using BouwdepotInvoiceValidator.Models.Prompts;

namespace BouwdepotInvoiceValidator.Services.Prompts
{
    public class PromptTemplateService
    {
        private readonly ILogger<PromptTemplateService> _logger;
        private readonly PromptFileService _fileService;
        private readonly List<PromptVersionHistory> _versionHistory = new();
        
        public PromptTemplateService(
            ILogger<PromptTemplateService> logger,
            PromptFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }
        
        public string GetPrompt(string templateName, Dictionary<string, string> parameters)
        {
            var template = _fileService.GetTemplate(templateName);
            
            if (template == null)
            {
                _logger.LogWarning("Template {TemplateName} not found. Using default prompt.", templateName);
                return parameters.ContainsKey("defaultPrompt") ? parameters["defaultPrompt"] : string.Empty;
            }
            
            return template.Build(parameters);
        }
        
        public async Task UpdateTemplateAsync(string category, string name, PromptFile newTemplate, string changeDescription)
        {
            // Get the old version if it exists
            var oldTemplate = _fileService.GetTemplate(newTemplate.Metadata.Name);
            string oldVersion = oldTemplate?.Version ?? "0.0";
            
            // Save the template
            await _fileService.SaveTemplateAsync(category, name, newTemplate);
            
            // Record the change in version history
            _versionHistory.Add(new PromptVersionHistory
            {
                TemplateName = newTemplate.Metadata.Name,
                Version = newTemplate.Metadata.Version,
                ImplementedDate = DateTime.UtcNow,
                ChangeDescription = changeDescription
            });
            
            _logger.LogInformation("Updated prompt template: {TemplateName} from v{OldVersion} to v{NewVersion}", 
                newTemplate.Metadata.Name, oldVersion, newTemplate.Metadata.Version);
        }
        
        public List<PromptVersionHistory> GetVersionHistory(string templateName)
        {
            return _versionHistory
                .Where(h => h.TemplateName == templateName)
                .OrderByDescending(h => h.ImplementedDate)
                .ToList();
        }
    }
    
    public class PromptVersionHistory
    {
        public string TemplateName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime ImplementedDate { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    }
}
