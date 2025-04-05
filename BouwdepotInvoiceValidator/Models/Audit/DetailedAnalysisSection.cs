namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Detailed analysis section with name and key-value pairs
    /// </summary>
    public class DetailedAnalysisSection
    {
        public string SectionName { get; set; } = string.Empty;
        public Dictionary<string, string> AnalysisItems { get; set; } = new Dictionary<string, string>();
    }
}
