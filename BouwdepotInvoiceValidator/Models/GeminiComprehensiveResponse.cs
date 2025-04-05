namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Comprehensive response model that consolidates various aspects of Gemini AI analysis
    /// </summary>
    public class GeminiComprehensiveResponse
    {
        // Document Analysis
        public bool IsValidInvoice { get; set; }
        public string DetectedDocumentType { get; set; }
        public double DocumentValidationConfidence { get; set; }
        public List<string> PresentInvoiceElements { get; set; } = new List<string>();
        public List<string> MissingInvoiceElements { get; set; } = new List<string>();
        
        // Home Improvement Analysis
        public bool IsHomeImprovement { get; set; }
        public double HomeImprovementConfidence { get; set; }
        public string HomeImprovementCategory { get; set; }
        public string HomeImprovementExplanation { get; set; }
        public List<string> HomeImprovementKeywords { get; set; } = new List<string>();
        
        // Fraud Detection
        public bool PossibleFraud { get; set; }
        public string FraudRiskLevel { get; set; }
        public double FraudDetectionConfidence { get; set; }
        public List<string> FraudIndicators { get; set; } = new List<string>();
        public string FraudAssessmentExplanation { get; set; }
        public List<string> FraudRecommendedActions { get; set; } = new List<string>();
        
        // Line Item Analysis
        public Analysis.LineItemAnalysisResult LineItemAnalysis { get; set; } = new Analysis.LineItemAnalysisResult();
        
        // Multi-modal Analysis
        public List<Analysis.DetailedVisualAssessment> VisualAssessments { get; set; } = new List<Analysis.DetailedVisualAssessment>();
        public bool HasVisualAnomalies { get; set; }
        
        // Audit Assessment
        public Analysis.AuditAssessment AuditAssessment { get; set; } = new Analysis.AuditAssessment();
        
        // Overall Assessment
        public bool IsValid { get; set; }
        public int OverallConfidenceScore { get; set; }
        public string DetailedReasoning { get; set; }
        public string RecommendedAction { get; set; }
        public List<ValidationIssue> ValidationIssues { get; set; } = new List<ValidationIssue>();
        
        // Raw Responses
        public string DocumentAnalysisResponse { get; set; }
        public string HomeImprovementResponse { get; set; }
        public string FraudDetectionResponse { get; set; }
        public string LineItemAnalysisResponse { get; set; }
        public string MultiModalResponse { get; set; }
        public string AuditAssessmentResponse { get; set; }
        
        // Timestamps
        public DateTime AnalysisStartTime { get; set; } = DateTime.Now;
        public DateTime AnalysisCompletionTime { get; set; }
        public TimeSpan AnalysisDuration => AnalysisCompletionTime - AnalysisStartTime;
        
        // Methods
        public void MarkAnalysisComplete()
        {
            AnalysisCompletionTime = DateTime.Now;
        }
        
        public void AddValidationIssue(ValidationSeverity severity, string message)
        {
            ValidationIssues.Add(new ValidationIssue
            {
                Severity = severity,
                Message = message
                // Timestamp = DateTime.Now // Removed Timestamp assignment as it doesn't exist on ValidationIssue
            });
        }

        public void SummarizeResults()
        {
            // Calculate overall confidence based on individual confidences
            double totalConfidence = 0;
            int confidenceFactors = 0;
            
            if (DocumentValidationConfidence > 0)
            {
                totalConfidence += DocumentValidationConfidence;
                confidenceFactors++;
            }
            
            if (HomeImprovementConfidence > 0)
            {
                totalConfidence += HomeImprovementConfidence;
                confidenceFactors++;
            }
            
            if (FraudDetectionConfidence > 0)
            {
                // Invert fraud confidence (higher fraud confidence = lower overall confidence)
                totalConfidence += (1 - FraudDetectionConfidence);
                confidenceFactors++;
            }
            
            if (LineItemAnalysis?.HomeImprovementRelevance > 0)
            {
                totalConfidence += (LineItemAnalysis.HomeImprovementRelevance / 100.0);
                confidenceFactors++;
            }
            
            OverallConfidenceScore = confidenceFactors > 0 
                ? (int)(totalConfidence / confidenceFactors * 100) 
                : 0;
            
            // Determine overall validity
            bool documentIsValid = IsValidInvoice;
            bool contentIsValid = IsHomeImprovement && 
                                 (LineItemAnalysis?.HomeImprovementRelevance > 50);
            bool noFraud = !PossibleFraud;
            
            IsValid = documentIsValid && contentIsValid && noFraud;
            
            // Set completion time if not already set
            if (AnalysisCompletionTime == default)
            {
                MarkAnalysisComplete();
            }
        }
    }
}
