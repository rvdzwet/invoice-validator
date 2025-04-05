using BouwdepotInvoiceValidator.Models;
// using BouwdepotInvoiceValidator.Models.Analysis; // Remove this using statement

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for validating invoices against Bouwdepot-specific rules
    /// </summary>
    public class BouwdepotRulesValidationService : IBouwdepotRulesValidationService
    {
        private readonly ILogger<BouwdepotRulesValidationService> _logger;
        
        // Lists of keywords for different categories (could be moved to configuration)
        private static readonly HashSet<string> VerduurzamingsdepotKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Insulation
            "isolatie", "isolation", "insulation", "gevelisolatie", "dakisolatie", "vloerisolatie", 
            "leidingisolatie", "muurisolatie", "spouwmuurisolatie", "wall insulation", "roof insulation",
            "floor insulation", "pipe insulation", "isoleren",
            
            // High-efficiency glass
            "HR++", "HR glas", "hoog rendement", "high efficiency glass", "dubbel glas", "triple glas",
            "double glazing", "triple glazing", "isolatieglas", "thermopane", "low-e", "low emissivity",
            
            // Energy-efficient doors/windows
            "energy efficient", "energiezuinig", "deuren", "kozijnen", "ramen", "windows", "doors",
            "frames", "energy saving", "energiebesparend",
            
            // Shower heat recovery
            "douche-warmteterugwinning", "warmteterugwinning", "heat recovery", "shower heat recovery",
            "douchepijp-wtw", "douchebak-wtw", "douchegoot-wtw",
            
            // Energy-efficient ventilation
            "ventilatie", "ventilation", "energy efficient ventilation", "balanced ventilation",
            "gebalanceerde ventilatie", "mechanische ventilatie", "wtw-unit", "warmteterugwinunit",
            "heat recovery ventilation", "HRV",
            
            // Heat pumps, solar cells
            "warmtepomp", "heat pump", "zonnecellen", "solar cells", "zonnepanelen", "solar panels",
            "PV", "photovoltaic", "lucht/water", "air/water", "ground source", "aardwarmte",
            "bodem/water", "hybride", "hybrid", "gasloze verwarming", "gas-free heating"
        };
        
        private static readonly HashSet<string> PermanentAttachmentKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Installation terms
            "installatie", "installation", "bevestiging", "montage", "mounting", "securing",
            "vastmaken", "inbouw", "built-in", "integrated", "verankering", "anchoring",
            
            // Construction and building terms
            "inbouwen", "afwerking", "finishing", "constructie", "construction", "renovatie", 
            "renovation", "verbouwing", "remodeling", "opbouw", "vaste", "fixed", "permanente",
            "permanent", "bouwen", "build", "aanbouw", "extension", "opbouwen", 
            
            // Mounting/fixing terms
            "vastschroeven", "screwed in", "verlijmen", "glued", "gemonteerd", "mounted",
            "vaste constructie", "fixed construction", "verwerkt in", "integrated into",
            "ingebouwd", "built-in", "vastgezet", "secured",
            
            // Home parts
            "muur", "wall", "vloer", "floor", "plafond", "ceiling", "dak", "roof",
            "fundering", "foundation", "gevel", "facade", "structureel", "structural"
        };
        
        // Lists for sustainability categories
        private static readonly Dictionary<VerduurzamingsdepotCategory, HashSet<string>> CategoryKeywords = 
            new Dictionary<VerduurzamingsdepotCategory, HashSet<string>>
        {
            { VerduurzamingsdepotCategory.Insulation, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "isolatie", "insulation", "gevelisolatie", "dakisolatie", "vloerisolatie", 
                "leidingisolatie", "muurisolatie", "spouwmuurisolatie", "isoleren", "wall insulation", 
                "roof insulation", "floor insulation", "pipe insulation", "thermal insulation"
            }},
            { VerduurzamingsdepotCategory.HighEfficiencyGlass, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "HR++", "HR glas", "hoog rendement", "high efficiency glass", "dubbel glas", 
                "triple glas", "double glazing", "triple glazing", "isolatieglas", "thermopane",
                "low-e", "low emissivity"
            }},
            { VerduurzamingsdepotCategory.EnergyEfficientDoors, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "energiezuinige deuren", "energy efficient doors", "geïsoleerde deuren", "insulated doors",
                "geïsoleerde kozijnen", "insulated frames", "hr++ kozijnen", "high efficiency frames",
                "energiebesparende ramen", "energy saving windows"
            }},
            { VerduurzamingsdepotCategory.ShowerHeatRecovery, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "douche-warmteterugwinning", "warmteterugwinning", "heat recovery", "shower heat recovery",
                "douchepijp-wtw", "douchebak-wtw", "douchegoot-wtw", "douche wtw"
            }},
            { VerduurzamingsdepotCategory.EnergyEfficientVentilation, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "energiezuinig ventilatiesysteem", "energy efficient ventilation", "balanced ventilation",
                "gebalanceerde ventilatie", "mechanische ventilatie", "wtw-unit", "warmteterugwinunit",
                "heat recovery ventilation", "HRV", "ventilatie"
            }},
            { VerduurzamingsdepotCategory.HeatPump, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "warmtepomp", "heat pump", "lucht/water", "air/water", "ground source", "aardwarmte",
                "bodem/water", "hybride", "hybrid", "gasloze verwarming", "gas-free heating", "water/water"
            }},
            { VerduurzamingsdepotCategory.SolarCells, new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "zonnecellen", "solar cells", "zonnepanelen", "solar panels", "PV", "photovoltaic",
                "zonnemodules", "solar modules", "solar system", "zonnesysteem", "solar installation"
            }}
        };
        
        public BouwdepotRulesValidationService(ILogger<BouwdepotRulesValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc />
        public async Task<ValidationResult> ValidateBouwdepotRulesAsync(Invoice invoice, ValidationResult result)
        {
            _logger.LogInformation("Validating invoice against general Bouwdepot rules: {FileName}", invoice.FileName);
            
            // Default to false until proven compliant
            result.IsBouwdepotCompliant = false;
            
            try
            {
                // Check if we have meaningful line items to analyze
                if (invoice.LineItems == null || invoice.LineItems.Count == 0)
                {
                    _logger.LogWarning("No line items available for Bouwdepot validation: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Cannot validate against Bouwdepot rules - no line items available.");
                    result.BouwdepotValidation.GeneralValidationNotes = 
                        "Unable to validate - insufficient line item details available.";
                    return result;
                }
                
                // Initialize validation variables
                bool qualityImprovementRulePassed = false;
                bool permanentAttachmentRulePassed = false;
                
                // Use purchase analysis if available
                if (result.PurchaseAnalysis != null && result.PurchaseAnalysis.LineItemDetails.Count > 0)
                {
                    _logger.LogInformation("Using existing purchase analysis for Bouwdepot validation");
                    
                    // Check each line item against Bouwdepot rules
                    foreach (var item in result.PurchaseAnalysis.LineItemDetails)
                    {
                        var lineItemValidation = new BouwdepotLineItemValidation
                        {
                            Description = item.Description,
                            ValidationNotes = string.Empty
                        };
                        
                        // 1. Check permanent attachment rule
                        bool isPermanentlyAttached = IsPermanentlyAttached(item);
                        lineItemValidation.IsPermanentlyAttached = isPermanentlyAttached;
                        
                        if (!isPermanentlyAttached)
                        {
                            lineItemValidation.ViolatedRules.Add("PermanentAttachment");
                            lineItemValidation.ValidationNotes = 
                                "Item does not appear to be permanently attached to the house.";
                        }
                        
                        // 2. Check quality improvement rule
                        bool improvesQuality = ImprovesHomeQuality(item);
                        lineItemValidation.ImproveHomeQuality = improvesQuality;
                        
                        if (!improvesQuality)
                        {
                            lineItemValidation.ViolatedRules.Add("QualityImprovement");
                            
                            string notes = string.IsNullOrEmpty(lineItemValidation.ValidationNotes)
                                ? "Item does not appear to improve home quality or value."
                                : lineItemValidation.ValidationNotes + " Additionally, it does not appear to improve home quality or value.";
                                
                            lineItemValidation.ValidationNotes = notes;
                        }
                        
                        // Add the line item validation to our collection
                        result.BouwdepotValidation.LineItemValidations.Add(lineItemValidation);
                        
                        // Update overall rules status
                        if (isPermanentlyAttached)
                        {
                            permanentAttachmentRulePassed = true;
                        }
                        
                        if (improvesQuality)
                        {
                            qualityImprovementRulePassed = true;
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No purchase analysis available, analyzing raw line items for Bouwdepot validation");
                    
                    // Check each raw line item
                    foreach (var item in invoice.LineItems)
                    {
                        var lineItemValidation = new BouwdepotLineItemValidation
                        {
                            Description = item.Description,
                            ValidationNotes = string.Empty
                        };
                        
                        // Use direct text analysis for raw line items
                        bool isPermanentlyAttached = IsPermanentlyAttachedText(item.Description);
                        lineItemValidation.IsPermanentlyAttached = isPermanentlyAttached;
                        
                        if (!isPermanentlyAttached)
                        {
                            lineItemValidation.ViolatedRules.Add("PermanentAttachment");
                            lineItemValidation.ValidationNotes = 
                                "Item does not appear to be permanently attached to the house.";
                        }
                        
                        // Simplified quality improvement check for raw text
                        bool improvesQuality = ImprovesHomeQualityText(item.Description);
                        lineItemValidation.ImproveHomeQuality = improvesQuality;
                        
                        if (!improvesQuality)
                        {
                            lineItemValidation.ViolatedRules.Add("QualityImprovement");
                            
                            string notes = string.IsNullOrEmpty(lineItemValidation.ValidationNotes)
                                ? "Item does not appear to improve home quality or value."
                                : lineItemValidation.ValidationNotes + " Additionally, it does not appear to improve home quality or value.";
                                
                            lineItemValidation.ValidationNotes = notes;
                        }
                        
                        // Add the line item validation to our collection
                        result.BouwdepotValidation.LineItemValidations.Add(lineItemValidation);
                        
                        // Update overall rules status
                        if (isPermanentlyAttached)
                        {
                            permanentAttachmentRulePassed = true;
                        }
                        
                        if (improvesQuality)
                        {
                            qualityImprovementRulePassed = true;
                        }
                    }
                }
                
                // Set the overall validation results
                result.BouwdepotValidation.QualityImprovementRule = qualityImprovementRulePassed;
                result.BouwdepotValidation.PermanentAttachmentRule = permanentAttachmentRulePassed;
                
                // An invoice is compliant if at least one of the items meets both rules
                bool anyItemFullyCompliant = result.BouwdepotValidation.LineItemValidations
                    .Any(item => item.IsPermanentlyAttached && item.ImproveHomeQuality);
                
                result.IsBouwdepotCompliant = anyItemFullyCompliant;
                
                // Add compliance issues based on rule results
                if (!permanentAttachmentRulePassed)
                {
                    _logger.LogWarning("Invoice failed permanent attachment rule: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Error, 
                        "No items appear to be permanently attached to the house as required by Bouwdepot rules.");
                }
                
                if (!qualityImprovementRulePassed)
                {
                    _logger.LogWarning("Invoice failed quality improvement rule: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Error, 
                        "No items appear to improve home quality or value as required by Bouwdepot rules.");
                }
                
                if (result.IsBouwdepotCompliant)
                {
                    _logger.LogInformation("Invoice is compliant with Bouwdepot rules: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Info, 
                        "Invoice contains items that meet the Bouwdepot requirements.");
                    
                    result.BouwdepotValidation.GeneralValidationNotes = 
                        "This invoice contains items that meet the Bouwdepot requirements for home improvements.";
                }
                else
                {
                    _logger.LogWarning("Invoice is not compliant with Bouwdepot rules: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Error, 
                        "Invoice does not meet the Bouwdepot requirements for home improvements.");
                    
                    result.BouwdepotValidation.GeneralValidationNotes = 
                        "This invoice does not contain items that meet all required Bouwdepot criteria.";
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice against Bouwdepot rules: {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating against Bouwdepot rules: {ex.Message}");
                result.BouwdepotValidation.GeneralValidationNotes = 
                    $"An error occurred during validation: {ex.Message}";
                return result;
            }
        }
        
        /// <inheritdoc />
        public async Task<ValidationResult> ValidateVerduurzamingsdepotRulesAsync(Invoice invoice, ValidationResult result)
        {
            _logger.LogInformation("Validating invoice against Verduurzamingsdepot rules: {FileName}", invoice.FileName);
            
            // Default to false until proven compliant
            result.IsVerduurzamingsdepotCompliant = false;
            
            try
            {
                // Check if we have meaningful line items to analyze
                if (invoice.LineItems == null || invoice.LineItems.Count == 0)
                {
                    _logger.LogWarning("No line items available for Verduurzamingsdepot validation: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Cannot validate against Verduurzamingsdepot rules - no line items available.");
                    return result;
                }
                
                // Identify matching sustainability categories
                var matchedCategories = new HashSet<VerduurzamingsdepotCategory>();
                
                // Get existing line item validations or create new ones
                if (result.BouwdepotValidation.LineItemValidations.Count == 0)
                {
                    // Create line item validations if they don't exist yet
                    if (result.PurchaseAnalysis != null && result.PurchaseAnalysis.LineItemDetails.Count > 0)
                    {
                        foreach (var item in result.PurchaseAnalysis.LineItemDetails)
                        {
                            result.BouwdepotValidation.LineItemValidations.Add(new BouwdepotLineItemValidation
                            {
                                Description = item.Description
                            });
                        }
                    }
                    else if (invoice.LineItems != null)
                    {
                        foreach (var item in invoice.LineItems)
                        {
                            result.BouwdepotValidation.LineItemValidations.Add(new BouwdepotLineItemValidation
                            {
                                Description = item.Description
                            });
                        }
                    }
                }
                
                // Now process each line item to check for sustainability criteria
                for (int i = 0; i < result.BouwdepotValidation.LineItemValidations.Count; i++)
                {
                    var validation = result.BouwdepotValidation.LineItemValidations[i];
                    
                    // Get corresponding purchase analysis if available
                    Models.LineItemAnalysisDetail? analysisDetail = null;
                    if (result.PurchaseAnalysis != null && 
                        i < result.PurchaseAnalysis.LineItemDetails.Count)
                    {
                        analysisDetail = result.PurchaseAnalysis.LineItemDetails[i];
                    }
                    
                    // Check for sustainability item
                    var sustainabilityCategory = GetSustainabilityCategory(validation.Description, analysisDetail);
                    
                    if (sustainabilityCategory != null)
                    {
                        validation.IsVerduurzamingsdepotItem = true;
                        validation.VerduurzamingsdepotCategory = sustainabilityCategory.ToString();
                        matchedCategories.Add(sustainabilityCategory.Value);
                        
                        _logger.LogInformation("Line item matched sustainability category {Category}: {Description}", 
                            sustainabilityCategory, validation.Description);
                    }
                    else
                    {
                        validation.IsVerduurzamingsdepotItem = false;
                        
                        // Check if the item needs to be combined with HR++ glass
                        if (DoesRequireHRGlass(validation.Description, analysisDetail))
                        {
                            validation.ValidationNotes += "This item can only be claimed under Verduurzamingsdepot if installed in combination with HR++ glass. ";
                            validation.ViolatedRules.Add("RequiresHRGlass");
                        }
                    }
                }
                
                // Add the identified categories to the result
                foreach (var category in matchedCategories)
                {
                    result.BouwdepotValidation.MatchedVerduurzamingsdepotCategories.Add(category.ToString());
                }
                
                // Check if any items qualify for Verduurzamingsdepot
                result.BouwdepotValidation.IsVerduurzamingsdepotItem = matchedCategories.Count > 0;
                
                // Check for combination compliance
                bool combinationValid = true;
                if (matchedCategories.Contains(VerduurzamingsdepotCategory.EnergyEfficientDoors))
                {
                    // Check if HR++ glass is also present
                    bool hasHRGlass = matchedCategories.Contains(VerduurzamingsdepotCategory.HighEfficiencyGlass);
                    if (!hasHRGlass)
                    {
                        combinationValid = false;
                        _logger.LogWarning("Energy efficient doors/windows found without required HR++ glass: {FileName}", invoice.FileName);
                        result.AddIssue(ValidationSeverity.Error, 
                            "Energy efficient doors or windows must be combined with HR++ glass to qualify for Verduurzamingsdepot.");
                    }
                }
                
                // Set overall compliance status
                result.BouwdepotValidation.MeetsVerduurzamingsdepotCriteria = matchedCategories.Count > 0 && combinationValid;
                result.IsVerduurzamingsdepotCompliant = result.BouwdepotValidation.MeetsVerduurzamingsdepotCriteria;
                
                // Add summary information
                if (result.IsVerduurzamingsdepotCompliant)
                {
                    _logger.LogInformation("Invoice is compliant with Verduurzamingsdepot rules: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Info, 
                        $"Invoice qualifies for Verduurzamingsdepot under categories: {string.Join(", ", matchedCategories)}");
                }
                else if (matchedCategories.Count > 0)
                {
                    _logger.LogWarning("Invoice contains sustainability items but doesn't meet all requirements: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Invoice contains sustainability items but doesn't meet all Verduurzamingsdepot requirements.");
                }
                else
                {
                    _logger.LogWarning("Invoice does not contain any eligible Verduurzamingsdepot items: {FileName}", invoice.FileName);
                    result.AddIssue(ValidationSeverity.Warning, 
                        "Invoice does not contain any eligible items for Verduurzamingsdepot funding.");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice against Verduurzamingsdepot rules: {FileName}", invoice.FileName);
                result.AddIssue(ValidationSeverity.Error, 
                    $"Error validating against Verduurzamingsdepot rules: {ex.Message}");
                return result;
            }
        }
        
        #region Helper Methods
        
        private bool IsPermanentlyAttached(Models.LineItemAnalysisDetail item)
        {
            // If we have specific interpretation
            if (!string.IsNullOrEmpty(item.InterpretedAs))
            {
                return IsPermanentlyAttachedText(item.InterpretedAs);
            }
            
            // If we have category information
            if (!string.IsNullOrEmpty(item.Category))
            {
                // Some categories are inherently permanently attached
                if (item.Category.Contains("fixed", StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains("installed", StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains("built-in", StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains("structural", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            // Check the description
            return IsPermanentlyAttachedText(item.Description);
        }
        
        private bool IsPermanentlyAttachedText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            
            // Check against our list of keywords
            foreach (var keyword in PermanentAttachmentKeywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            // Some categories are inherently permanently attached
            string lowerText = text.ToLowerInvariant();
            if (lowerText.Contains("installatie") || lowerText.Contains("installation") ||
                lowerText.Contains("renovatie") || lowerText.Contains("renovation") ||
                lowerText.Contains("verbouwing") || lowerText.Contains("construction") ||
                lowerText.Contains("structuur") || lowerText.Contains("structure"))
            {
                return true;
            }
            
            return false;
        }
        
        private bool ImprovesHomeQuality(Models.LineItemAnalysisDetail item)
        {
            // If we have specific notes that mention value improvement
            if (!string.IsNullOrEmpty(item.Notes))
            {
                if (item.Notes.Contains("value", StringComparison.OrdinalIgnoreCase) &&
                    (item.Notes.Contains("increase", StringComparison.OrdinalIgnoreCase) ||
                     item.Notes.Contains("improve", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            
            // Home improvement items are generally assumed to improve quality
            if (item.IsHomeImprovement)
            {
                return true;
            }
            
            // Check if the category is related to improvement
            if (!string.IsNullOrEmpty(item.Category))
            {
                if (item.Category.Contains("improvement", StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains("renovation", StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains("upgrade", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            // Check the raw description
            return ImprovesHomeQualityText(item.Description);
        }
        
        private bool ImprovesHomeQualityText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            
            // Keywords that suggest quality improvement
            var qualityKeywords = new[] {
                "verbetering", "improvement", "upgrade", "renovatie", "renovation",
                "modernisering", "modernization", "verduurzaming", "sustainability",
                "energie", "energy", "isolatie", "insulation", "kwaliteit", "quality",
                "waarde", "value"
            };
            
            foreach (var keyword in qualityKeywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            // Most structural or major installation items improve quality
            string lowerText = text.ToLowerInvariant();
            if (lowerText.Contains("installatie") || lowerText.Contains("installation") ||
                lowerText.Contains("renovatie") || lowerText.Contains("renovation") ||
                lowerText.Contains("verbouwing") || lowerText.Contains("construction"))
            {
                return true;
            }
            
            return false;
        }
        
        private VerduurzamingsdepotCategory? GetSustainabilityCategory(string description, Models.LineItemAnalysisDetail? analysis = null)
        {
            // If we have analysis category information
            if (analysis != null && !string.IsNullOrEmpty(analysis.Category))
            {
                foreach (var category in CategoryKeywords)
                {
                    if (analysis.Category.Contains(category.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return category.Key;
                    }
                }
            }
            
            // Check if "sustainability", "energy saving", or similar terms are in the description
            string textToCheck = description;
            if (analysis != null && !string.IsNullOrEmpty(analysis.InterpretedAs))
            {
                textToCheck = analysis.InterpretedAs;
            }
            
            if (string.IsNullOrEmpty(textToCheck))
            {
                return null;
            }
            
            // Check if it contains general verduurzamingsdepot keywords
            bool hasSustainabilityKeyword = false;
            foreach (var keyword in VerduurzamingsdepotKeywords)
            {
                if (textToCheck.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    hasSustainabilityKeyword = true;
                    break;
                }
            }
            
            if (!hasSustainabilityKeyword)
            {
                return null;
            }
            
            // Find the most specific category match
            VerduurzamingsdepotCategory? bestMatch = null;
            int bestMatchScore = 0;
            
            foreach (var category in CategoryKeywords)
            {
                int matchScore = 0;
                foreach (var keyword in category.Value)
                {
                    if (textToCheck.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        matchScore++;
                    }
                }
                
                if (matchScore > bestMatchScore)
                {
                    bestMatch = category.Key;
                    bestMatchScore = matchScore;
                }
            }
            
            // If we couldn't find a specific category, default to Combination
            if (bestMatch == null && hasSustainabilityKeyword)
            {
                return VerduurzamingsdepotCategory.Combination;
            }
            
            return bestMatch;
        }
        
        private bool DoesRequireHRGlass(string description, Models.LineItemAnalysisDetail? analysis = null)
        {
            // Check if it's energy-efficient doors/windows without HR++ glass
            string textToCheck = description;
            if (analysis != null && !string.IsNullOrEmpty(analysis.InterpretedAs))
            {
                textToCheck = analysis.InterpretedAs;
            }
            
            // If it already has "HR++" or similar, it doesn't require additional HR glass
            if (textToCheck.Contains("HR++") || textToCheck.Contains("hoog rendement"))
            {
                return false;
            }
            
            // Check if it contains door/window keywords
            var doorWindowKeywords = new[] {
                "deur", "door", "kozijn", "frame", "raam", "window"
            };
            
            foreach (var keyword in doorWindowKeywords)
            {
                if (textToCheck.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    // Check for energy efficiency terms
                    if (textToCheck.Contains("energie", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("energy", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("efficien", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("zuinig", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("bespar", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("saving", StringComparison.OrdinalIgnoreCase) ||
                        textToCheck.Contains("isolat", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        #endregion
    }
}
