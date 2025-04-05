using System.Text;
using System.Text.Json.Serialization;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Services.Prompts;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for analyzing invoice line items using Gemini API
    /// </summary>
    public class GeminiLineItemAnalysisService : GeminiServiceBase
    {
        private readonly ILogger<GeminiLineItemAnalysisService> _lineItemLogger;
        private readonly PromptTemplateService _promptService;
        
        public GeminiLineItemAnalysisService(
            ILogger<GeminiLineItemAnalysisService> logger, 
            IConfiguration configuration,
            PromptTemplateService promptService) 
            : base(logger, configuration)
        {
            _lineItemLogger = logger;
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <summary>
        /// Analyzes invoice line items to determine what was purchased and categorize them
        /// </summary>
        public async Task<LineItemAnalysisResult> AnalyzeLineItemsAsync(Invoice invoice)
        {
            _lineItemLogger.LogInformation("Analyzing line items for invoice: {FileName}", invoice.FileName);
            
            var result = new LineItemAnalysisResult();
            
            try
            {
                // Check if we have line items to analyze
                if (invoice.LineItems == null || invoice.LineItems.Count == 0)
                {
                    _lineItemLogger.LogWarning("No line items available for analysis in invoice: {FileName}", invoice.FileName);
                    
                    result.Success = false;
                    result.ErrorMessage = "No line items available for analysis";
                    return result;
                }
                
                // Prepare the line item analysis prompt
                var prompt = BuildLineItemAnalysisPrompt(invoice);
                
                // Call Gemini API with specific operation name
                var response = await CallGeminiApiAsync(prompt, null, "LineItemAnalysis");
                result.RawResponse = response;
                
                // Parse the response
                ParseLineItemAnalysisResponse(response, result);
                
                _lineItemLogger.LogInformation("Line item analysis completed successfully: {FileName}, Categories: {Categories}", 
                    invoice.FileName, string.Join(", ", result.Categories));
                
                return result;
            }
            catch (Exception ex)
            {
                _lineItemLogger.LogError(ex, "Error analyzing line items with Gemini for {FileName}", invoice.FileName);
                
                result.Success = false;
                result.ErrorMessage = $"Error analyzing line items: {ex.Message}";
                return result;
            }
        }
        
        private string BuildLineItemAnalysisPrompt(Invoice invoice)
        {
            _lineItemLogger.LogDebug("Building line item analysis prompt for {FileName}", invoice.FileName);
            
            try
            {
                // Build context for the prompt
                var contextBuilder = new StringBuilder();
                
                if (!string.IsNullOrEmpty(invoice.VendorName))
                {
                    contextBuilder.AppendLine($"Vendor: {invoice.VendorName}");
                }
                
                if (invoice.InvoiceDate.HasValue)
                {
                    contextBuilder.AppendLine($"Invoice Date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
                }
                
                if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
                {
                    contextBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
                }
                
                contextBuilder.AppendLine($"Total Amount: {invoice.TotalAmount:C}");
                
                contextBuilder.AppendLine("\nLine Items:");
                foreach (var item in invoice.LineItems)
                {
                    contextBuilder.AppendLine($"- {item.Description}");
                    contextBuilder.AppendLine($"  Quantity: {item.Quantity}, Unit Price: {item.UnitPrice:C}, Total: {item.TotalPrice:C}");
                }
                
                // Try to get the prompt from the template service
                var parameters = new Dictionary<string, string>
                {
                    { "context", contextBuilder.ToString() },
                    { "vendorName", invoice.VendorName ?? "Unknown" }
                };
                
                var prompt = _promptService.GetPrompt("LineItemAnalysis", parameters);
                
                // If we got a valid prompt, return it
                if (!string.IsNullOrEmpty(prompt))
                {
                    _lineItemLogger.LogDebug("Successfully built line item analysis prompt from template");
                    return prompt;
                }
            }
            catch (Exception ex)
            {
                _lineItemLogger.LogWarning(ex, "Error using prompt template for line item analysis. Falling back to default prompt.");
            }
            
            // Fallback to the old hardcoded prompt if template fails
            _lineItemLogger.LogWarning("Using fallback hardcoded prompt for line item analysis");
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine("### LINE ITEM ANALYSIS ###");
            promptBuilder.AppendLine("You are an expert in analyzing construction and home improvement purchases.");
            promptBuilder.AppendLine("Your task is to analyze the line items from an invoice to determine what was purchased and categorize each item.");
            
            promptBuilder.AppendLine("\n### INVOICE CONTEXT ###");
            if (!string.IsNullOrEmpty(invoice.VendorName))
            {
                promptBuilder.AppendLine($"Vendor: {invoice.VendorName}");
            }
            
            if (invoice.InvoiceDate.HasValue)
            {
                promptBuilder.AppendLine($"Invoice Date: {invoice.InvoiceDate.Value:yyyy-MM-dd}");
            }
            
            if (!string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                promptBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
            }
            
            promptBuilder.AppendLine($"Total Amount: {invoice.TotalAmount:C}");
            
            promptBuilder.AppendLine("\n### LINE ITEMS TO ANALYZE ###");
            foreach (var item in invoice.LineItems)
            {
                promptBuilder.AppendLine($"- {item.Description}");
                promptBuilder.AppendLine($"  Quantity: {item.Quantity}, Unit Price: {item.UnitPrice:C}, Total: {item.TotalPrice:C}");
            }
            
            promptBuilder.AppendLine("\n### ANALYSIS INSTRUCTIONS ###");
            promptBuilder.AppendLine("1. Analyze each line item to determine what product or service was purchased");
            promptBuilder.AppendLine("2. Categorize each line item into home improvement categories (e.g., Plumbing, Electrical, Flooring, etc.)");
            promptBuilder.AppendLine("3. Determine if the line items collectively represent home improvement purchases");
            promptBuilder.AppendLine("4. Provide a summary of what was purchased and the main purpose of these purchases");
            promptBuilder.AppendLine("5. Identify any line items that don't appear to be related to home improvement");
            
            promptBuilder.AppendLine("\n### HOME IMPROVEMENT CATEGORIES ###");
            promptBuilder.AppendLine("Use these categories (or suggest more specific ones if appropriate):");
            promptBuilder.AppendLine("- Plumbing: Pipes, faucets, toilets, showers, water heaters");
            promptBuilder.AppendLine("- Electrical: Wiring, outlets, switches, lighting fixtures, electrical panels");
            promptBuilder.AppendLine("- HVAC: Heating, ventilation, air conditioning components and systems");
            promptBuilder.AppendLine("- Flooring: Tile, wood, laminate, vinyl, carpet");
            promptBuilder.AppendLine("- Walls & Ceilings: Drywall, plaster, paint, wallpaper, ceiling materials");
            promptBuilder.AppendLine("- Windows & Doors: Windows, doors, frames, glass, handles, locks");
            promptBuilder.AppendLine("- Roofing: Roof tiles, shingles, flashing, gutters");
            promptBuilder.AppendLine("- Kitchen: Cabinets, countertops, appliances, sinks");
            promptBuilder.AppendLine("- Bathroom: Fixtures, vanities, tubs, shower enclosures");
            promptBuilder.AppendLine("- Structural: Beams, supports, foundations, framing");
            promptBuilder.AppendLine("- Outdoor: Decking, fencing, landscaping materials");
            promptBuilder.AppendLine("- Tools & Equipment: Construction tools, rental equipment");
            promptBuilder.AppendLine("- General Supplies: Fasteners, adhesives, general hardware");
            promptBuilder.AppendLine("- Professional Services: Labor, design, inspection");
            promptBuilder.AppendLine("- Non-Home Improvement: Items not related to home improvement");
            
            promptBuilder.AppendLine("\n### OUTPUT FORMAT ###");
            promptBuilder.AppendLine("Respond with ONLY a JSON object in the following format:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("  \"summary\": \"A general summary of what was purchased and the likely project\",");
            promptBuilder.AppendLine("  \"primaryPurpose\": \"The main purpose or project these items are for\",");
            promptBuilder.AppendLine("  \"categories\": [\"List of categories represented in the line items\"],");
            promptBuilder.AppendLine("  \"lineItemAnalysis\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"description\": \"Original line item description\",");
            promptBuilder.AppendLine("      \"interpretedAs\": \"What this item actually is\",");
            promptBuilder.AppendLine("      \"category\": \"Home improvement category\",");
            promptBuilder.AppendLine("      \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("      \"confidence\": 0.0-1.0,");
            promptBuilder.AppendLine("      \"notes\": \"Any additional notes about this item\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("    // Repeat for each line item");
            promptBuilder.AppendLine("  ]");
            promptBuilder.AppendLine("}");
            
            return promptBuilder.ToString();
        }
        
        private void ParseLineItemAnalysisResponse(string responseText, LineItemAnalysisResult result)
        {
            _lineItemLogger.LogDebug("Parsing line item analysis response");
            
            try
            {
                // Use the base class helper method for JSON extraction and deserialization
                var response = ExtractAndDeserializeJson<LineItemAnalysisResponse>(responseText);
                
                if (response != null)
                {
                    _lineItemLogger.LogInformation("Line item analysis: IsHomeImprovement={IsHomeImprovement}, Confidence={Confidence:P0}", 
                        response.IsHomeImprovement, response.Confidence);
                    
                    // Set basic properties
                    result.Success = true;
                    result.IsHomeImprovement = response.IsHomeImprovement;
                    result.Confidence = response.Confidence;
                    result.Summary = response.Summary;
                    result.PrimaryPurpose = response.PrimaryPurpose;
                    result.Categories = response.Categories;
                    
                    // Process line item analysis
                    if (response.LineItemAnalysis != null)
                    {
                        _lineItemLogger.LogInformation("Processing {Count} line item analyses", response.LineItemAnalysis.Count);
                        
                        foreach (var item in response.LineItemAnalysis)
                        {
                            var analysis = new LineItemAnalysisDetail
                            {
                                Description = item.Description,
                                InterpretedAs = item.InterpretedAs,
                                Category = item.Category,
                                IsHomeImprovement = item.IsHomeImprovement,
                                Confidence = item.Confidence,
                                Notes = item.Notes
                            };
                            
                            result.LineItemDetails.Add(analysis);
                            
                            _lineItemLogger.LogDebug("Line item: {Description} - Category: {Category}, IsHomeImprovement: {IsHomeImprovement}", 
                                item.Description, item.Category, item.IsHomeImprovement);
                        }
                    }
                    
                    // Calculate percentage of items that are home improvement related
                    if (result.LineItemDetails.Count > 0)
                    {
                        var homeImprovementCount = result.LineItemDetails.Count(x => x.IsHomeImprovement);
                        result.HomeImprovementPercentage = (double)homeImprovementCount / result.LineItemDetails.Count;
                        
                        _lineItemLogger.LogInformation("Home improvement percentage: {Percentage:P0}", result.HomeImprovementPercentage);
                    }
                }
                else
                {
                    _lineItemLogger.LogWarning("Failed to parse line item analysis response as JSON");
                    
                    result.Success = false;
                    result.ErrorMessage = "Failed to parse analysis response";
                }
            }
            catch (Exception ex)
            {
                _lineItemLogger.LogError(ex, "Error parsing line item analysis response: {ErrorMessage}", ex.Message);
                
                result.Success = false;
                result.ErrorMessage = $"Error parsing analysis: {ex.Message}";
            }
        }
        
        internal class LineItemAnalysisResponse
        {
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("summary")]
            public string Summary { get; set; } = string.Empty;
            
            [JsonPropertyName("primaryPurpose")]
            public string PrimaryPurpose { get; set; } = string.Empty;
            
            [JsonPropertyName("categories")]
            public List<string> Categories { get; set; } = new List<string>();
            
            [JsonPropertyName("lineItemAnalysis")]
            public List<LineItemDetail> LineItemAnalysis { get; set; } = new List<LineItemDetail>();
        }
        
        internal class LineItemDetail
        {
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
            
            [JsonPropertyName("interpretedAs")]
            public string InterpretedAs { get; set; } = string.Empty;
            
            [JsonPropertyName("category")]
            public string Category { get; set; } = string.Empty;
            
            [JsonPropertyName("isHomeImprovement")]
            public bool IsHomeImprovement { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
            
            [JsonPropertyName("notes")]
            public string Notes { get; set; } = string.Empty;
        }
    }
    
    /// <summary>
    /// Result of line item analysis
    /// </summary>
    public class LineItemAnalysisResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsHomeImprovement { get; set; }
        public double Confidence { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string PrimaryPurpose { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new List<string>();
        public List<LineItemAnalysisDetail> LineItemDetails { get; set; } = new List<LineItemAnalysisDetail>();
        public double HomeImprovementPercentage { get; set; }
        public string RawResponse { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Detailed analysis of a single line item
    /// </summary>
    public class LineItemAnalysisDetail
    {
        public string Description { get; set; } = string.Empty;
        public string InterpretedAs { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsHomeImprovement { get; set; }
        public double Confidence { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
