using System.Collections.Generic;

namespace BouwdepotInvoiceValidator.Models
{
    /// <summary>
    /// Represents the result of an invoice validation process
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        public Invoice? ExtractedInvoice { get; set; }
        public bool PossibleTampering { get; set; }
        public bool IsHomeImprovement { get; set; }
        public string RawGeminiResponse { get; set; } = string.Empty;
        
        /// <summary>
        /// Add an issue to the validation result
        /// </summary>
        /// <param name="severity">The severity of the issue</param>
        /// <param name="message">The issue message</param>
        public void AddIssue(ValidationSeverity severity, string message)
        {
            Issues.Add(new ValidationIssue
            {
                Severity = severity,
                Message = message
            });
            
            // Update IsValid based on issue severities
            if (severity == ValidationSeverity.Error)
            {
                IsValid = false;
            }
        }
    }

    /// <summary>
    /// Represents an issue found during validation
    /// </summary>
    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enum representing the severity of a validation issue
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
