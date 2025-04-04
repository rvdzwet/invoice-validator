using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Court-admissible audit trail that complies with European legal requirements
    /// </summary>
    public class AuditTrail
    {
        // Core audit information
        public string AuditIdentifier { get; set; } = Guid.NewGuid().ToString();
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;
        public string ValidationSystemVersion { get; set; } = string.Empty;
        
        // User and system information
        public string ValidatedBy { get; set; } = string.Empty;
        public string ValidatorIdentifier { get; set; } = string.Empty;
        public string ValidatorRole { get; set; } = string.Empty;
        public string SystemIPAddress { get; set; } = string.Empty;
        
        // Document integrity
        public string OriginalDocumentHash { get; set; } = string.Empty;
        public string DocumentSourceIdentifier { get; set; } = string.Empty;
        
        // Decision and reasoning
        public string ConclusionExplanation { get; set; } = string.Empty;
        public List<string> KeyFactors { get; set; } = new List<string>();
        
        // Legal references and compliance
        public List<RuleApplication> AppliedRules { get; set; } = new List<RuleApplication>();
        public List<LegalReference> LegalBasis { get; set; } = new List<LegalReference>();
        
        // Chain of custody
        public List<ProcessingStep> ProcessingSteps { get; set; } = new List<ProcessingStep>();
        
        // Digital signature
        public string AuditSignature { get; set; } = string.Empty;
        public string SignatureCertificateThumbprint { get; set; } = string.Empty;
    }
}
