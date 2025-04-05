namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Categories of fraud indicators
    /// </summary>
    public enum FraudIndicatorCategory
    {
        DocumentManipulation, 
        ContentInconsistency,
        AnomalousPricing,
        VendorIssue,
        HistoricalPattern,
        DigitalArtifact,
        ContextualMismatch,
        BehavioralFlag
    }
}
