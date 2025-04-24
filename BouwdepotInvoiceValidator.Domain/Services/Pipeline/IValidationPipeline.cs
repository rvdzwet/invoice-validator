namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline
{
    /// <summary>
    /// Interface for the validation pipeline
    /// </summary>
    public interface IValidationPipeline
    {
        /// <summary>
        /// Executes the validation pipeline on the provided document
        /// </summary>
        /// <param name="initialContext">The initial validation context</param>
        /// <param name="documentStream">The document stream to validate</param>
        /// <returns>The enriched validation context after all steps have executed</returns>
        Task<ValidationContext> ExecuteAsync(ValidationContext initialContext, Stream documentStream);
    }
}
