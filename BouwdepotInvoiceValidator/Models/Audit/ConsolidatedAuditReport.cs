using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Simplified consolidated audit report for Bouwdepot invoice validation
    /// </summary>
    public class ConsolidatedAuditReport
    {
        // Basic Invoice Information
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Vendor Information
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public VendorBankDetails BankDetails { get; set; } = new VendorBankDetails();
        
        // Overall Assessment (Summary)
        public bool IsApproved { get; set; }
        public int OverallScore { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        
        // Validation Results (Simplified)
        public DocumentValidation DocumentValidation { get; set; } = new DocumentValidation();
        public BouwdepotEligibility BouwdepotEligibility { get; set; } = new BouwdepotEligibility();
        public List<ValidatedLineItem> LineItems { get; set; } = new List<ValidatedLineItem>();
        public List<ValidatedLineItem> ApprovedItems { get; set; } = new List<ValidatedLineItem>();
        public List<ValidatedLineItem> ReviewItems { get; set; } = new List<ValidatedLineItem>();
        
        // Detailed Analysis Sections
        public List<DetailedAnalysisSection> DetailedAnalysis { get; set; } = new List<DetailedAnalysisSection>();
        
        // Audit Trail (Compliant with European law)
        public AuditTrail AuditInformation { get; set; } = new AuditTrail();
        
        // Optional Technical Details (expandable)
        public TechnicalDetails TechnicalDetails { get; set; } = new TechnicalDetails();
    }
}
