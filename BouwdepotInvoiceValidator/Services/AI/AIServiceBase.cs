using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services.AI
{
    /// <summary>
    /// Configuration options for AI services
    /// </summary>
    public class AIServiceOptions
    {
        /// <summary>
        /// The temperature setting for the AI model (0.0 to 1.0)
        /// Lower values make outputs more deterministic, higher values more creative
        /// </summary>
        public double Temperature { get; set; } = 0.2;
        
        /// <summary>
        /// Whether to use multi-modal analysis (text + image) when available
        /// </summary>
        public bool UseMultiModalAnalysis { get; set; } = true;
        
        /// <summary>
        /// Maximum tokens to generate in the output
        /// </summary>
        public int MaxOutputTokens { get; set; } = 4096;
        
        /// <summary>
        /// Default confidence threshold for auto-approval (0-100)
        /// </summary>
        public int DefaultConfidenceThreshold { get; set; } = 75;
        
        /// <summary>
        /// Threshold for high risk identification (0-100)
        /// </summary>
        public int HighRiskThreshold { get; set; } = 40;
        
        /// <summary>
        /// Whether to enable auto-approval based on confidence
        /// </summary>
        public bool EnableAutoApproval { get; set; } = true;
    }
    
    /// <summary>
    /// Base class for AI services that provides common functionality
    /// </summary>
    public abstract class AIServiceBase : IAIService
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly AIServiceOptions _options;
        
        private readonly string _promptsBasePath;
        
        protected AIServiceBase(
            ILogger logger, 
            IConfiguration configuration, 
            AIServiceOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _options = options ?? new AIServiceOptions();
            
            // Configure the base path for prompt templates
            _promptsBasePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, 
                "Prompts");
                
            _logger.LogInformation("AIServiceBase initialized with Temperature: {Temperature}, " +
                "UseMultiModalAnalysis: {UseMultiModalAnalysis}", 
                _options.Temperature, _options.UseMultiModalAnalysis);
        }
        
        /// <summary>
        /// Validates an invoice using AI
        /// </summary>
        public abstract Task<ValidationResult> ValidateInvoiceAsync(
            Invoice invoice, 
            string locale = "nl-NL", 
            int? confidenceThreshold = null);
        
        /// <summary>
        /// Gets information about the AI model
        /// </summary>
        public abstract AIModelInfo GetModelInfo();
        
        /// <summary>
        /// Builds a unified validation prompt for invoice validation
        /// </summary>
        protected string BuildUnifiedValidationPrompt(Invoice invoice, string locale)
        {
            _logger.LogInformation("Building unified validation prompt for invoice: {InvoiceNumber}", 
                invoice.InvoiceNumber);
                
            var promptBuilder = new StringBuilder();
            
            // Add system instructions
            promptBuilder.AppendLine("# Invoice Validation System");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("You are an expert invoice validator specializing in home improvement invoices for mortgage providers.");
            promptBuilder.AppendLine("Your task is to analyze the provided invoice thoroughly and determine:");
            promptBuilder.AppendLine("1. If it is a valid invoice document");
            promptBuilder.AppendLine("2. If it contains home improvement expenses eligible for mortgage financing");
            promptBuilder.AppendLine("3. The confidence level of this determination");
            promptBuilder.AppendLine();
            
            // Add role and objective
            promptBuilder.AppendLine("## Your Role");
            promptBuilder.AppendLine("You work for a mortgage provider that offers construction deposits (bouwdepot) for home improvements.");
            promptBuilder.AppendLine("Your analysis will determine if expenses are eligible for reimbursement from these construction deposits.");
            promptBuilder.AppendLine();
            
            // Add detailed instructions about what constitutes home improvement
            promptBuilder.AppendLine("## Home Improvement Definition");
            promptBuilder.AppendLine("Home improvements include:");
            promptBuilder.AppendLine("- Structural modifications (walls, floors, ceilings)");
            promptBuilder.AppendLine("- Kitchen renovations and installations");
            promptBuilder.AppendLine("- Bathroom renovations and installations");
            promptBuilder.AppendLine("- Electrical and plumbing work");
            promptBuilder.AppendLine("- Heating, ventilation, and air conditioning");
            promptBuilder.AppendLine("- Extensions or additions to the home");
            promptBuilder.AppendLine("- Roof repairs or replacements");
            promptBuilder.AppendLine("- Window replacements");
            promptBuilder.AppendLine("- Insulation improvements");
            promptBuilder.AppendLine("- Solar panel installations");
            promptBuilder.AppendLine("- Permanent fixtures and fittings");
            promptBuilder.AppendLine();
            
            promptBuilder.AppendLine("NOT considered home improvements:");
            promptBuilder.AppendLine("- Loose furniture, appliances not built-in");
            promptBuilder.AppendLine("- Decorative items (curtains, blinds, lamps)");
            promptBuilder.AppendLine("- Routine cleaning services");
            promptBuilder.AppendLine("- Garden maintenance (vs. garden renovation)");
            promptBuilder.AppendLine("- Mobile or temporary structures");
            promptBuilder.AppendLine();
            
            // Add invoice details if available
            if (invoice != null)
            {
                promptBuilder.AppendLine("## Invoice Information");
                promptBuilder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
                promptBuilder.AppendLine($"Invoice Date: {invoice.InvoiceDate?.ToString("yyyy-MM-dd") ?? "Not provided"}");
                promptBuilder.AppendLine($"Vendor: {invoice.VendorName}");
                promptBuilder.AppendLine($"Total Amount: {invoice.TotalAmount} {invoice.Currency}");
                
                // Add line items if available
                if (invoice.LineItems.Count > 0)
                {
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("## Line Items");
                    
                    foreach (var item in invoice.LineItems)
                    {
                        promptBuilder.AppendLine($"- {item.Description}: {item.Quantity} x {item.UnitPrice} = {item.TotalPrice} {invoice.Currency}");
                    }
                }
                
                promptBuilder.AppendLine();
            }
            
            // Add expected response format
            promptBuilder.AppendLine("## Expected Response Format");
            promptBuilder.AppendLine("Provide your analysis in JSON format with the following structure:");
            promptBuilder.AppendLine("```json");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"isValidInvoice\": true/false,");
            promptBuilder.AppendLine("  \"isHomeImprovement\": true/false,");
            promptBuilder.AppendLine("  \"confidenceScore\": 0-100,");
            promptBuilder.AppendLine("  \"contentSummary\": {");
            promptBuilder.AppendLine("    \"purchasedItems\": \"description of items purchased\",");
            promptBuilder.AppendLine("    \"intendedPurpose\": \"analysis of likely purpose/project\",");
            promptBuilder.AppendLine("    \"propertyImpact\": \"how these items impact the property value\",");
            promptBuilder.AppendLine("    \"projectCategory\": \"category of home improvement\",");
            promptBuilder.AppendLine("    \"estimatedProjectScope\": \"small repair, major renovation, etc.\"");
            promptBuilder.AppendLine("  },");
            promptBuilder.AppendLine("  \"confidenceFactors\": [");
            promptBuilder.AppendLine("    {");
            promptBuilder.AppendLine("      \"factorName\": \"name of factor\",");
            promptBuilder.AppendLine("      \"impact\": -20 to +20,");
            promptBuilder.AppendLine("      \"explanation\": \"explanation of this factor\"");
            promptBuilder.AppendLine("    }");
            promptBuilder.AppendLine("  ],");
            promptBuilder.AppendLine("  \"fraudDetection\": {");
            promptBuilder.AppendLine("    \"fraudRiskScore\": 0-100,");
            promptBuilder.AppendLine("    \"recommendedAction\": \"e.g., 'Approve', 'Request additional verification'\",");
            promptBuilder.AppendLine("    \"fraudIndicators\": [");
            promptBuilder.AppendLine("      {");
            promptBuilder.AppendLine("        \"indicatorName\": \"name of indicator\",");
            promptBuilder.AppendLine("        \"description\": \"description\",");
            promptBuilder.AppendLine("        \"evidence\": \"specific evidence\",");
            promptBuilder.AppendLine("        \"severity\": 0.0-1.0");
            promptBuilder.AppendLine("      }");
            promptBuilder.AppendLine("    ]");
            promptBuilder.AppendLine("  },");
            promptBuilder.AppendLine("  \"auditReport\": {");
            promptBuilder.AppendLine("    \"executiveSummary\": \"concise summary of decision\",");
            promptBuilder.AppendLine("    \"ruleAssessments\": [");
            promptBuilder.AppendLine("      {");
            promptBuilder.AppendLine("        \"ruleName\": \"name of rule\",");
            promptBuilder.AppendLine("        \"description\": \"description of rule\",");
            promptBuilder.AppendLine("        \"isSatisfied\": true/false,");
            promptBuilder.AppendLine("        \"evidence\": \"evidence\",");
            promptBuilder.AppendLine("        \"reasoning\": \"reasoning\",");
            promptBuilder.AppendLine("        \"score\": 0-100");
            promptBuilder.AppendLine("      }");
            promptBuilder.AppendLine("    ],");
            promptBuilder.AppendLine("    \"approvalFactors\": [\"factor 1\", \"factor 2\"],");
            promptBuilder.AppendLine("    \"concernFactors\": [\"concern 1\", \"concern 2\"]");
            promptBuilder.AppendLine("  }");
            promptBuilder.AppendLine("}");
            promptBuilder.AppendLine("```");
            promptBuilder.AppendLine();
            
            // Add locale-specific instructions
            if (locale.Equals("nl-NL", StringComparison.OrdinalIgnoreCase))
            {
                promptBuilder.AppendLine("## Dutch-Specific Considerations");
                promptBuilder.AppendLine("- In the Netherlands, 'BTW' refers to VAT (Value Added Tax)");
                promptBuilder.AppendLine("- Common home improvement vendors include: Gamma, Karwei, Praxis, Hornbach, Bauhaus");
                promptBuilder.AppendLine("- Pay special attention to 'arbeidskosten' (labor costs) which are usually eligible");
                promptBuilder.AppendLine("- 'Meubilair' (furniture) is typically not eligible unless it's built-in");
                promptBuilder.AppendLine();
            }
            
            // Add concluding instructions
            promptBuilder.AppendLine("## Final Instructions");
            promptBuilder.AppendLine("1. Be thorough in your analysis");
            promptBuilder.AppendLine("2. Return valid JSON - this is critical"); 
            promptBuilder.AppendLine("3. Be certain about your confidence score - 100 means absolute certainty, 0 means complete uncertainty");
            promptBuilder.AppendLine("4. Provide detailed reasoning where possible");
            
            return promptBuilder.ToString();
        }
        
        /// <summary>
        /// Loads a prompt template from the file system
        /// </summary>
        protected string LoadPromptTemplate(string templateName)
        {
            try
            {
                string templatePath = Path.Combine(_promptsBasePath, $"{templateName}.txt");
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Prompt template not found: {TemplatePath}", templatePath);
                    return string.Empty;
                }
                
                string template = File.ReadAllText(templatePath);
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prompt template {TemplateName}", templateName);
                return string.Empty;
            }
        }
    }
}
