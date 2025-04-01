using System.IO;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Interface for the PDF extraction service
    /// </summary>
    public interface IPdfExtractionService
    {
        /// <summary>
        /// Extracts text and data from a PDF file
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>A task representing the extraction operation with the extracted invoice data</returns>
        Task<Invoice> ExtractFromPdfAsync(Stream fileStream, string fileName);
        
        /// <summary>
        /// Checks for signs of tampering or manipulation in the PDF
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <returns>True if tampering is detected, otherwise false</returns>
        Task<bool> DetectTamperingAsync(Stream fileStream);
    }
}
