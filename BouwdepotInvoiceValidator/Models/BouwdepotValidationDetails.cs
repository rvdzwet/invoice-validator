namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Detailed validation information for Bouwdepot-specific rules
    /// </summary>
    public class BouwdepotValidationDetails
    {
        // General Bouwdepot validation
        public bool QualityImprovementRule { get; set; }
        public bool PermanentAttachmentRule { get; set; }
        public string GeneralValidationNotes { get; set; } = string.Empty;
        
        // Verduurzamingsdepot (sustainability) specific validation
        public bool IsVerduurzamingsdepotItem { get; set; }
        public bool MeetsVerduurzamingsdepotCriteria { get; set; }
        public List<string> MatchedVerduurzamingsdepotCategories { get; set; } = new List<string>();
        
        // Line item specific validation
        public List<BouwdepotLineItemValidation> LineItemValidations { get; set; } = new List<BouwdepotLineItemValidation>();
        
        // Administrative validation
        public bool MaximumDurationRule { get; set; } = true; // Default to true as we may not have duration info
        public string AdministrativeNotes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Validation details for a specific line item
    /// </summary>
    public class BouwdepotLineItemValidation
    {
        public string Description { get; set; } = string.Empty;
        public bool IsPermanentlyAttached { get; set; }
        public bool ImproveHomeQuality { get; set; }
        public bool IsVerduurzamingsdepotItem { get; set; }
        public string VerduurzamingsdepotCategory { get; set; } = string.Empty;
        public List<string> ViolatedRules { get; set; } = new List<string>();
        public string ValidationNotes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Enumeration of sustainability categories for Verduurzamingsdepot
    /// </summary>
    public enum VerduurzamingsdepotCategory
    {
        Insulation,
        HighEfficiencyGlass,
        EnergyEfficientDoors,
        ShowerHeatRecovery,
        EnergyEfficientVentilation,
        HeatPump,
        SolarCells,
        Combination
    }
}
