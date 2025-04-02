using System;

namespace BouwdepotInvoiceValidator.Models.Analysis
{
    /// <summary>
    /// Extended visual assessment of an invoice image with additional fields beyond the base VisualAssessment
    /// </summary>
    public class DetailedVisualAssessment
    {
        /// <summary>
        /// Page number of the assessment
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Overall quality of the image (readable, partial, poor)
        /// </summary>
        public string ImageQuality { get; set; } = string.Empty;
        
        /// <summary>
        /// Visual elements identified in the image
        /// </summary>
        public string[] IdentifiedElements { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Detected anomalies or issues with the image
        /// </summary>
        public string[] DetectedAnomalies { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Confidence score for the assessment (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Textual assessment of the image
        /// </summary>
        public string Assessment { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether tampering or alterations were detected
        /// </summary>
        public bool TamperingDetected { get; set; }
        
        /// <summary>
        /// Description of any tampering detected
        /// </summary>
        public string TamperingDescription { get; set; } = string.Empty;
    }
}
