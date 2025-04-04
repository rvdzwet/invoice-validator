using System;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Reference to an applicable regulation
    /// </summary>
    public class RegulationReference
    {
        /// <summary>
        /// Reference code for the regulation
        /// </summary>
        public string ReferenceCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the regulation
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Why this regulation applies
        /// </summary>
        public string ApplicabilityExplanation { get; set; } = string.Empty;
        
        /// <summary>
        /// Compliance status (Compliant/Non-compliant/Partially compliant)
        /// </summary>
        public string ComplianceStatus { get; set; } = string.Empty;
    }
}
