using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service interface for generating consolidated audit reports from validation results
    /// </summary>
    public interface IAuditReportService
    {
        /// <summary>
        /// Generates a consolidated audit report from a validation result
        /// </summary>
        /// <param name="validationResult">The detailed validation result</param>
        /// <returns>A consolidated audit report</returns>
        Task<ConsolidatedAuditReport> GenerateAuditReportAsync(ValidationResult validationResult);
    }
}
