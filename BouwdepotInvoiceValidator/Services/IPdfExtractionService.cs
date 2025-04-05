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
        
        /// <summary>
        /// Extracts page images from a PDF file for visual analysis.
        /// Optimized to initially process only the first page for faster results.
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <returns>A list of page images extracted from the PDF</returns>
        Task<List<InvoicePageImage>> ExtractPageImagesAsync(Stream fileStream);
        
        /// <summary>
        /// Extracts additional pages from a PDF for more comprehensive analysis when needed.
        /// Only processes pages starting from the specified page number.
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <param name="startPage">The page number to start extracting from (default is 2)</param>
        /// <returns>A list of additional page images extracted from the PDF</returns>
        Task<List<InvoicePageImage>> ExtractAdditionalPagesAsync(Stream fileStream, int startPage = 2);
        
        /// <summary>
        /// Performs visual analysis on the PDF to detect layout, logos, signatures, and other visual elements
        /// </summary>
        /// <param name="fileStream">The PDF file stream</param>
        /// <returns>Results of visual analysis including detected elements and anomalies</returns>
        Task<VisualAnalysisResult> AnalyzeVisualElementsAsync(Stream fileStream);
    }
}
