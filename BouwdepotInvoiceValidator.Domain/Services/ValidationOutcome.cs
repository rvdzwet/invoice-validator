namespace BouwdepotInvoiceValidator.Domain.Services
{
    public enum ValidationOutcome
    {
        NeedsReview,
        Valid,
        Invalid,
        Error,
        Unknown
    }
}