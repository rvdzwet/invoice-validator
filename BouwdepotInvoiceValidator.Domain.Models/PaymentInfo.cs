namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Payment information
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>
        /// The name of the account holder
        /// </summary>
        public string AccountHolderName { get; set; }

        /// <summary>
        /// International Bank Account Number
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// Bank Identifier Code (SWIFT code)
        /// </summary>
        public string BIC { get; set; }

        /// <summary>
        /// Bank name
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Payment reference or invoice number to include with payment
        /// </summary>
        public string PaymentReference { get; set; }

        /// <summary>
        /// Whether the payment details have been verified
        /// </summary>
        public bool IsVerified { get; set; }
    }
}
