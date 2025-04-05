using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Implementation of IGeminiService that delegates to specialized Gemini services
    /// This is a transitional class to support both the new Gemini/* services and legacy integrations
    /// </summary>
    public class GeminiService : GeminiServiceBase, IGeminiService
    {
        private readonly ILogger<GeminiService> _logger;
        private readonly Gemini.GeminiConversationService _conversationService;
        private readonly Gemini.GeminiDocumentAnalysisService _documentService;
        private readonly Gemini.GeminiHomeImprovementService _homeImprovementService;
        private readonly Gemini.GeminiFraudDetectionService _fraudDetectionService;
        private readonly Gemini.GeminiLineItemAnalysisService _lineItemService;

        public GeminiService(
            ILogger<GeminiService> logger,
            IConfiguration configuration,
            Gemini.GeminiConversationService conversationService,
            Gemini.GeminiDocumentAnalysisService documentService,
            Gemini.GeminiHomeImprovementService homeImprovementService,
            Gemini.GeminiFraudDetectionService fraudDetectionService,
            Gemini.GeminiLineItemAnalysisService lineItemService) 
            : base(logger, configuration)
        {
            _logger = logger;
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _homeImprovementService = homeImprovementService ?? throw new ArgumentNullException(nameof(homeImprovementService));
            _fraudDetectionService = fraudDetectionService ?? throw new ArgumentNullException(nameof(fraudDetectionService));
            _lineItemService = lineItemService ?? throw new ArgumentNullException(nameof(lineItemService));
        }

        #region Conversation Methods

        /// <summary>
        /// Starts a new conversation context, making it the current active conversation
        /// </summary>
        /// <param name="metadata">Optional metadata for the conversation</param>
        /// <returns>The ID of the new conversation</returns>
        public new string StartNewConversation(Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Starting new conversation");
            return _conversationService.StartNewConversation(metadata);
        }
        
        /// <summary>
        /// Switches to an existing conversation context
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>True if the conversation was found, false otherwise</returns>
        public new bool SwitchConversation(string conversationId)
        {
            _logger.LogInformation("Switching to conversation: {ConversationId}", conversationId);
            return _conversationService.SwitchConversation(conversationId);
        }
        
        /// <summary>
        /// Clears the message history for the current conversation
        /// </summary>
        public new void ClearCurrentConversation()
        {
            _logger.LogInformation("Clearing current conversation");
            _conversationService.ClearCurrentConversation();
        }
        
        /// <summary>
        /// Gets the current conversation context
        /// </summary>
        /// <returns>The current conversation context</returns>
        public new ConversationContext GetCurrentConversation()
        {
            return _conversationService.GetCurrentConversation();
        }
        
        /// <summary>
        /// Send a prompt to Gemini in the context of an ongoing conversation
        /// </summary>
        /// <param name="prompt">The user's prompt or question</param>
        /// <param name="useHistory">Whether to include conversation history (defaults to true)</param>
        /// <returns>Gemini's response as a string</returns>
        public async Task<string> GetConversationPromptAsync(string prompt, bool useHistory = true)
        {
            _logger.LogInformation("Processing conversational prompt");
            return await _conversationService.GetConversationPromptAsync(prompt, useHistory);
        }

        #endregion

        #region Document Analysis Methods

        /// <summary>
        /// Uses Gemini AI to extract invoice data from the PDF images
        /// </summary>
        /// <param name="invoice">The invoice with page images</param>
        /// <returns>The invoice with extracted data from the images</returns>
        public async Task<Invoice> ExtractInvoiceDataFromImagesAsync(Invoice invoice)
        {
            _logger.LogInformation("Extracting invoice data from images");
            return await _documentService.ExtractInvoiceDataFromImagesAsync(invoice);
        }
        
        /// <summary>
        /// Uses Gemini AI to verify if the document is actually an invoice
        /// </summary>
        /// <param name="invoice">The extracted document data</param>
        /// <returns>A validation result with Gemini's document type assessment</returns>
        public async Task<ValidationResult> VerifyDocumentTypeAsync(Invoice invoice)
        {
            _logger.LogInformation("Verifying document type");
            return await _documentService.VerifyDocumentTypeAsync(invoice);
        }

        #endregion

        #region Home Improvement Methods

        /// <summary>
        /// Uses Gemini AI to validate if the invoice is related to home improvement
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with Gemini's assessment</returns>
        public async Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice)
        {
            _logger.LogInformation("Validating if invoice is home improvement related");
            return await _homeImprovementService.ValidateHomeImprovementAsync(invoice);
        }
        
        /// <summary>
        /// Uses Gemini AI with multi-modal capabilities to validate invoice using both text and images
        /// </summary>
        /// <param name="invoice">The extracted invoice data including page images</param>
        /// <returns>A comprehensive validation result with visual and textual analysis</returns>
        public async Task<ValidationResult> ValidateWithMultiModalAnalysisAsync(Invoice invoice)
        {
            _logger.LogInformation("Performing multi-modal validation of invoice");
            return await _homeImprovementService.ValidateWithMultiModalAnalysisAsync(invoice);
        }

        #endregion

        #region Fraud Detection Methods

        /// <summary>
        /// Uses Gemini AI to check for signs of tampering or fraud in the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <param name="detectedTampering">Initial tampering detection result from PDF analysis</param>
        /// <returns>True if Gemini detects possible fraud, otherwise false</returns>
        public async Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering)
        {
            _logger.LogInformation("Detecting fraud in invoice");
            return await _fraudDetectionService.DetectFraudAsync(invoice, detectedTampering);
        }

        #endregion

        #region Line Item Analysis Methods

        /// <summary>
        /// Uses Gemini AI to analyze the invoice line items to determine what was purchased
        /// </summary>
        /// <param name="invoice">The extracted invoice data with line items</param>
        /// <returns>A detailed analysis of the invoice line items</returns>
        public async Task<Models.Analysis.LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing invoice line items");
            
            try
            {
                // The _lineItemService is a GeminiLineItemAnalysisService from the Gemini namespace
                // which returns Models.Analysis.LineItemAnalysisResult
                return await _lineItemService.AnalyzeLineItemsAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing line items");
                
                // Return a minimal result with error information
                return new Models.Analysis.LineItemAnalysisResult
                {
                    Categories = new List<string> { "Error" },
                    PrimaryCategory = "Error",
                    HomeImprovementRelevance = 0,
                    LineItemAnalysis = new List<Models.Analysis.LineItemAnalysisDetails>(),
                    OverallAssessment = $"Error analyzing line items: {ex.Message}",
                    RawResponse = $"Error: {ex.Message}"
                };
            }
        }
        /// <summary>
        /// Uses Gemini AI to provide a detailed audit-ready assessment of the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with detailed audit information</returns>
        public async Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice)
        {
            _logger.LogInformation("Getting audit-ready assessment");
            
            try
            {
                // Create a new validation result since the method doesn't exist in _lineItemService
                var result = new ValidationResult { ExtractedInvoice = invoice };
                
                // Analyze line items first to get basic information
                var lineItemAnalysis = await AnalyzeLineItemsAsync(invoice);
                
                // Set up the validation result based on line item analysis
                result.IsValid = true;
                result.ConfidenceScore = lineItemAnalysis.HomeImprovementRelevance;
                result.DetailedReasoning = lineItemAnalysis.OverallAssessment;
                
                // Add an informational message
                result.AddIssue(ValidationSeverity.Info, 
                    $"Audit assessment based on line item analysis: {lineItemAnalysis.PrimaryCategory}");
                
                // If home improvement relevance is low, add a warning
                if (lineItemAnalysis.HomeImprovementRelevance < 50)
                {
                    result.AddIssue(ValidationSeverity.Warning, 
                        $"Low home improvement relevance: {lineItemAnalysis.HomeImprovementRelevance}%");
                    result.IsValid = false;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit-ready assessment");
                
                // Return a minimal result with error information
                var result = new ValidationResult { ExtractedInvoice = invoice };
                result.IsValid = false;
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error getting audit-ready assessment: {ex.Message}");
                
                return result;
            }
        }

        #endregion
    }
}
