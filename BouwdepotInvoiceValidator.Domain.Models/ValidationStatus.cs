namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Overall validation status
    /// </summary>
    public enum ValidationStatus
    {
        /// <summary>
        /// Validation has not been performed yet
        /// </summary>
        Unknown,

        /// <summary>
        /// The invoice is valid
        /// </summary>
        Valid,

        /// <summary>
        /// The invoice is invalid
        /// </summary>
        Invalid,

        /// <summary>
        /// The invoice requires manual review
        /// </summary>
        NeedsReview,

        /// <summary>
        /// An error occurred during validation
        /// </summary>
        Error
    }
}
