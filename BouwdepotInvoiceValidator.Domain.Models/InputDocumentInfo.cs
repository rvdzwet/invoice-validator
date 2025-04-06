namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Information about the input document
    /// </summary>
    public class InputDocumentInfo
    {
        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// MIME type of the document
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Number of pages in the document
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// MD5 hash of the file content
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// Timestamp when the document was uploaded
        /// </summary>
        public DateTimeOffset UploadTimestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
