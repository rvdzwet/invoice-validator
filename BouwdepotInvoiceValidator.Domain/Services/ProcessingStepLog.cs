
namespace BouwdepotInvoiceValidator.Domain.Services
{
    public class ProcessingStepLog
    {
        public string StepName { get; internal set; }
        public string Description { get; internal set; }
        public ProcessingStepStatus Status { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
    }
}