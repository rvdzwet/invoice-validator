using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Services.AI
{
    /// <summary>
    /// Interface for services that analyze the layout of invoice documents
    /// </summary>
    public interface IDocumentLayoutAnalyzer
    {
        /// <summary>
        /// Analyzes the layout of an invoice document to identify key regions and elements
        /// </summary>
        /// <param name="context">The invoice analysis context containing document images</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method should:
        /// 1. Analyze the document images in the context
        /// 2. Identify key regions (header, footer, line items table, etc.)
        /// 3. Detect visual elements (logo, signature, stamp, etc.)
        /// 4. Populate the document structure information in the context
        /// 5. Add a processing step to the context log
        /// 6. Add the AI model usage to the context
        /// 
        /// The language used for any textual descriptions should respect context.DetectedLanguage
        /// </remarks>
        Task AnalyzeDocumentLayoutAsync(InvoiceAnalysisContext context);
    }
}
