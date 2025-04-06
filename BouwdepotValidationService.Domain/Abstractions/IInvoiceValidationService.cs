using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Domain.Abstractions
{
    /// <summary>
    /// Interface for the invoice validation service
    /// </summary>
    public interface IInvoiceValidationService
    {
        /// <summary>
        /// Validates a PDF invoice file
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>A task representing the validation operation with the validation result</returns>
        Task<ValidationResult> ValidateInvoiceAsync(Stream fileStream, string fileName);
    }
}
