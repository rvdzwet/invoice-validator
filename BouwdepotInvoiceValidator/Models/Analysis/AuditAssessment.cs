using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Analysis
{
    /// <summary>
    /// Represents an audit assessment of an invoice
    /// </summary>
    public class AuditAssessment
    {
        /// <summary>
        /// Overall status of the audit assessment (Approved, Rejected, Needs Further Review)
        /// </summary>
        public string OverallStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the document is a valid invoice
        /// </summary>
        public bool ValidDocument { get; set; }
        
        /// <summary>
        /// Whether the invoice is related to home improvement
        /// </summary>
        public bool HomeImprovementRelated { get; set; }
        
        /// <summary>
        /// Confidence score of the assessment (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Specific type of home improvement
        /// </summary>
        public string ImprovementType { get; set; } = string.Empty;
        
        /// <summary>
        /// List of audit findings
        /// </summary>
        public List<AuditFinding> Findings { get; set; } = new List<AuditFinding>();
        
        /// <summary>
        /// List of required documentation
        /// </summary>
        public List<string> RequiredDocumentation { get; set; } = new List<string>();
        
        /// <summary>
        /// Summary of the audit assessment
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp of the assessment
        /// </summary>
        public DateTime AssessmentTimestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Represents an individual finding in an audit assessment
    /// </summary>
    public class AuditFinding
    {
        /// <summary>
        /// Category of the finding (Document, Relevance, Compliance, Other)
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the finding
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Severity of the finding (Info, Warning, Critical)
        /// </summary>
        public AuditFindingSeverity Severity { get; set; } = AuditFindingSeverity.Info;
        
        /// <summary>
        /// Recommended action to address the finding
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Severity level of an audit finding
    /// </summary>
    public enum AuditFindingSeverity
    {
        /// <summary>
        /// Informational finding with no impact on audit approval
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning that may require attention but does not block approval
        /// </summary>
        Warning,
        
        /// <summary>
        /// Critical issue that prevents audit approval
        /// </summary>
        Critical
    }
}
