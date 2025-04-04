using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services.Prompts;

namespace BouwdepotInvoiceValidator.Services.Gemini
{
    /// <summary>
    /// Main service that implements IGeminiService by delegating to specialized Gemini services
    /// </summary>
    public class GeminiService : IGeminiService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<GeminiService> _logger;
        private readonly GeminiConversationService _conversationService;
        private readonly GeminiDocumentAnalysisService _documentService;
        private readonly GeminiHomeImprovementService _homeImprovementService;
        private readonly GeminiFraudDetectionService _fraudDetectionService;
        private readonly GeminiLineItemAnalysisService _lineItemService;

        public GeminiService(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            PromptTemplateService promptTemplateService)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<GeminiService>();
            
            // Initialize specialized services
            _conversationService = new GeminiConversationService(
                _loggerFactory.CreateLogger<GeminiConversationService>(), configuration);
                
            _documentService = new GeminiDocumentAnalysisService(
                _loggerFactory.CreateLogger<GeminiDocumentAnalysisService>(), 
                configuration,
                promptTemplateService);
                
            _homeImprovementService = new GeminiHomeImprovementService(
                _loggerFactory.CreateLogger<GeminiHomeImprovementService>(), configuration);
                
            _fraudDetectionService = new GeminiFraudDetectionService(
                _loggerFactory.CreateLogger<GeminiFraudDetectionService>(), configuration);
                
            _lineItemService = new GeminiLineItemAnalysisService(
                _loggerFactory.CreateLogger<GeminiLineItemAnalysisService>(), configuration);
        }

        #region Conversation Methods

        /// <summary>
        /// Starts a new conversation context, making it the current active conversation
        /// </summary>
        /// <param name="metadata">Optional metadata for the conversation</param>
        /// <returns>The ID of the new conversation</returns>
        public string StartNewConversation(Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Starting new conversation");
            return _conversationService.StartNewConversation(metadata);
        }
        
        /// <summary>
        /// Switches to an existing conversation context
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>True if the conversation was found, false otherwise</returns>
        public bool SwitchConversation(string conversationId)
        {
            _logger.LogInformation("Switching to conversation: {ConversationId}", conversationId);
            return _conversationService.SwitchConversation(conversationId);
        }
        
        /// <summary>
        /// Clears the message history for the current conversation
        /// </summary>
        public void ClearCurrentConversation()
        {
            _logger.LogInformation("Clearing current conversation");
            _conversationService.ClearCurrentConversation();
        }
        
        /// <summary>
        /// Gets the current conversation context
        /// </summary>
        /// <returns>The current conversation context</returns>
        public ConversationContext GetCurrentConversation()
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
        // Corrected return type to use the specific model namespace
        public async Task<Models.Analysis.LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing invoice line items");
            return await _lineItemService.AnalyzeLineItemsAsync(invoice); // _lineItemService already returns the correct type
        }
        
        /// <summary>
        /// Uses Gemini AI to provide a detailed audit-ready assessment of the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with detailed audit information</returns>
        public async Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice)
        {
            _logger.LogInformation("Getting audit-ready assessment");
            return await _lineItemService.GetAuditReadyAssessmentAsync(invoice);
        }

        #endregion
    }
}
