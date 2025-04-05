using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Services.AI
{
    /// <summary>
    /// Interface for services that detect the language of invoice documents
    /// </summary>
    public interface ILanguageDetector
    {
        /// <summary>
        /// Detects the primary language used in an invoice document
        /// </summary>
        /// <param name="context">The invoice analysis context containing document images and extracted text</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method should:
        /// 1. Analyze the document images and/or extracted text in the context
        /// 2. Identify the primary language used in the document
        /// 3. Set context.DetectedLanguage to the ISO 639-1 language code (e.g., "nl", "en")
        /// 4. Set context.LanguageDetectionConfidence to a value between 0.0 and 1.0
        /// 5. Add a processing step to the context log
        /// 6. Add the AI model usage to the context
        /// </remarks>
        Task DetectLanguageAsync(InvoiceAnalysisContext context);
    }
}
