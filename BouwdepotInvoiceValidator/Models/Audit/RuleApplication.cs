using System;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Record of a specific rule application in the validation process
    /// </summary>
    public class RuleApplication
    {
        public string RuleId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string RuleDescription { get; set; } = string.Empty;
        public bool WasSatisfied { get; set; }
        public string EvidenceReference { get; set; } = string.Empty;
    }
}
