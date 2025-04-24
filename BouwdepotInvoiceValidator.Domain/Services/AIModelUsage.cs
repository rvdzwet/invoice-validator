
namespace BouwdepotInvoiceValidator.Domain.Services
{
    public class AIModelUsage
    {
        public string ModelName { get; internal set; }
        public string ModelVersion { get; internal set; }
        public string Operation { get; internal set; }
        public int TokenCount { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
    }
}