using BouwdepotInvoiceValidator.Domain.Models;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Interface for services that validate construction fund withdrawal proofs
    /// </summary>
    public interface IWithdrawalProofValidationService
    {
        /// <summary>
        /// Validates a withdrawal proof document (invoice, receipt, quotation) and returns a detailed validation result
        /// </summary>
        /// <param name="fileStream">The document file stream</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="contentType">The MIME type of the file</param>
        /// <returns>A comprehensive validation response with audit information</returns>
        Task<ValidationResult> ValidateWithdrawalProofAsync(Stream fileStream, string fileName, string contentType);
    }
}
