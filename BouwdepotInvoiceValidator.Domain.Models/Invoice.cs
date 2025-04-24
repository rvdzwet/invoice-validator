namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Represents an invoice with all extracted data
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Invoice number
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }
        
        /// <summary>
        /// Due date for payment
        /// </summary>
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Total amount of the invoice
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// VAT/tax amount
        /// </summary>
        public decimal VatAmount { get; set; }
        
        /// <summary>
        /// Currency code (e.g., "EUR", "USD")
        /// </summary>
        public string Currency { get; set; } = "EUR";
        
        /// <summary>
        /// Name of the vendor/supplier
        /// </summary>
        public string VendorName { get; set; } = string.Empty;
        
        /// <summary>
        /// Address of the vendor/supplier
        /// </summary>
        public string VendorAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// KvK (Chamber of Commerce) number of the vendor
        /// </summary>
        public string VendorKvkNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// BTW (VAT) number of the vendor
        /// </summary>
        public string VendorBtwNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Raw text extracted from the invoice
        /// </summary>
        public string RawText { get; set; } = string.Empty;
        
        /// <summary>
        /// Line items in the invoice
        /// </summary>
        public List<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        
        /// <summary>
        /// Payment details for the invoice
        /// </summary>
        public PaymentDetails PaymentDetails { get; set; } = new PaymentDetails();
        
        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Creation date of the file
        /// </summary>
        public DateTime? FileCreationDate { get; set; }
        
        /// <summary>
        /// Last modification date of the file
        /// </summary>
        public DateTime? FileModificationDate { get; set; }
    }
}
