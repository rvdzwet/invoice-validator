using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service for extracting data and detecting tampering from PDF files using iText7
    /// </summary>
    public class PdfExtractionService : IPdfExtractionService
    {
        private readonly ILogger<PdfExtractionService> _logger;

        public PdfExtractionService(ILogger<PdfExtractionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Invoice> ExtractFromPdfAsync(Stream fileStream, string fileName)
        {
            _logger.LogInformation("Extracting data from PDF file: {FileName}", fileName);
            
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
                
                // Extract text from PDF
                var text = await ExtractTextFromPdfAsync(fileStream);
                invoice.RawText = text;

                // Extract invoice data from text
                ExtractInvoiceData(invoice, text);
                
                _logger.LogInformation("Successfully extracted invoice data: Number={InvoiceNumber}, Date={InvoiceDate}, Amount={TotalAmount}", 
                    invoice.InvoiceNumber, invoice.InvoiceDate, invoice.TotalAmount);
                
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
        
        private async Task<string> ExtractTextFromPdfAsync(Stream fileStream)
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
