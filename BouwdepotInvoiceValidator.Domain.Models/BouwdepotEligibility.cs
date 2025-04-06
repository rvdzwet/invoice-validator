namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Bouwdepot eligibility assessment
    /// </summary>
    public class BouwdepotEligibility
    {
        /// <summary>
        /// Whether the invoice is eligible for Bouwdepot
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Reason why the invoice is or is not eligible for Bouwdepot
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Percentage of the invoice that is eligible for Bouwdepot (0-100)
        /// </summary>
        public decimal EligiblePercentage { get; set; }

        /// <summary>
        /// Amount of the invoice that is eligible for Bouwdepot
        /// </summary>
        public decimal EligibleAmount { get; set; }
    }
}
