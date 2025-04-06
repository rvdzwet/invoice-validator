using BouwdepotInvoiceValidator.Domain.Models;
using System.Text.Json;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Helper methods for invoice validation
    /// </summary>
    public static class InvoiceValidationHelpers
    {
        /// <summary>
        /// Loads a prompt template from a JSON file
        /// </summary>
        public static async Task<PromptTemplate> LoadPromptTemplateAsync(string relativePath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", relativePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Prompt template not found: {fullPath}");
            }
            
            string json = await File.ReadAllTextAsync(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<PromptTemplate>(json, options);
        }

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
                Id = context.Id,
                Status = MapOutcomeToStatus(context.OverallOutcome),
                Summary = context.OverallOutcomeSummary,
                Issues = context.Issues.Select(i => new ValidationIssue
                {
                    Type = i.IssueType,
                    Description = i.Description,
                    Severity = i.Severity,
                    Field = i.Field
                }).ToList(),
                ProcessingInfo = new ProcessingInfo
                {
                    DurationMs = durationMs,
                    ModelsUsed = context.AIModelsUsed.Select(m => $"{m.ModelName} {m.ModelVersion}").ToList(),
                    Steps = context.ProcessingSteps.Select(s => s.StepName).ToList()
                }
            };
            
            // Add invoice data if available
            if (context.ExtractedInvoice != null)
            {
                result.InvoiceData = new InvoiceData
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
                result.FraudAnalysis = new FraudAnalysisResult
                {
                    RiskLevel = context.FraudAnalysis.RiskLevel,
                    RiskScore = context.FraudAnalysis.RiskScore,
                    Summary = context.FraudAnalysis.Summary,
                    Indicators = context.FraudAnalysis.Indicators.Select(i => new FraudIndicator
                    {
                        Category = i.Category,
                        Description = i.Description,
                        Confidence = i.Confidence
                    }).ToList()
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
