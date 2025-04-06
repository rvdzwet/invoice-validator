using BouwdepotInvoiceValidator.Domain.Models;

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
        Task<ValidationResult> ValidateInvoiceAsync(Stream fileStream, string fileName, string contentType);

    }
}
