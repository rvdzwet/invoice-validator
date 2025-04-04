using System;
using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models.Audit
{
    /// <summary>
    /// Document validation results in a simplified format
    /// </summary>
    public class DocumentValidation
    {
        public bool IsValid { get; set; }
        public int Score { get; set; }
        public string PrimaryReason { get; set; } = string.Empty;
        public List<string> ValidationDetails { get; set; } = new List<string>();
    }
}
