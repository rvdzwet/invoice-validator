using System;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Represents payment details for an invoice
    /// </summary>
    public class PaymentDetails
    {
        /// <summary>
        /// The name of the account holder
        /// </summary>
        public string AccountHolderName { get; set; } = string.Empty;
        
        /// <summary>
        /// International Bank Account Number
        /// </summary>
        public string IBAN { get; set; } = string.Empty;
        
        /// <summary>
        /// Bank Identifier Code (SWIFT code)
        /// </summary>
        public string BIC { get; set; } = string.Empty;
        
        /// <summary>
        /// Bank name
        /// </summary>
        public string BankName { get; set; } = string.Empty;
        
        /// <summary>
        /// Payment reference or invoice number to include with payment
        /// </summary>
        public string PaymentReference { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the payment details have been verified
        /// </summary>
        public bool IsVerified { get; set; }
        
        /// <summary>
        /// When the payment details were last verified
        /// </summary>
        public DateTime? LastVerifiedDate { get; set; }
        
        /// <summary>
        /// Masking method for IBAN display
        /// </summary>
        public string MaskedIBAN
        {
            get
            {
                if (string.IsNullOrEmpty(IBAN) || IBAN.Length < 8)
                    return IBAN;
                    
                return IBAN.Substring(0, 4) + "****" + IBAN.Substring(IBAN.Length - 4);
            }
        }
    }
}
