namespace BouwdepotInvoiceValidator.Models.Enhanced
{
    /// <summary>
    /// Status of digital signature verification
    /// </summary>
    public enum SignatureVerificationStatus
    {
        /// <summary>
        /// Signature has not been verified
        /// </summary>
        NotVerified,
        
        /// <summary>
        /// Signature verification was successful
        /// </summary>
        Valid,
        
        /// <summary>
        /// Signature verification failed
        /// </summary>
        Invalid,
        
        /// <summary>
        /// Signature verification failed due to crypto error
        /// </summary>
        CryptoError
    }
}
