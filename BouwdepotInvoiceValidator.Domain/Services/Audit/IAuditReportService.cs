using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Domain.Models.Analysis;
using BouwdepotInvoiceValidator.Domain.Models.API;
using BouwdepotInvoiceValidator.Domain.Models.Audit;

namespace BouwdepotInvoiceValidator.Domain.Services.Audit
{
    /// <summary>
    /// Interface for services that generate and persist audit reports
    /// </summary>
    public interface IAuditReportService
    {
        /// <summary>
        /// Generates an audit report from an invoice analysis context
        /// </summary>
        /// <param name="context">The invoice analysis context</param>
        /// <returns>The generated audit report</returns>
        Task<ArchivableAuditReport> GenerateAuditReportAsync(InvoiceAnalysisContext context);

        /// <summary>
        /// Persists an audit report to storage
        /// </summary>
        /// <param name="report">The audit report to persist</param>
        /// <returns>The ID of the persisted report</returns>
        Task<string> PersistAuditReportAsync(ArchivableAuditReport report);

        /// <summary>
        /// Retrieves an audit report from storage
        /// </summary>
        /// <param name="reportId">The ID of the report to retrieve</param>
        /// <returns>The retrieved audit report, or null if not found</returns>
        Task<ArchivableAuditReport> GetAuditReportAsync(string reportId);

        /// <summary>
        /// Generates a simplified validation response from an invoice analysis context
        /// </summary>
        /// <param name="context">The invoice analysis context</param>
        /// <returns>The simplified validation response</returns>
        Task<SimplifiedValidationResponse> GenerateSimplifiedResponseAsync(InvoiceAnalysisContext context);
    }
}
