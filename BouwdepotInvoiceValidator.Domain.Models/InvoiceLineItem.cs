namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Represents a single line item in an invoice
    /// </summary>
    public class InvoiceLineItem
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantity of the item
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Price per unit
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Total price for this line item
        /// </summary>
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// VAT/tax rate applied to this item
        /// </summary>
        public decimal VatRate { get; set; }
    }
}
