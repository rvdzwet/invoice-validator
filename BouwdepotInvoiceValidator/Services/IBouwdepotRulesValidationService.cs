using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Interface for validating invoices against Bouwdepot-specific rules
    /// </summary>
    public interface IBouwdepotRulesValidationService
    {
        /// <summary>
        /// Validates an invoice against general Bouwdepot rules
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <param name="result">The current validation result</param>
        /// <returns>Updated validation result with Bouwdepot-specific validation</returns>
        Task<ValidationResult> ValidateBouwdepotRulesAsync(Invoice invoice, ValidationResult result);
        
        /// <summary>
        /// Validates an invoice against Verduurzamingsdepot (sustainability) specific rules
        /// </summary>
        /// <param name="invoice">The invoice to validate</param>
        /// <param name="result">The current validation result</param>
        /// <returns>Updated validation result with Verduurzamingsdepot-specific validation</returns>
        Task<ValidationResult> ValidateVerduurzamingsdepotRulesAsync(Invoice invoice, ValidationResult result);
    }
}
