
namespace BouwdepotInvoiceValidator.Domain.Services
{
    public class InputDocumentInfo
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTimeOffset UploadTimestamp { get; set; }
    }
}