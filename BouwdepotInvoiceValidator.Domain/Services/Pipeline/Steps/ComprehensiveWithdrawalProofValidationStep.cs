using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotInvoiceValidator.Domain.Services.Services;
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
        private readonly PromptService _promptService;

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
        public Type ResponseType => typeof(HomeImprovementResponse);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComprehensiveWithdrawalProofValidation"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="promptService">The prompt service</param>
        public ComprehensiveWithdrawalProofValidationStep(
            ILogger<ComprehensiveWithdrawalProofValidationStep> logger,
            PromptService promptService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
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

            var prompt = _promptService.GetPrompt(PromptTemplatePath).Render();

            return (prompt, new List<Stream> { documentStream }, new List<string> { context.InputDocument.ContentType });
        }

        /// <inheritdoc/>
        public async Task ProcessResponseAsync(ValidationContext context, object response)
        {
            var eligibilityResponse = (HomeImprovementResponse)response;

            // Update line items with eligibility information
            if (context.ExtractedInvoice.LineItems != null && eligibilityResponse.lineItemAssessment != null)
            {
                for (int i = 0; i < context.ExtractedInvoice.LineItems.Count && i < eligibilityResponse.lineItemAssessment.Count; i++)
                {
                    var lineItem = context.ExtractedInvoice.LineItems[i];
                    var assessment = eligibilityResponse.lineItemAssessment[i];

                    lineItem.IsBouwdepotEligible = assessment.isEligible;
                    lineItem.EligibilityReason = assessment.reason;

                    // Map to construction activities if eligible
                    if (assessment.isEligible && !string.IsNullOrEmpty(assessment.category))
                    {
                        var activityCategory = WithdrawalEligibilityHelper.MapCategoryStringToEnum(assessment.category);

                        context.IdentifiedActivities.Add(new ConstructionActivity
                        {
                            ActivityName = lineItem.Description,
                            Description = assessment.reason,
                            Category = activityCategory,
                            IsEligible = true,
                            IdentificationKeywords = new List<string> { lineItem.Description }
                        });
                    }
                }
            }

            // Determine overall eligibility
            bool isEligible = eligibilityResponse.isHomeImprovement && eligibilityResponse.totalEligibleAmount > 0;

            // Generate a summary of identified construction activities
            var activitySummary = WithdrawalEligibilityHelper.GenerateActivitySummary(context.IdentifiedActivities);

            if (isEligible)
            {
                context.AddProcessingStep(StepName,
                    $"Document is eligible for construction fund withdrawal. Eligible amount: {eligibilityResponse.totalEligibleAmount:C}",
                    ProcessingStepStatus.Success);

                if (context.OverallOutcome != ValidationOutcome.NeedsReview)
                {
                    context.OverallOutcome = ValidationOutcome.Valid;

                    var summaryBuilder = new StringBuilder();
                    summaryBuilder.AppendLine($"Document is valid and eligible for construction fund withdrawal.");
                    summaryBuilder.AppendLine($"Identified {context.IdentifiedActivities.Count} eligible construction activities.");
                    summaryBuilder.AppendLine($"Eligible amount: {eligibilityResponse.totalEligibleAmount:C}");

                    if (eligibilityResponse.eligibleCategories?.Any() == true)
                    {
                        summaryBuilder.AppendLine($"Eligible categories: {string.Join(", ", eligibilityResponse.eligibleCategories)}");
                    }

                    summaryBuilder.AppendLine(activitySummary);

                    context.OverallOutcomeSummary = summaryBuilder.ToString().TrimEnd();
                }
            }
            else
            {
                context.AddProcessingStep(StepName,
                    "Document is not eligible for construction fund withdrawal",
                    ProcessingStepStatus.Warning);

                context.AddIssue("NotEligible",
                    "Document is not eligible for construction fund financing",
                    IssueSeverity.Warning);

                context.OverallOutcome = ValidationOutcome.Invalid;
                context.OverallOutcomeSummary = "Document is not eligible for construction fund financing.";
            }

            context.AddAIModelUsage("Gemini", "1.0", "WithdrawalEligibility", 500);

            await Task.CompletedTask; // To satisfy the async signature
        }
    }
}
