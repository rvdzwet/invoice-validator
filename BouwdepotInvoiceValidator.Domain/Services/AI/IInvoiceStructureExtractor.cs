using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Domain.Models.Analysis;

namespace BouwdepotInvoiceValidator.Domain.Services.AI
{
    /// <summary>
    /// Interface for services that extract structured data from invoice documents
    /// </summary>
    public interface IInvoiceStructureExtractor
    {
        /// <summary>
        /// Extracts structured invoice data from document images
        /// </summary>
        /// <param name="context">The invoice analysis context containing document images</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method should:
        /// 1. Analyze the document images in the context
        /// 2. Extract key invoice data (invoice number, date, amounts, etc.)
        /// 3. Extract vendor information (name, address, tax numbers, etc.)
        /// 4. Extract line items (descriptions, quantities, prices, etc.)
        /// 5. Extract payment details (bank account, payment references, etc.)
        /// 6. Create a structured Invoice object and set it to context.ExtractedInvoice
        /// 7. Add a processing step to the context log
        /// 8. Add the AI model usage to the context
        /// 
        /// The language used for any textual descriptions should respect context.DetectedLanguage
        /// </remarks>
        Task ExtractInvoiceStructureAsync(InvoiceAnalysisContext context);
    }
}
