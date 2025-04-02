using System.Collections.Generic;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Interface for the Gemini AI service
    /// </summary>
    public interface IGeminiService
    {
        /// <summary>
        /// Starts a new conversation context, making it the current active conversation
        /// </summary>
        /// <param name="metadata">Optional metadata for the conversation</param>
        /// <returns>The ID of the new conversation</returns>
        string StartNewConversation(Dictionary<string, string> metadata = null);
        
        /// <summary>
        /// Switches to an existing conversation context
        /// </summary>
        /// <param name="conversationId">ID of the conversation to switch to</param>
        /// <returns>True if the conversation was found, false otherwise</returns>
        bool SwitchConversation(string conversationId);
        
        /// <summary>
        /// Clears the message history for the current conversation
        /// </summary>
        void ClearCurrentConversation();
        
        /// <summary>
        /// Gets the current conversation context
        /// </summary>
        /// <returns>The current conversation context</returns>
        ConversationContext GetCurrentConversation();
        
        /// <summary>
        /// Send a prompt to Gemini in the context of an ongoing conversation
        /// </summary>
        /// <param name="prompt">The user's prompt or question</param>
        /// <param name="useHistory">Whether to include conversation history (defaults to true)</param>
        /// <returns>Gemini's response as a string</returns>
        Task<string> GetConversationPromptAsync(string prompt, bool useHistory = true);
        /// <summary>
        /// Uses Gemini AI to extract invoice data from the PDF images
        /// </summary>
        /// <param name="invoice">The invoice with page images</param>
        /// <returns>The invoice with extracted data from the images</returns>
        Task<Invoice> ExtractInvoiceDataFromImagesAsync(Invoice invoice);
        
        /// <summary>
        /// Uses Gemini AI to verify if the document is actually an invoice
        /// </summary>
        /// <param name="invoice">The extracted document data</param>
        /// <returns>A validation result with Gemini's document type assessment</returns>
        Task<ValidationResult> VerifyDocumentTypeAsync(Invoice invoice);
        
        /// <summary>
        /// Uses Gemini AI to validate if the invoice is related to home improvement
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with Gemini's assessment</returns>
        Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice);
        
        /// <summary>
        /// Uses Gemini AI to analyze the invoice line items to determine what was purchased
        /// </summary>
        /// <param name="invoice">The extracted invoice data with line items</param>
        /// <returns>A detailed analysis of the invoice line items</returns>
        // Corrected return type to use the specific model namespace
        Task<Models.Analysis.LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice);

        /// <summary>
        /// Uses Gemini AI to check for signs of tampering or fraud in the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <param name="detectedTampering">Initial tampering detection result from PDF analysis</param>
        /// <returns>True if Gemini detects possible fraud, otherwise false</returns>
        Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering);
        
        /// <summary>
        /// Uses Gemini AI with multi-modal capabilities to validate invoice using both text and images
        /// </summary>
        /// <param name="invoice">The extracted invoice data including page images</param>
        /// <returns>A comprehensive validation result with visual and textual analysis</returns>
        Task<ValidationResult> ValidateWithMultiModalAnalysisAsync(Invoice invoice);
        
        /// <summary>
        /// Uses Gemini AI to provide a detailed audit-ready assessment of the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with detailed audit information</returns>
        Task<ValidationResult> GetAuditReadyAssessmentAsync(Invoice invoice);
    }
}
