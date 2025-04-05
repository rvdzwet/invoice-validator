using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Services.AI
{
    /// <summary>
    /// Interface for services that detect potential fraud in invoice documents
    /// </summary>
    public interface IFraudDetector
    {
        /// <summary>
        /// Analyzes an invoice for potential fraud indicators
        /// </summary>
        /// <param name="context">The invoice analysis context containing document images and extracted invoice data</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method should:
        /// 1. Analyze the document images and extracted invoice data in the context
        /// 2. Identify potential fraud indicators (inconsistencies, manipulations, etc.)
        /// 3. Create a FraudAnalysisResult and set it to context.FraudAnalysis
        /// 4. Add a processing step to the context log
        /// 5. Add the AI model usage to the context
        /// 
        /// The language used for any textual descriptions should respect context.DetectedLanguage
        /// </remarks>
        Task DetectFraudAsync(InvoiceAnalysisContext context);
    }
}
