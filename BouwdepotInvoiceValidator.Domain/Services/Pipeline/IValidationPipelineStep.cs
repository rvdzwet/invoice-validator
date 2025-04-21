using BouwdepotInvoiceValidator.Domain.Models;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline
{
    /// <summary>
    /// Interface for a step in the validation pipeline
    /// Each step can only execute one prompt
    /// </summary>
    public interface IValidationPipelineStep
    {
        /// <summary>
        /// Name of the pipeline step
        /// </summary>
        string StepName { get; }
        
        /// <summary>
        /// Order in which the step should be executed (lower numbers execute first)
        /// </summary>
        int Order { get; }
        
        /// <summary>
        /// Gets the prompt template path for this step
        /// </summary>
        string PromptTemplatePath { get; }
        
        /// <summary>
        /// Gets the type of the expected response from the LLM
        /// </summary>
        Type ResponseType { get; }
        
        /// <summary>
        /// Prepares the prompt for execution
        /// </summary>
        /// <param name="context">The validation context</param>
        /// <param name="documentStream">The document stream to process</param>
        /// <returns>The prepared prompt and any additional parameters</returns>
        Task<(string Prompt, List<Stream> ImageStreams, List<string> MimeTypes)> PreparePromptAsync(ValidationContext context, Stream documentStream);
        
        /// <summary>
        /// Processes the response from the LLM and updates the validation context
        /// </summary>
        /// <param name="context">The validation context to update</param>
        /// <param name="response">The response from the LLM</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ProcessResponseAsync(ValidationContext context, object response);
        
        /// <summary>
        /// Determines whether this step should execute based on the current context
        /// </summary>
        /// <param name="context">The current validation context</param>
        /// <returns>True if the step should execute, false otherwise</returns>
        bool ShouldExecute(ValidationContext context);
    }
}
