namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Status of a processing step
    /// </summary>
    public enum ProcessingStepStatus
    {
        /// <summary>
        /// Step is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Step completed successfully
        /// </summary>
        Success,

        /// <summary>
        /// Step completed with warnings
        /// </summary>
        Warning,

        /// <summary>
        /// Step failed
        /// </summary>
        Error,

        /// <summary>
        /// Step was skipped
        /// </summary>
        Skipped
    }
}
