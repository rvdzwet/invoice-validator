using System;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Bouwdepot eligibility assessment in a simplified format
    /// </summary>
    public class BouwdepotEligibility
    {
        public bool IsEligible { get; set; }
        public int Score { get; set; }
        public bool MeetsQualityImprovement { get; set; }
        public bool MeetsPermanentAttachment { get; set; }
        public string ConstructionType { get; set; } = string.Empty;
        public string PaymentPriority { get; set; } = string.Empty;
        public string SpecialConditions { get; set; } = string.Empty;
    }
}
