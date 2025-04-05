namespace BouwdepotInvoiceValidator.Services.Prompts
{
    public class PromptInitializationService : IHostedService
    {
        private readonly ILogger<PromptInitializationService> _logger;
        private readonly PromptFileService _promptFileService;
        
        public PromptInitializationService(
            ILogger<PromptInitializationService> logger,
            PromptFileService promptFileService)
        {
            _logger = logger;
            _promptFileService = promptFileService;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing prompt templates");
            await _promptFileService.InitializeAsync();
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
