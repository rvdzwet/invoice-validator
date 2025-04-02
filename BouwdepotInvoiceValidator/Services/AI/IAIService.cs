using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Represents information about an AI model
    /// </summary>
    public class AIModelInfo
    {
        /// <summary>
        /// The provider of the AI model (e.g., Google, OpenAI, Anthropic)
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;
        
        /// <summary>
        /// The ID of the model being used
        /// </summary>
        public string ModelId { get; set; } = string.Empty;
        
        /// <summary>
        /// The version of the model
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Maximum context length in tokens
        /// </summary>
        public int MaxContextLength { get; set; }
        
        /// <summary>
        /// Whether the model supports multi-modal inputs (text + images)
        /// </summary>
        public bool SupportsMultiModal { get; set; }
        
        /// <summary>
        /// When the model was last updated
        /// </summary>
        public string LastUpdated { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Provides a model-agnostic interface for AI services
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Validates an invoice using AI analysis
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <param name="locale">The locale to use for analysis (default: nl-NL)</param>
        /// <param name="confidenceThreshold">Optional confidence threshold for auto-approval</param>
        /// <returns>The validation result</returns>
        Task<ValidationResult> ValidateInvoiceAsync(Invoice invoice, string locale = "nl-NL", int? confidenceThreshold = null);
        
        /// <summary>
        /// Gets information about the AI model being used
        /// </summary>
        /// <returns>Information about the AI model</returns>
        AIModelInfo GetModelInfo();
    }
}
