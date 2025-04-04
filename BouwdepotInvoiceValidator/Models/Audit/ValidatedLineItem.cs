using System;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Represents a validated line item from an invoice with simplified structure
    /// </summary>
    public class ValidatedLineItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsApproved { get; set; }
        public string ValidationNote { get; set; } = string.Empty;
    }
}
