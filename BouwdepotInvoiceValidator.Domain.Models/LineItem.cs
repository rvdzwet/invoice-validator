namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Line item in an invoice
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }

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

        /// <summary>
        /// Whether this item is eligible for Bouwdepot
        /// </summary>
        public bool IsBouwdepotEligible { get; set; }

        /// <summary>
        /// Reason why this item is or is not eligible for Bouwdepot
        /// </summary>
        public string EligibilityReason { get; set; }
    }
}
