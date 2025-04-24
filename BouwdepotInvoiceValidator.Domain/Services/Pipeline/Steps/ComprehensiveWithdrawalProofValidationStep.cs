using BouwdepotInvoiceValidator.Domain.Services.Schema;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Pipeline step for determining construction fund withdrawal eligibility
    /// </summary>
    public class ComprehensiveWithdrawalProofValidationStep : IValidationPipelineStep
    {
        private readonly ILogger<ComprehensiveWithdrawalProofValidationStep> _logger;
        private readonly DynamicPromptService _dynamicPromptService;

        /// <summary>
        /// Gets the name of the step
        /// </summary>
        public string StepName => "ComprehensiveWithdrawalProofValidation";

        /// <summary>
        /// Gets the order of the step in the pipeline
        /// </summary>
        public int Order => 100;

        /// <summary>
        /// Gets the prompt template name for this step
        /// </summary>
        public string PromptTemplatePath => "ComprehensiveWithdrawalProofValidation";

        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        public Type ResponseType => typeof(ComprehensiveWithdrawalProofResponse);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComprehensiveWithdrawalProofValidationStep"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        /// <param name="dynamicPromptService">The dynamic prompt service</param>
        public ComprehensiveWithdrawalProofValidationStep(
            ILogger<ComprehensiveWithdrawalProofValidationStep> logger,
            DynamicPromptService dynamicPromptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dynamicPromptService = dynamicPromptService ?? throw new ArgumentNullException(nameof(dynamicPromptService));
        }

        /// <inheritdoc/>
        public bool ShouldExecute(ValidationContext context)
        {
            return context.OverallOutcome != ValidationOutcome.Invalid &&
                   context.OverallOutcome != ValidationOutcome.Error;
        }

        /// <inheritdoc/>
        public async Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Preparing construction fund withdrawal eligibility prompt: {ContextId}", context.Id);
            context.AddProcessingStep(StepName, "Determining eligibility for construction fund withdrawal", ProcessingStepStatus.InProgress);

            // Use dynamic prompt service to generate schema from POCO
            var prompt = _dynamicPromptService.BuildPromptWithDynamicSchema<ComprehensiveWithdrawalProofResponse>(PromptTemplatePath);

            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }

        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var comprehensiveResponse = (ComprehensiveWithdrawalProofResponse)response;
            
            // Store the comprehensive validation result directly in the context
            context.ComprehensiveValidationResult = comprehensiveResponse;
            
                        

            // Determine overall outcome based on eligibility status
            string overallStatus = comprehensiveResponse.EligibilityDetermination.OverallStatus;
            switch (overallStatus)
            {
                case "Eligible":
                    context.OverallOutcome = ValidationOutcome.Valid;
                    break;
                case "Partially Eligible":
                    context.OverallOutcome = ValidationOutcome.NeedsReview; // Use NeedsReview instead of PartiallyValid
                    break;
                default:
                    context.OverallOutcome = ValidationOutcome.Invalid;
                    break;
            }

            

            // Set context summary based on eligibility
            if (comprehensiveResponse.EligibilityDetermination.OverallStatus == "Eligible" || 
                comprehensiveResponse.EligibilityDetermination.OverallStatus == "Partially Eligible")
            {
                context.AddProcessingStep(StepName,
                    $"Document validation completed. Status: {comprehensiveResponse.EligibilityDetermination.OverallStatus}. " +
                    $"Eligible amount: {comprehensiveResponse.EligibilityDetermination.TotalEligibleAmountDetermined:C}",
                    ProcessingStepStatus.Success);

                if (context.OverallOutcome != ValidationOutcome.NeedsReview)
                {
                    var summaryBuilder = new StringBuilder();
                    summaryBuilder.AppendLine(comprehensiveResponse.EligibilityDetermination.RationaleSummary);
                    summaryBuilder.AppendLine();
                    summaryBuilder.AppendLine($"Identified {context.ComprehensiveValidationResult.ConstructionActivities.DetailedActivityAnalysis.Where(c => c.IsEligible).Count()} eligible construction activities.");
                    summaryBuilder.AppendLine($"Eligible amount: {comprehensiveResponse.EligibilityDetermination.TotalEligibleAmountDetermined:C}");
                    summaryBuilder.AppendLine($"Fraud risk level: {comprehensiveResponse.FraudAnalysis.FraudRiskLevel}");
                    
                    // Add required actions
                    if (comprehensiveResponse.EligibilityDetermination.RequiredActions != null &&
                        comprehensiveResponse.EligibilityDetermination.RequiredActions.Count > 0)
                    {
                        summaryBuilder.AppendLine();
                        summaryBuilder.AppendLine("Required actions:");
                        foreach (var action in comprehensiveResponse.EligibilityDetermination.RequiredActions)
                        {
                            summaryBuilder.AppendLine($"- {action}");
                            context.AddIssue("RequiredAction", action, IssueSeverity.Info);
                        }
                    }

                    summaryBuilder.AppendLine();

                    context.OverallOutcomeSummary = summaryBuilder.ToString().TrimEnd();
                }
            }
            else
            {
                context.AddProcessingStep(StepName,
                    $"Document is not eligible for construction fund withdrawal. Reason: {comprehensiveResponse.EligibilityDetermination.RationaleCategory}",
                    ProcessingStepStatus.Warning);

                context.AddIssue("NotEligible",
                    comprehensiveResponse.EligibilityDetermination.RationaleSummary,
                    IssueSeverity.Warning);

                context.OverallOutcome = ValidationOutcome.Invalid;
                context.OverallOutcomeSummary = comprehensiveResponse.EligibilityDetermination.RationaleSummary;
            }

            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
