using System;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Vendor bank account details for payment processing
    /// </summary>
    public class VendorBankDetails
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;
        public string BIC { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
    }
}
