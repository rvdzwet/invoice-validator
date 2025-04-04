using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Comprehensive audit report for transparency
    /// </summary>
    public class AuditReport
    {
        /// <summary>
        /// Unique audit identifier
        /// </summary>
        public string AuditIdentifier { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// When the audit was created (UTC)
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Version of the AI model used
        /// </summary>
        public string ModelVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Version of the validation system
        /// </summary>
        public string ValidatorVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Cryptographic hash of the document
        /// </summary>
        public string DocumentHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Source of the document (filename)
        /// </summary>
        public string DocumentSource { get; set; } = string.Empty;
        
        /// <summary>
        /// List of verification tests performed
        /// </summary>
        public List<string> VerificationTests { get; set; } = new List<string>();
        
        /// <summary>
        /// Executive summary of the decision
        /// </summary>
        public string ExecutiveSummary { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed assessments of each rule applied
        /// </summary>
        public List<RuleAssessment> RuleAssessments { get; set; } = new List<RuleAssessment>();
        
        /// <summary>
        /// References to applicable regulations
        /// </summary>
        public List<RegulationReference> ApplicableRegulations { get; set; } = new List<RegulationReference>();
        
        /// <summary>
        /// Key metrics about the validation
        /// </summary>
        public Dictionary<string, double> KeyMetrics { get; set; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Threshold configuration applied in this validation
        /// </summary>
        public ValidationThresholdConfiguration AppliedThresholds { get; set; } = new ValidationThresholdConfiguration();
        
        /// <summary>
        /// Factors supporting approval
        /// </summary>
        public List<string> ApprovalFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Factors raising concerns
        /// </summary>
        public List<string> ConcernFactors { get; set; } = new List<string>();
        
        /// <summary>
        /// Trace of processing events
        /// </summary>
        public List<ProcessingEvent> ProcessingEvents { get; set; } = new List<ProcessingEvent>();
        
        /// <summary>
        /// Vendor insights for audit context
        /// </summary>
        public VendorInsights VendorInsights { get; set; } = new VendorInsights();
    }
}
