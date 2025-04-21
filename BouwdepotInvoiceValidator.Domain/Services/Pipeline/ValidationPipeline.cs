using BouwdepotInvoiceValidator.Domain.Models;
using BouwdepotValidationValidator.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline
{
    /// <summary>
    /// Implementation of the validation pipeline
    /// </summary>
    public class ValidationPipeline : IValidationPipeline
    {
        private readonly ILogger<ValidationPipeline> _logger;
        private readonly List<IValidationPipelineStep> _steps;
        private readonly ILLMProvider _llmProvider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationPipeline"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="steps">The pipeline steps</param>
        /// <param name="llmProvider">The LLM provider</param>
        public ValidationPipeline(
            ILogger<ValidationPipeline> logger,
            IEnumerable<IValidationPipelineStep> steps,
            ILLMProvider llmProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _steps = steps?.OrderBy(s => s.Order).ToList() ?? throw new ArgumentNullException(nameof(steps));
            _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        }
        
        /// <inheritdoc/>
        public async Task<ValidationContext> ExecuteAsync(ValidationContext context, Stream documentStream)
        {
            _logger.LogInformation("Starting validation pipeline with {StepCount} steps", _steps.Count);
            
            // Add initial processing step
            context.AddProcessingStep("InitializePipeline", 
                "Starting document validation pipeline", 
                ProcessingStepStatus.Success);
                
            // Initialize conversation context for maintaining history between LLM calls
            context.ConversationContext = _llmProvider.CreateConversationContext();
            
            foreach (var step in _steps)
            {
                _logger.LogDebug("Evaluating step: {StepName}", step.StepName);
                
                if (step.ShouldExecute(context))
                {
                    _logger.LogInformation("Executing pipeline step: {StepName}", step.StepName);
                    
                    try
                    {
                        // Prepare the prompt
                        var (prompt, imageStreams, mimeTypes) = await step.PreparePromptAsync(context, documentStream);
                        
                        // Execute the prompt
                        object response;
                        if (imageStreams != null && imageStreams.Any())
                        {
                            _logger.LogDebug("Sending multimodal prompt for step: {StepName}", step.StepName);
                            response = await _llmProvider.SendMultimodalStructuredPromptAsync(
                                step.ResponseType,
                                prompt,
                                imageStreams,
                                mimeTypes,
                                context.ConversationContext,
                                step.StepName);
                        }
                        else
                        {
                            _logger.LogDebug("Sending text prompt for step: {StepName}", step.StepName);
                            response = await _llmProvider.SendStructuredPromptAsync(
                                step.ResponseType,
                                prompt,
                                context.ConversationContext,
                                step.StepName);
                        }
                        
                        // Process the response
                        await step.ProcessResponseAsync(context, response);
                        
                        // Reset stream position for the next step
                        documentStream.Position = 0;
                        
                        _logger.LogDebug("Completed step: {StepName}", step.StepName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing pipeline step: {StepName}", step.StepName);
                        
                        // Record the error in the context
                        context.AddProcessingStep(step.StepName, 
                            $"Error executing step: {ex.Message}", 
                            ProcessingStepStatus.Error);
                        
                        context.AddIssue($"{step.StepName}Error", 
                            ex.Message, 
                            IssueSeverity.Error);
                        
                        context.OverallOutcome = ValidationOutcome.Error;
                        
                        // Break the pipeline on error
                        break;
                    }
                    
                    // Check for early termination conditions
                    if (context.OverallOutcome == ValidationOutcome.Invalid || 
                        context.OverallOutcome == ValidationOutcome.Error)
                    {
                        _logger.LogInformation("Pipeline terminated early after step {StepName} with outcome: {Outcome}", 
                            step.StepName, context.OverallOutcome);
                        break;
                    }
                }
                else
                {
                    _logger.LogInformation("Skipping pipeline step: {StepName}", step.StepName);
                    context.AddProcessingStep(step.StepName, "Step skipped", ProcessingStepStatus.Skipped);
                }
            }
            
            // Add final processing step
            context.AddProcessingStep("CompletePipeline", 
                $"Document validation pipeline completed with outcome: {context.OverallOutcome}", 
                ProcessingStepStatus.Success);
            
            _logger.LogInformation("Validation pipeline completed with outcome: {Outcome}", context.OverallOutcome);
            
            return context;
        }
    }
}
