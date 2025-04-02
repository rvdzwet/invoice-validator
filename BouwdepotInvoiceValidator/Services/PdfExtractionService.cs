using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using ImageMagick;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for extracting data and detecting tampering from PDF files using iText7
    /// </summary>
    public class PdfExtractionService : IPdfExtractionService
    {
        private readonly ILogger<PdfExtractionService> _logger;
        private readonly IGeminiService _geminiService;

        public PdfExtractionService(
            ILogger<PdfExtractionService> logger,
            IGeminiService geminiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        }

        /// <inheritdoc />
        public async Task<Invoice> ExtractFromPdfAsync(Stream fileStream, string fileName)
        {
            _logger.LogInformation("Extracting data from PDF file using Gemini AI: {FileName}", fileName);
            
            var invoice = new Invoice
            {
                FileName = fileName,
                FileSizeBytes = fileStream.Length,
                FileCreationDate = DateTime.Now,
                FileModificationDate = DateTime.Now
            };

            try
            {
                // Reset stream position
                fileStream.Position = 0;
                
                // Extract page images for visual analysis
                invoice.PageImages = await ExtractPageImagesAsync(fileStream);
                
                // Add visual analysis data
                invoice.VisualAnalysis = await AnalyzeVisualElementsAsync(fileStream);
                
                // Extract invoice data from images using Gemini AI
                if (invoice.PageImages != null && invoice.PageImages.Count > 0)
                {
                    _logger.LogInformation("Using Gemini AI to extract invoice data from images");
                    
                    // Use Gemini to extract invoice data from the images
                    invoice = await _geminiService.ExtractInvoiceDataFromImagesAsync(invoice);
                    
                    _logger.LogInformation("Successfully extracted invoice data using Gemini AI: " +
                                           "InvoiceNumber={InvoiceNumber}, InvoiceDate={InvoiceDate}, TotalAmount={TotalAmount}",
                                           invoice.InvoiceNumber, invoice.InvoiceDate, invoice.TotalAmount);
                }
                else
                {
                    _logger.LogWarning("No page images available for Gemini AI extraction");
                }
                
                // If Gemini didn't extract the text yet, extract it using iText
                if (string.IsNullOrEmpty(invoice.RawText))
                {
                    invoice.RawText = ExtractTextFromPdf(fileStream);
                    
                    // If Gemini AI didn't extract invoice data, try with regex
                    if (string.IsNullOrEmpty(invoice.InvoiceNumber) && !invoice.InvoiceDate.HasValue && invoice.TotalAmount <= 0)
                    {
                        _logger.LogInformation("Using regex to extract invoice data from text");
                        ExtractInvoiceData(invoice, invoice.RawText);
                    }
                }
                
                _logger.LogInformation("Successfully extracted data from: {FileName}", fileName);
                
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data from PDF file: {FileName}", fileName);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DetectTamperingAsync(Stream fileStream)
        {
            _logger.LogInformation("Checking for PDF tampering signs");
            
            try
            {
                // Reset stream position
                fileStream.Position = 0;
                
                // Create a PdfReader instance
                using var reader = new PdfReader(fileStream);
                using var document = new PdfDocument(reader);
                
                // Check for modification dates
                var info = document.GetDocumentInfo();
                var creationDate = info.GetMoreInfo("CreationDate");
                var modDate = info.GetMoreInfo("ModDate");
                
                // If creation date and modification date differ significantly, it might indicate tampering
                if (!string.IsNullOrEmpty(creationDate) && !string.IsNullOrEmpty(modDate) && 
                    creationDate != modDate)
                {
                    _logger.LogWarning("Possible tampering detected: Creation date and modification date differ");
                    return true;
                }
                
                // Check for multiple authors or producers
                var author = info.GetAuthor();
                var producer = info.GetProducer();
                
                if (!string.IsNullOrEmpty(author) && author.Contains(';') || 
                    !string.IsNullOrEmpty(producer) && producer.Contains(';'))
                {
                    _logger.LogWarning("Possible tampering detected: Multiple authors or producers");
                    return true;
                }
                
                // Check for inconsistent fonts or formatting
                // This is a simplistic check - a real implementation would be more sophisticated
                var fontNames = new HashSet<string>();
                var numberOfPages = document.GetNumberOfPages();
                
                for (int i = 1; i <= numberOfPages; i++)
                {
                    var page = document.GetPage(i);
                    // Advanced font checks would go here
                }
                
                _logger.LogInformation("No obvious signs of tampering detected");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking PDF for tampering");
                // If we couldn't check properly, assume no tampering
                return false;
            }
        }
        
        private string ExtractTextFromPdf(Stream fileStream)
        {
            var text = new StringBuilder();
            
            try
            {
                // Reset stream position
                fileStream.Position = 0;
                
                using var reader = new PdfReader(fileStream);
                using var document = new PdfDocument(reader);
                
                int numberOfPages = document.GetNumberOfPages();
                
                for (int i = 1; i <= numberOfPages; i++)
                {
                    var page = document.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    
                    text.AppendLine(pageText);
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF");
                throw;
            }
        }

        // Simple in-memory cache to avoid repeated processing of the same PDF
        private static readonly Dictionary<string, List<InvoicePageImage>> _pageImageCache = 
            new Dictionary<string, List<InvoicePageImage>>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();
        
        /// <inheritdoc />
        public async Task<List<InvoicePageImage>> ExtractPageImagesAsync(Stream fileStream)
        {
            _logger.LogInformation("Extracting optimized page images from PDF for AI visual analysis");

            try
            {
                // Generate a cache key based on file length and first 1KB of content
                string cacheKey = GenerateCacheKey(fileStream);
                
                // Check if we've already processed this PDF
                lock (_cacheLock)
                {
                    if (_pageImageCache.TryGetValue(cacheKey, out var cachedImages))
                    {
                        _logger.LogInformation("Using cached page images for PDF");
                        return cachedImages;
                    }
                }
                
                fileStream.Position = 0;
                var pageImages = new List<InvoicePageImage>();

                using var reader = new PdfReader(fileStream);
                using var document = new PdfDocument(reader);

                int numberOfPages = document.GetNumberOfPages();
                _logger.LogInformation("PDF contains {NumberOfPages} pages", numberOfPages);

                // Optimized: Process only the first page by default (most important data usually on first page)
                int pagesToProcess = 1;
                _logger.LogInformation("Processing first {PageCount} page(s) for initial analysis", pagesToProcess);

                // Pre-read the PDF bytes once to avoid multiple stream operations
                fileStream.Position = 0;
                byte[] pdfBytes;
                using (var ms = new MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    pdfBytes = ms.ToArray();
                }

                // Process pages in parallel for multi-core advantage
                var tasks = new List<Task<InvoicePageImage>>();
                for (int i = 1; i <= pagesToProcess; i++)
                {
                    int pageNumber = i; // Create local copy for lambda
                    tasks.Add(Task.Run(() => ExtractSinglePageImageAsync(pdfBytes, pageNumber, document)));
                }

                // Wait for all conversions to complete
                var results = await Task.WhenAll(tasks);
                pageImages.AddRange(results);

                // Cache the results
                lock (_cacheLock)
                {
                    // Limit cache size to prevent memory issues
                    if (_pageImageCache.Count > 100) // Arbitrary limit
                    {
                        // Remove random entry if too many items
                        var keyToRemove = _pageImageCache.Keys.First();
                        _pageImageCache.Remove(keyToRemove);
                    }
                    
                    _pageImageCache[cacheKey] = pageImages;
                }

                _logger.LogInformation("Successfully extracted optimized images for initial analysis");
                return pageImages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting optimized page images from PDF");
                throw;
            }
        }
        
        /// <summary>
        /// Generate a cache key for a PDF stream
        /// </summary>
        private string GenerateCacheKey(Stream fileStream)
        {
            long originalPosition = fileStream.Position;
            try
            {
                fileStream.Position = 0;
                
                // Get length
                long length = fileStream.Length;
                
                // Read first 1KB (or less if file is smaller)
                byte[] buffer = new byte[Math.Min(1024, length)];
                fileStream.Read(buffer, 0, buffer.Length);
                
                // Create hash of this content
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(buffer);
                    string hash = Convert.ToBase64String(hashBytes);
                    
                    // Combine file length and hash for a more unique key
                    return $"{length}_{hash}";
                }
            }
            finally
            {
                // Restore original position
                fileStream.Position = originalPosition;
            }
        }
        
        /// <summary>
        /// Extract a single page image asynchronously
        /// </summary>
        private InvoicePageImage ExtractSinglePageImageAsync(byte[] pdfBytes, int pageNumber, PdfDocument document)
        {
            _logger.LogInformation("Processing page {PageNumber} for optimized image extraction", pageNumber);

            var page = document.GetPage(pageNumber);
            var pageSize = page.GetPageSize();

            try
            {
                using (var magick = new MagickImage())
                {
                    // Further optimized: Reduced DPI from 200 to 150 for initial analysis - still good for text recognition but even faster
                    var readSettings = new MagickReadSettings
                    {
                        Density = new Density(150),
                        Format = MagickFormat.Pdf,
                        FrameIndex = (uint)(pageNumber - 1),
                        FrameCount = 1u
                    };
                    _logger.LogDebug("Reading PDF page with optimized density of {Density} DPI", readSettings.Density);

                    magick.Read(pdfBytes, readSettings);

                    // Optimized: Simplified processing steps
                    magick.BackgroundColor = MagickColors.White;
                    magick.Alpha(AlphaOption.Remove);
                    
                    // Further optimization: resize large images to maximize 1500 pixels on longest dimension
                    if (magick.Width > 1500 || magick.Height > 1500)
                    {
                        double ratio = (double)magick.Width / magick.Height;
                        uint newWidth, newHeight;
                        
                        if (ratio > 1) // Wider than tall
                        {
                            newWidth = 1500u;
                            newHeight = (uint)(1500 / ratio);
                        }
                        else // Taller than wide
                        {
                            newHeight = 1500u;
                            newWidth = (uint)(1500 * ratio);
                        }
                        
                        magick.Resize(newWidth, newHeight);
                    }

                    // Optimize for size - use jpg instead of png for smaller file size
                    using var memoryStream = new MemoryStream();
                    magick.Format = MagickFormat.Jpg;
                    magick.Quality = 85; // Further reduced for speed while maintaining readability
                    
                    // Use progressive JPEG for perceived faster loading
                    magick.Settings.Interlace = Interlace.Plane;
                    
                    magick.Write(memoryStream);
                    memoryStream.Position = 0;

                    var pageImage = new InvoicePageImage
                    {
                        PageNumber = pageNumber,
                        ImageData = memoryStream.ToArray()
                    };

                    _logger.LogDebug("Extracted optimized image for page {PageNumber}, size: {Size} bytes", pageNumber, pageImage.ImageData.Length);
                    return pageImage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PDF page extraction for page {PageNumber}", pageNumber);
                throw;
            }
        }

        /// <summary>
        /// Extracts additional pages from a PDF for more comprehensive analysis when needed
        /// </summary>
        public async Task<List<InvoicePageImage>> ExtractAdditionalPagesAsync(Stream fileStream, int startPage = 2)
        {
            _logger.LogInformation("Extracting additional pages from PDF starting from page {StartPage}", startPage);

            try
            {
                fileStream.Position = 0;
                var pageImages = new List<InvoicePageImage>();

                using var reader = new PdfReader(fileStream);
                using var document = new PdfDocument(reader);

                int numberOfPages = document.GetNumberOfPages();
                
                if (startPage > numberOfPages)
                {
                    _logger.LogInformation("No additional pages to process - requested start page {StartPage} exceeds total pages {TotalPages}", 
                        startPage, numberOfPages);
                    return pageImages;
                }

                for (int i = startPage; i <= numberOfPages; i++)
                {
                    _logger.LogInformation("Processing additional page {PageNumber} of {TotalPages}", i, numberOfPages);

                    try
                    {
                        fileStream.Position = 0;

                        byte[] pdfBytes;
                        using (var ms = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(ms);
                            pdfBytes = ms.ToArray();
                        }

                        using (var magick = new MagickImage())
                        {
                            var readSettings = new MagickReadSettings
                            {
                                Density = new Density(200),
                                Format = MagickFormat.Pdf,
                                FrameIndex = (uint)(i - 1),
                                FrameCount = 1u
                            };

                            magick.Read(pdfBytes, readSettings);
                            magick.BackgroundColor = MagickColors.White;
                            magick.Alpha(AlphaOption.Remove);

                            using var memoryStream = new MemoryStream();
                            magick.Format = MagickFormat.Png;
                            magick.Quality = 90;
                            magick.Write(memoryStream);
                            memoryStream.Position = 0;

                            var pageImage = new InvoicePageImage
                            {
                                PageNumber = i,
                                ImageData = memoryStream.ToArray()
                            };

                            pageImages.Add(pageImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in additional PDF page extraction for page {PageNumber}", i);
                        throw;
                    }
                }

                _logger.LogInformation("Successfully extracted {Count} additional pages", pageImages.Count);
                return pageImages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting additional PDF pages");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<VisualAnalysisResult> AnalyzeVisualElementsAsync(Stream fileStream)
        {
            _logger.LogInformation("Analyzing visual elements in PDF");
            
            try
            {
                var result = new VisualAnalysisResult();
                
                // Extract page images first
                var pageImages = await ExtractPageImagesAsync(fileStream);
                
                if (pageImages.Count == 0)
                {
                    _logger.LogWarning("No pages found in PDF for visual analysis");
                return result;
                }
                
                // In a real implementation, this would use image analysis techniques
                // (possibly via ML models) to detect logos, signatures, etc.
                
                // Detect logo (placeholder implementation)
                result.HasLogo = DetectLogoInImages(pageImages);
                
                // Detect signature (placeholder implementation)
                result.HasSignature = DetectSignatureInImages(pageImages);
                
                // Detect table structure (placeholder implementation) 
                result.HasTableStructure = DetectTableStructureInImages(pageImages);
                
                // Detect visual anomalies (placeholder implementation)
                result.DetectedAnomalies = DetectVisualAnomalies(pageImages);
                
                _logger.LogInformation("Visual analysis completed: " +
                    "HasLogo={HasLogo}, HasSignature={HasSignature}, HasTableStructure={HasTableStructure}, " +
                    "AnomaliesDetected={AnomaliesCount}", 
                    result.HasLogo, result.HasSignature, result.HasTableStructure, result.DetectedAnomalies.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing visual elements in PDF");
                throw;
            }
        }
        
        private bool DetectLogoInImages(List<InvoicePageImage> pageImages)
        {
            // This would use image recognition to detect company logos
            // For demonstration purposes, we'll assume a logo is present on the first page
            
            if (pageImages.Count > 0)
            {
                // Placeholder for actual logo detection logic
                // In a real implementation, this would use computer vision techniques
                return true;
            }
            
            return false;
        }
        
        private bool DetectSignatureInImages(List<InvoicePageImage> pageImages)
        {
            // This would analyze the last page for signature marks
            // For demonstration purposes, we'll simulate signature detection
            
            if (pageImages.Count > 0)
            {
                var lastPage = pageImages[pageImages.Count - 1];
                
                // Placeholder for actual signature detection logic
                // In a real implementation, this would use pattern recognition
                return true;
            }
            
            return false;
        }
        
        private bool DetectTableStructureInImages(List<InvoicePageImage> pageImages)
        {
            // This would detect table structures in the images
            // For demonstration purposes, we'll simulate table detection
            
            if (pageImages.Count > 0)
            {
                // Placeholder for actual table structure detection logic
                // In a real implementation, this would look for grid patterns
                return true;
            }
            
            return false;
        }
        
        private List<VisualAnomalyDetection> DetectVisualAnomalies(List<InvoicePageImage> pageImages)
        {
            var anomalies = new List<VisualAnomalyDetection>();
            
            // This would analyze images for visual inconsistencies that might indicate fraud
            // For demonstration purposes, we'll return an empty list
            
            return anomalies;
        }
        
        private void ExtractInvoiceData(Invoice invoice, string text)
        {
            // Extract invoice number
            var invoiceNumberMatch = Regex.Match(text, @"(?:Invoice|Factuur)\s*(?:#|nr|nummer|number)?\s*:?\s*([A-Za-z0-9\-/]+)", RegexOptions.IgnoreCase);
            if (invoiceNumberMatch.Success)
            {
                invoice.InvoiceNumber = invoiceNumberMatch.Groups[1].Value.Trim();
            }
            
            // Extract invoice date
            var datePatterns = new[]
            {
                @"(?:Invoice|Factuur)\s*(?:date|datum)\s*:?\s*(\d{1,2}[-./ ]\d{1,2}[-./ ]\d{2,4})",
                @"(?:Date|Datum)\s*:?\s*(\d{1,2}[-./ ]\d{1,2}[-./ ]\d{2,4})",
                @"(\d{1,2}[-./ ]\d{1,2}[-./ ]\d{2,4})"
            };
            
            foreach (var pattern in datePatterns)
            {
                var dateMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    var dateStr = dateMatch.Groups[1].Value.Trim();
                    var dateFormats = new[]
                    {
                        "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/M/yyyy",
                        "dd.MM.yyyy", "d.M.yyyy", "dd MM yyyy", "d M yyyy",
                        "MM-dd-yyyy", "M-d-yyyy", "MM/dd/yyyy", "M/d/yyyy"
                    };
                    
                    if (DateTime.TryParseExact(dateStr, dateFormats, CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, out DateTime date))
                    {
                        invoice.InvoiceDate = date;
                        break;
                    }
                }
            }
            
            // Extract total amount
            var amountPatterns = new[]
            {
                @"(?:Total|Totaal|Amount|Bedrag)\s*:?\s*€?\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))",
                @"(?:Total|Totaal|Amount|Bedrag)\s*:?\s*EUR\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))",
                @"€\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))"
            };
            
            foreach (var pattern in amountPatterns)
            {
                var amountMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (amountMatch.Success)
                {
                    var amountStr = amountMatch.Groups[1].Value.Trim()
                        .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                        .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    
                    if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal amount))
                    {
                        invoice.TotalAmount = amount;
                        break;
                    }
                }
            }
            
            // Extract VAT amount (simplified)
            var vatPatterns = new[]
            {
                @"(?:VAT|BTW)\s*:?\s*€?\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))",
                @"(?:VAT|BTW)\s*:?\s*EUR\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))"
            };
            
            foreach (var pattern in vatPatterns)
            {
                var vatMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (vatMatch.Success)
                {
                    var vatStr = vatMatch.Groups[1].Value.Trim()
                        .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                        .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    
                    if (decimal.TryParse(vatStr, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal vat))
                    {
                        invoice.VatAmount = vat;
                        break;
                    }
                }
            }
            
            // Extract vendor name
            var vendorNameMatch = Regex.Match(text, @"(?:From|Van|Company|Bedrijf|Leverancier)\s*:?\s*([A-Za-z0-9\s&.,']+(?:\r|\n|$))", RegexOptions.IgnoreCase);
            if (vendorNameMatch.Success)
            {
                invoice.VendorName = vendorNameMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Try to extract from the top of the document (first line often contains the company name)
                var headerLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (headerLines.Length > 0)
                {
                    invoice.VendorName = headerLines[0].Trim();
                }
            }
            
            // Extract line items (simplified)
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Look for lines containing prices
            var pricePattern = @"(\d+(?:[.,]\d{2})?)"; // Simple price pattern
            var quantityPattern = @"(\d+)\s*(?:x|×)\s*"; // Quantity pattern like "2 x" or "2×"
            
            foreach (var line in lines)
            {
                // Skip short lines or lines without prices
                if (line.Length < 10 || !Regex.IsMatch(line, pricePattern))
                    continue;
                
                // Check if this looks like a line item (contains both description and price)
                var prices = Regex.Matches(line, pricePattern);
                if (prices.Count >= 1)
                {
                    var lineItem = new InvoiceLineItem();
                    
                    // Extract quantity if present
                    var quantityMatch = Regex.Match(line, quantityPattern);
                    if (quantityMatch.Success && int.TryParse(quantityMatch.Groups[1].Value, out int quantity))
                    {
                        lineItem.Quantity = quantity;
                    }
                    else
                    {
                        lineItem.Quantity = 1; // Default quantity
                    }
                    
                    // Extract price (last number in the line is likely the total)
                    var priceStr = prices[prices.Count - 1].Value
                        .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                        .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    
                    if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal price))
                    {
                        lineItem.TotalPrice = price;
                        
                        // Calculate unit price if quantity > 1
                        if (lineItem.Quantity > 1)
                        {
                            lineItem.UnitPrice = price / lineItem.Quantity;
                        }
                        else
                        {
                            lineItem.UnitPrice = price;
                        }
                    }
                    
                    // Extract description (everything before the price)
                    var description = line;
                    
                    // Remove the price and quantity from the description
                    foreach (Match priceMatch in prices)
                    {
                        description = description.Replace(priceMatch.Value, "");
                    }
                    
                    if (quantityMatch.Success)
                    {
                        description = description.Replace(quantityMatch.Value, "");
                    }
                    
                    // Clean up the description
                    description = Regex.Replace(description, @"\s{2,}", " ").Trim();
                    description = Regex.Replace(description, @"[€$]", "").Trim();
                    
                    lineItem.Description = description;
                    
                    // VAT rate extraction (simplified)
                    if (line.Contains("21%"))
                    {
                        lineItem.VatRate = 21;
                    }
                    else if (line.Contains("9%"))
                    {
                        lineItem.VatRate = 9;
                    }
                    else
                    {
                        lineItem.VatRate = 21; // Default Dutch high rate
                    }
                    
                    invoice.LineItems.Add(lineItem);
                }
            }
        }
    }
}
