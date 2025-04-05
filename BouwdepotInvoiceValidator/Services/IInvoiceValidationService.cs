using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
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
        
        /// <summary>
        /// Retrieves a validation result by its ID
        /// </summary>
        /// <param name="validationId">The validation result ID</param>
        /// <returns>The validation result or null if not found</returns>
        Task<ValidationResult> GetValidationResultAsync(string validationId);
        
        /// <summary>
        /// Processes additional pages of a PDF for more comprehensive analysis
        /// </summary>
        /// <param name="validationId">The validation result ID</param>
        /// <param name="startPage">The page number to start processing from (default is 2)</param>
        /// <returns>The number of additional pages processed</returns>
        Task<int> ProcessAdditionalPagesAsync(string validationId, int startPage = 2);
    }
}
