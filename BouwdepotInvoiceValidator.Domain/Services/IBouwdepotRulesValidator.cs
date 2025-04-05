using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Interface for services that validate invoices against Bouwdepot-specific rules
    /// </summary>
    public interface IBouwdepotRulesValidator
    {
        /// <summary>
        /// Validates an invoice against Bouwdepot-specific rules
        /// </summary>
        /// <param name="context">The invoice analysis context containing the extracted invoice data</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method should:
        /// 1. Apply all relevant Bouwdepot validation rules to the invoice
        /// 2. Add validation results to context.ValidationResults
        /// 3. Set context.OverallOutcome based on the validation results
        /// 4. Set context.OverallOutcomeSummary with an explanation (in context.DetectedLanguage)
        /// 5. Add a processing step to the context log
        /// </remarks>
        Task ValidateInvoiceAsync(InvoiceAnalysisContext context);
    }
}
