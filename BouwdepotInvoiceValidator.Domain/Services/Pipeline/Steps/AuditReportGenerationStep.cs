using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for generating a comprehensive audit report for construction fund withdrawal validation
    /// </summary>
    public class AuditReportGenerationStep : IValidationPipelineStep
    {
        private readonly ILogger<AuditReportGenerationStep> _logger;
        private readonly PromptService _promptService;
        
        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "GenerateAuditReport";
        
        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 900; // Last step
        
        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "ComprehensiveDocumentIntelligence";
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(string);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditReportGenerationStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public AuditReportGenerationStep(
            ILogger<AuditReportGenerationStep> logger,
            PromptService promptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        }
        
        /// <inheritdoc/>
        public bool ShouldExecute(ValidationContext context)
        {
            // Always execute this step as the final step
            return true;
        }
        
        /// <inheritdoc/>
        public async Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Preparing audit report generation prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Generating audit report", ProcessingStepStatus.InProgress);
            
            // Get the audit report generation prompt
            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render(
                new KeyValuePair<string, string>("context", GetContextSummary(context))
            );
            
            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }
        
        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var auditReport = (string)response;
            
            // Store the audit report in the context
            context.OverallOutcomeSummary = auditReport;
            
            context.AddProcessingStep(StepName,
                "Audit report generated",
                ProcessingStepStatus.Success);
            
            context.AddAIModelUsage("Gemini", "1.0", "AuditReportGeneration", 1000);
            
            await Task.CompletedTask; // To satisfy the async signature
        }
        
        /// <summary>
        /// Gets a summary of the validation context for the audit report
        /// </summary>
        private string GetContextSummary(ValidationContext context)
        {
            var summaryBuilder = new StringBuilder();
            
            summaryBuilder.AppendLine("Generate a comprehensive audit report for the document validation process.");
            summaryBuilder.AppendLine();
            summaryBuilder.AppendLine("Document Information:");
            summaryBuilder.AppendLine($"- File Name: {context.InputDocument.FileName}");
            summaryBuilder.AppendLine($"- Document Type: {context.DocumentType}");
            summaryBuilder.AppendLine($"- Overall Outcome: {context.OverallOutcome}");
            summaryBuilder.AppendLine($"- Summary: {context.OverallOutcomeSummary}");
            summaryBuilder.AppendLine();
            
            if (context.ExtractedInvoice != null)
            {
                summaryBuilder.AppendLine("Invoice Information:");
                summaryBuilder.AppendLine($"- Invoice Number: {context.ExtractedInvoice.InvoiceNumber}");
                summaryBuilder.AppendLine($"- Invoice Date: {context.ExtractedInvoice.InvoiceDate}");
                summaryBuilder.AppendLine($"- Due Date: {context.ExtractedInvoice.DueDate}");
                summaryBuilder.AppendLine($"- Total Amount: {context.ExtractedInvoice.TotalAmount} {context.ExtractedInvoice.Currency}");
                summaryBuilder.AppendLine($"- VAT Amount: {context.ExtractedInvoice.VatAmount} {context.ExtractedInvoice.Currency}");
                summaryBuilder.AppendLine($"- Vendor: {context.ExtractedInvoice.VendorName}");
                summaryBuilder.AppendLine();
                
                if (context.ExtractedInvoice.LineItems != null && context.ExtractedInvoice.LineItems.Any())
                {
                    summaryBuilder.AppendLine("Line Items:");
                    foreach (var lineItem in context.ExtractedInvoice.LineItems)
                    {
                        summaryBuilder.AppendLine($"- {lineItem.Description}: {lineItem.Quantity} x {lineItem.UnitPrice} = {lineItem.TotalPrice} {context.ExtractedInvoice.Currency}");
                        summaryBuilder.AppendLine($"  Bouwdepot Eligible: {(lineItem.IsBouwdepotEligible ? "Yes" : "No")} - {lineItem.EligibilityReason}");
                    }
                    summaryBuilder.AppendLine();
                }
            }
            
            if (context.IdentifiedActivities != null && context.IdentifiedActivities.Any())
            {
                summaryBuilder.AppendLine("Construction Activities:");
                var activityCategories = context.IdentifiedActivities
                    .GroupBy(a => a.Category)
                    .OrderByDescending(g => g.Count());
                    
                foreach (var category in activityCategories)
                {
                    summaryBuilder.AppendLine($"- {category.Key}: {category.Count()} items");
                    foreach (var activity in category.Take(5))
                    {
                        summaryBuilder.AppendLine($"  * {activity.ActivityName} - {activity.Description}");
                    }
                    if (category.Count() > 5)
                    {
                        summaryBuilder.AppendLine("  * (and more...)");
                    }
                }
                summaryBuilder.AppendLine();
            }
            
            if (context.FraudAnalysis != null)
            {
                summaryBuilder.AppendLine("Fraud Analysis:");
                summaryBuilder.AppendLine($"- Risk Level: {context.FraudAnalysis.RiskLevel}");
                summaryBuilder.AppendLine($"- Risk Score: {context.FraudAnalysis.RiskScore}");
                summaryBuilder.AppendLine($"- Summary: {context.FraudAnalysis.Summary}");
                
                if (context.FraudAnalysis.Indicators != null && context.FraudAnalysis.Indicators.Any())
                {
                    summaryBuilder.AppendLine("Fraud Indicators:");
                    foreach (var indicator in context.FraudAnalysis.Indicators)
                    {
                        summaryBuilder.AppendLine($"- {indicator.Category}: {indicator.Description} (Confidence: {indicator.Confidence:P0})");
                    }
                }
                summaryBuilder.AppendLine();
            }
            
            if (context.Issues != null && context.Issues.Any())
            {
                summaryBuilder.AppendLine("Issues:");
                foreach (var issue in context.Issues)
                {
                    summaryBuilder.AppendLine($"- {issue.IssueType} ({issue.Severity}): {issue.Description}");
                }
                summaryBuilder.AppendLine();
            }
            
            if (context.ProcessingSteps != null && context.ProcessingSteps.Any())
            {
                summaryBuilder.AppendLine("Processing Steps:");
                foreach (var step in context.ProcessingSteps)
                {
                    summaryBuilder.AppendLine($"- {step.StepName} ({step.Status}): {step.Description}");
                }
                summaryBuilder.AppendLine();
            }
            
            summaryBuilder.AppendLine("Please generate a comprehensive audit report based on this information.");
            
            return summaryBuilder.ToString();
        }
    }
}
