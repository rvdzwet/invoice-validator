using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Analysis
{
    /// <summary>
    /// Represents the result of analyzing line items in an invoice
    /// </summary>
    public class LineItemAnalysisResult
    {
        /// <summary>
        /// Categories identified in the line items
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();
        
        /// <summary>
        /// Primary category of the invoice
        /// </summary>
        public string PrimaryCategory { get; set; } = string.Empty;
        
        /// <summary>
        /// How relevant the invoice is to home improvement (0-100)
        /// </summary>
        public int HomeImprovementRelevance { get; set; }
        
        /// <summary>
        /// Detailed analysis for each line item
        /// </summary>
        public List<LineItemAnalysisDetails> LineItemAnalysis { get; set; } = new List<LineItemAnalysisDetails>();
        
        /// <summary>
        /// Overall assessment of the invoice line items
        /// </summary>
        public string OverallAssessment { get; set; } = string.Empty;
        
        /// <summary>
        /// Raw response from the Gemini AI
        /// </summary>
        public string RawResponse { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Details from analyzing a single line item
    /// </summary>
    public class LineItemAnalysisDetails
    {
        /// <summary>
        /// Original line item description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the item is related to home improvement
        /// </summary>
        public bool IsHomeImprovement { get; set; }
        
        /// <summary>
        /// Category of the item (plumbing, electrical, etc.)
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed explanation of the analysis
        /// </summary>
        public string Explanation { get; set; } = string.Empty;
        
        /// <summary>
        /// Assessment of the pricing (reasonable, high, low)
        /// </summary>
        public string PricingAssessment { get; set; } = "Unknown";
    }
}
