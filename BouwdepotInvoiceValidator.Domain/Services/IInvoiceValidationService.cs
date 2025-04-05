using BouwdepotInvoiceValidator.Domain.Models.Analysis;
using BouwdepotInvoiceValidator.Domain.Models.API;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Interface for services that validate invoices
    /// </summary>
    public interface IInvoiceValidationService
    {
        /// <summary>
        /// Validates an invoice and returns a simplified response
        /// </summary>
        /// <param name="fileStream">The invoice file stream</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="contentType">The MIME type of the file</param>
        /// <returns>A simplified validation response</returns>
        Task<SimplifiedValidationResponse> ValidateInvoiceAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Analyzes an invoice and returns the full analysis context
        /// </summary>
        /// <param name="fileStream">The invoice file stream</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="contentType">The MIME type of the file</param>
        /// <returns>The invoice analysis context</returns>
        Task<InvoiceAnalysisContext> AnalyzeInvoiceAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Gets a validation result by ID
        /// </summary>
        /// <param name="validationId">The ID of the validation result to retrieve</param>
        /// <returns>The validation result, or null if not found</returns>
        Task<SimplifiedValidationResponse> GetValidationResultAsync(string validationId);

        /// <summary>
        /// Gets an analysis context by ID
        /// </summary>
        /// <param name="analysisId">The ID of the analysis context to retrieve</param>
        /// <returns>The analysis context, or null if not found</returns>
        Task<InvoiceAnalysisContext> GetAnalysisContextAsync(string analysisId);
    }
}
