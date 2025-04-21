using BouwdepotInvoiceValidator.Domain.Models;
using System.Text.Json;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Helper methods for invoice validation
    /// </summary>
    public static class InvoiceValidationHelpers
    {
        // LoadPromptTemplateAsync method removed as it's now handled by the DynamicPromptService

        /// <summary>
        /// Parses a date string into a DateTime object
        /// </summary>
        public static DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }
            
            // Try standard formats
            if (DateTime.TryParse(dateString, out DateTime result))
            {
                return result;
            }
            
            // Try common European formats (dd-MM-yyyy, dd/MM/yyyy, dd.MM.yyyy)
            string[] formats = { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd" };
            if (DateTime.TryParseExact(dateString, formats, System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out DateTime exactResult))
            {
                return exactResult;
            }
            
            return null;
        }

        /// <summary>
        /// Maps a ValidationContext to a ValidationResult
        /// </summary>
        public static ValidationResult MapToValidationResult(ValidationContext context, long durationMs)
        {
            var result = new ValidationResult
            {
                ValidationId = context.Id,
                Status = MapOutcomeToStatus(context.OverallOutcome),
                Summary = context.OverallOutcomeSummary,
                Issues = context.Issues.Select(i => new ValidationIssue
                {
                    Type = i.IssueType,
                    Description = i.Description,
                    Severity = i.Severity,
                    Field = i.Field
                }).ToList(),
                Processing = new ProcessingInfo
                {
                    DurationMs = durationMs,
                    ModelsUsed = context.AIModelsUsed.Select(m => $"{m.ModelName} {m.ModelVersion}").ToList(),
                    Steps = context.ProcessingSteps.Select(s => s.StepName).ToList(),
                    
                    // Add detailed processing steps
                    DetailedSteps = context.ProcessingSteps.Select(s => new ProcessingStep
                    {
                        Name = s.StepName,
                        Description = s.Description,
                        Status = s.Status.ToString(),
                        Timestamp = s.Timestamp
                    }).ToList(),
                    
                    // Add detailed AI model usage
                    DetailedModelsUsed = context.AIModelsUsed.Select(m => new AIModelUsageInfo
                    {
                        ModelName = m.ModelName,
                        ModelVersion = m.ModelVersion,
                        Operation = m.Operation,
                        TokenCount = m.TokenCount,
                        Timestamp = m.Timestamp
                    }).ToList()
                }
            };
            
            // Add conversation history if available
            if (context.ConversationContext != null && context.ConversationContext.Messages.Any())
            {
                result.Processing.ConversationHistory = context.ConversationContext.Messages.Select(m => new ConversationMessage
                {
                    Role = m.Role,
                    Content = m.Content,
                    StepName = m.StepName,
                    Timestamp = m.Timestamp
                }).ToList();
            }
            
            // Add invoice data if available
            if (context.ExtractedInvoice != null)
            {
                result.Invoice = new InvoiceData
                {
                    InvoiceNumber = context.ExtractedInvoice.InvoiceNumber,
                    InvoiceDate = context.ExtractedInvoice.InvoiceDate,
                    DueDate = context.ExtractedInvoice.DueDate,
                    TotalAmount = context.ExtractedInvoice.TotalAmount,
                    VatAmount = context.ExtractedInvoice.VatAmount,
                    Currency = context.ExtractedInvoice.Currency,
                    VendorName = context.ExtractedInvoice.VendorName,
                    VendorAddress = context.ExtractedInvoice.VendorAddress,
                    VendorKvkNumber = context.ExtractedInvoice.VendorKvkNumber,
                    VendorBtwNumber = context.ExtractedInvoice.VendorBtwNumber,
                    Payment = context.ExtractedInvoice.PaymentDetails != null ? new PaymentInfo
                    {
                        PaymentReference = context.ExtractedInvoice.PaymentDetails.PaymentReference,
                        AccountHolderName = context.ExtractedInvoice.PaymentDetails.AccountHolderName,
                        IBAN = context.ExtractedInvoice.PaymentDetails.IBAN,
                        BIC = context.ExtractedInvoice.PaymentDetails.BIC,
                        BankName = context.ExtractedInvoice.PaymentDetails.BankName
                    } : new PaymentInfo(),
                    LineItems = context.ExtractedInvoice.LineItems.Select(li => new LineItem
                    {
                        Description = li.Description,
                        Quantity = li.Quantity,
                        UnitPrice = li.UnitPrice,
                        TotalPrice = li.TotalPrice,
                        VatRate = li.VatRate,
                        IsBouwdepotEligible = li.IsBouwdepotEligible,
                        EligibilityReason = li.EligibilityReason
                    }).ToList()
                };
            }
            
            // Add fraud analysis if available
            if (context.FraudAnalysis != null)
            {
                // Add fraud indicators
                result.FraudIndicators = context.FraudAnalysis.Indicators.Select(i => new FraudIndicator
                {
                    Category = i.Category,
                    Description = i.Description,
                    Confidence = i.Confidence
                }).ToList();
                
                // Set fraud risk assessment
                result.FraudRisk = new FraudRiskAssessment
                {
                    RiskLevel = (FraudRiskLevel)Enum.Parse(typeof(FraudRiskLevel), context.FraudAnalysis.RiskLevel, true),
                    RiskScore = context.FraudAnalysis.RiskScore,
                    Summary = context.FraudAnalysis.Summary
                };
            }
            
            return result;
        }

        /// <summary>
        /// Maps a ValidationOutcome to a ValidationStatus
        /// </summary>
        private static ValidationStatus MapOutcomeToStatus(ValidationOutcome outcome)
        {
            return outcome switch
            {
                ValidationOutcome.Valid => ValidationStatus.Valid,
                ValidationOutcome.Invalid => ValidationStatus.Invalid,
                ValidationOutcome.NeedsReview => ValidationStatus.NeedsReview,
                ValidationOutcome.Error => ValidationStatus.Error,
                _ => ValidationStatus.Unknown
            };
        }
    }
}
