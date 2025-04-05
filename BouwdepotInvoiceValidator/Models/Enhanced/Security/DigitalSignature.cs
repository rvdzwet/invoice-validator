namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Digital signature for verification
    /// </summary>
    public class DigitalSignature
    {
        /// <summary>
        /// The actual signature value (Base64 encoded)
        /// </summary>
        public string SignatureValue { get; set; } = string.Empty;
        
        /// <summary>
        /// When the signature was created (UTC)
        /// </summary>
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Algorithm used for signing
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;
        
        /// <summary>
        /// Fields that were included in the signature
        /// </summary>
        public string SignedFields { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the entity that created the signature
        /// </summary>
        public string SignerId { get; set; } = string.Empty;
        
        /// <summary>
        /// Verification status of the signature
        /// </summary>
        public SignatureVerificationStatus VerificationStatus { get; set; } = SignatureVerificationStatus.NotVerified;
    }
}
