namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Reference to a specific legal or regulatory basis for a decision
    /// </summary>
    public class LegalReference
    {
        public string ReferenceCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Applicability { get; set; } = string.Empty;
    }
}
