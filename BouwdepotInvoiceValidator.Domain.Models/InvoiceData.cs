namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Extracted invoice data
    /// </summary>
    public class InvoiceData
    {
        /// <summary>
        /// Invoice number
        /// </summary>
        public string InvoiceNumber { get; set; }

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
        public string VendorName { get; set; }

        /// <summary>
        /// Address of the vendor/supplier
        /// </summary>
        public string VendorAddress { get; set; }

        /// <summary>
        /// KvK (Chamber of Commerce) number of the vendor
        /// </summary>
        public string VendorKvkNumber { get; set; }

        /// <summary>
        /// BTW (VAT) number of the vendor
        /// </summary>
        public string VendorBtwNumber { get; set; }

        /// <summary>
        /// Payment details
        /// </summary>
        public PaymentInfo Payment { get; set; } = new PaymentInfo();

        /// <summary>
        /// Line items in the invoice
        /// </summary>
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
    }
}
