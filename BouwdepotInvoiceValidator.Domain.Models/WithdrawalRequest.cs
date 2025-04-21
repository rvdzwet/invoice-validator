namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Represents a withdrawal request from a construction fund depot
    /// </summary>
    public class WithdrawalRequest
    {
        /// <summary>
        /// Unique identifier for the withdrawal request
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Identifier for the construction fund
        /// </summary>
        public string ConstructionFundId { get; set; }

        /// <summary>
        /// Customer reference or account number
        /// </summary>
        public string CustomerReference { get; set; }

        /// <summary>
        /// Amount requested for withdrawal
        /// </summary>
        public decimal RequestedAmount { get; set; }

        /// <summary>
        /// Currency code (default is EUR)
        /// </summary>
        public string Currency { get; set; } = "EUR";

        /// <summary>
        /// Timestamp when the request was submitted
        /// </summary>
        public DateTimeOffset RequestTimestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// List of document types included in this withdrawal request
        /// (e.g. invoice, receipt, quotation, project plan)
        /// </summary>
        public List<string> IncludedDocumentTypes { get; set; } = new List<string>();

        /// <summary>
        /// Notes or additional information provided with the request
        /// </summary>
        public string AdditionalNotes { get; set; }
    }
}
