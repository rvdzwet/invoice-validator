using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Represents an invoice with all extracted data
    /// </summary>
    public class Invoice
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public string VendorKvkNumber { get; set; } = string.Empty;
        public string VendorBtwNumber { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public List<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        
        // Original file metadata
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime? FileCreationDate { get; set; }
        public DateTime? FileModificationDate { get; set; }
    }

    /// <summary>
    /// Represents a single line item in an invoice
    /// </summary>
    public class InvoiceLineItem
    {
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal VatRate { get; set; }
    }
}
