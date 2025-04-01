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
        /// Uses Gemini AI to validate if the invoice is related to home improvement
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <returns>A validation result with Gemini's assessment</returns>
        Task<ValidationResult> ValidateHomeImprovementAsync(Invoice invoice);
        
        /// <summary>
        /// Uses Gemini AI to check for signs of tampering or fraud in the invoice
        /// </summary>
        /// <param name="invoice">The extracted invoice data</param>
        /// <param name="detectedTampering">Initial tampering detection result from PDF analysis</param>
        /// <returns>True if Gemini detects possible fraud, otherwise false</returns>
        Task<bool> DetectFraudAsync(Invoice invoice, bool detectedTampering);
    }
}
