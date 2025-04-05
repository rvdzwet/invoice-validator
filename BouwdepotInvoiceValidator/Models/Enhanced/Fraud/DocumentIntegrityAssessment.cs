namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Assessment of document integrity
    /// </summary>
    public class DocumentIntegrityAssessment
    {
        /// <summary>
        /// Whether digital tampering was detected
        /// </summary>
        public bool HasDigitalTampering { get; set; }
        
        /// <summary>
        /// Whether inconsistent formatting was detected
        /// </summary>
        public bool HasInconsistentFormatting { get; set; }
        
        /// <summary>
        /// Whether disrupted text flow was detected
        /// </summary>
        public bool HasDisruptedTextFlow { get; set; }
        
        /// <summary>
        /// Whether misaligned elements were detected
        /// </summary>
        public bool HasMisalignedElements { get; set; }
        
        /// <summary>
        /// Whether font inconsistencies were detected
        /// </summary>
        public bool HasFontInconsistencies { get; set; }
        
        /// <summary>
        /// Whether suspicious metadata was detected
        /// </summary>
        public bool HasSuspiciousMetadata { get; set; }
        
        /// <summary>
        /// Results of digital analysis
        /// </summary>
        public List<string> DigitalAnalysisResults { get; set; } = new List<string>();
    }
}
