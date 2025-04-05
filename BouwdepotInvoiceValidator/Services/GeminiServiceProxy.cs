using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services.Prompts;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Proxy service that delegates Gemini API operations to specialized service classes
    /// </summary>
    public class GeminiServiceProxy : IGeminiService
    {
        private readonly ILogger<GeminiServiceProxy> _logger;
        private readonly Gemini.GeminiDocumentAnalysisService _documentService;
        private readonly Gemini.GeminiHomeImprovementService _homeImprovementService;
        private readonly Gemini.GeminiAdvancedAnalysisService _advancedAnalysisService;
        private readonly Gemini.GeminiLineItemAnalysisService _lineItemAnalysisService;
        private readonly Gemini.GeminiFraudDetectionService _fraudDetectionService;
        private readonly BouwdepotInvoiceValidator.Services.Gemini.GeminiService _geminiService;
        
        public GeminiServiceProxy(
            ILogger<GeminiServiceProxy> logger,
            Gemini.GeminiDocumentAnalysisService documentService,
            Gemini.GeminiHomeImprovementService homeImprovementService,
            Gemini.GeminiAdvancedAnalysisService advancedAnalysisService,
            Gemini.GeminiLineItemAnalysisService lineItemAnalysisService,
            Gemini.GeminiFraudDetectionService fraudDetectionService,
            BouwdepotInvoiceValidator.Services.Gemini.GeminiService geminiService,
            PromptTemplateService promptTemplateService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _homeImprovementService = homeImprovementService ?? throw new ArgumentNullException(nameof(homeImprovementService));
            _advancedAnalysisService = advancedAnalysisService ?? throw new ArgumentNullException(nameof(advancedAnalysisService));
            _lineItemAnalysisService = lineItemAnalysisService ?? throw new ArgumentNullException(nameof(lineItemAnalysisService));
            _fraudDetectionService = fraudDetectionService ?? throw new ArgumentNullException(nameof(fraudDetectionService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            
            // We don't need to store promptTemplateService as a field since it's only used for dependency injection
            // But we still validate it's not null
            if (promptTemplateService == null) throw new ArgumentNullException(nameof(promptTemplateService));
            
            _logger.LogInformation("GeminiServiceProxy initialized with specialized service implementations");
        }
        
        /// <summary>
        /// Delegates starting a new conversation to GeminiService
        /// </summary>
        public string StartNewConversation(Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Delegating start new conversation to GeminiService");
            return _geminiService.StartNewConversation(metadata);
        }
        
        /// <summary>
        /// Delegates switching conversation to GeminiService
        /// </summary>
        public bool SwitchConversation(string conversationId)
        {
            _logger.LogInformation("Delegating switch conversation to GeminiService for ID: {ConversationId}", conversationId);
            return _geminiService.SwitchConversation(conversationId);
        }
        
        /// <summary>
        /// Delegates clearing current conversation to GeminiService
        /// </summary>
        public void ClearCurrentConversation()
        {
            _logger.LogInformation("Delegating clear current conversation to GeminiService");
            _geminiService.ClearCurrentConversation();
        }
        
        /// <summary>
        /// Delegates getting current conversation to GeminiService
        /// </summary>
        public ConversationContext GetCurrentConversation()
        {
            _logger.LogInformation("Delegating get current conversation to GeminiService");
            return _geminiService.GetCurrentConversation();
        }
        
        /// <summary>
        /// Delegates getting conversation prompt to GeminiService
        /// </summary>
        public async Task<string> GetConversationPromptAsync(string prompt, bool useHistory = true)
        {
            _logger.LogInformation("Delegating conversation prompt to GeminiService");
            return await _geminiService.GetConversationPromptAsync(prompt, useHistory);
        }
        
        /// <summary>
        /// Delegates document type verification to GeminiDocumentService
        /// </summary>
        public async Task<ValidationResult> VerifyDocumentTypeAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating document type verification for {FileName} to GeminiDocumentService", invoice.FileName);
            return await _documentService.VerifyDocumentTypeAsync(invoice);
        }
        
        /// <summary>
        /// Delegates home improvement validation to GeminiHomeImprovementService
        /// </summary>
        public async Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating home improvement validation for {FileName} to GeminiHomeImprovementService", invoice.FileName);
            return await _homeImprovementService.ValidateHomeImprovementAsync(invoice);
        }
        
        /// <summary>
        /// Delegates fraud detection to GeminiFraudDetectionService
        /// </summary>
        public async Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering)
        {
            _logger.LogInformation("Delegating fraud detection for {FileName} to GeminiFraudDetectionService", invoice.FileName);
            return await _fraudDetectionService.DetectFraudAsync(invoice, detectedTampering);
        }
        
        /// <summary>
        /// Delegates multi-modal analysis to GeminiHomeImprovementService
        /// </summary>
        public async Task<ValidationResult> ValidateWithMultiModalAnalysisAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating multi-modal analysis for {FileName} to GeminiHomeImprovementService", invoice.FileName);
            return await _homeImprovementService.ValidateWithMultiModalAnalysisAsync(invoice);
        }
        
        /// <summary>
        /// Delegates audit-ready assessment to GeminiLineItemAnalysisService
        /// </summary>
        public async Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating audit-ready assessment for {FileName} to GeminiLineItemAnalysisService", invoice.FileName);
            return await _lineItemAnalysisService.GetAuditReadyAssessmentAsync(invoice);
        }
        
        /// <summary>
        /// Delegates invoice data extraction from images to GeminiAdvancedAnalysisService
        /// </summary>
        public async Task<Invoice> ExtractInvoiceDataFromImagesAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating invoice data extraction from images for {FileName} to GeminiAdvancedAnalysisService", invoice.FileName);
            return await _advancedAnalysisService.ExtractInvoiceDataFromImagesAsync(invoice);
        }
        
        /// <summary>
        /// Delegates line item analysis to GeminiLineItemAnalysisService
        /// </summary>
        public async Task<Models.Analysis.LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice)
        {
            _logger.LogInformation("Delegating line item analysis for {FileName} to GeminiLineItemAnalysisService", invoice.FileName);
            // The _lineItemAnalysisService is a GeminiLineItemAnalysisService from the Gemini namespace
            // which returns Models.Analysis.LineItemAnalysisResult
            return await _lineItemAnalysisService.AnalyzeLineItemsAsync(invoice);
        }
    }
}
