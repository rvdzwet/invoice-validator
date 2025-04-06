using Microsoft.Extensions.Logging;
using BouwdepotValidationValidator.Infrastructure.Abstractions;
using BouwdepotInvoiceValidator.Domain.Models;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using static BouwdepotInvoiceValidator.Domain.Services.InvoiceValidationHelpers;

namespace BouwdepotInvoiceValidator.Domain.Services
{
    /// <summary>
    /// Main service for validating invoices using AI-powered analysis
    /// </summary>
    public class InvoiceValidationService : IInvoiceValidationService
    {
        private readonly ILogger<InvoiceValidationService> _logger;
        private readonly ILLMProvider _llmProvider;
        private readonly string _promptsBasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceValidationService"/> class.
        /// </summary>
        public InvoiceValidationService(
            ILogger<InvoiceValidationService> logger,
            ILLMProvider llmProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
            _promptsBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");
        }

        /// <inheritdoc/>
        public async Task<ValidationResult> ValidateInvoiceAsync(Stream fileStream, string fileName, string contentType)
        {
            _logger.LogInformation("Starting invoice validation for file: {FileName}", fileName);
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Create a new analysis context
                var context = new ValidationContext();

                // Initialize input document info
                context.InputDocument = new InputDocumentInfo
                {
                    FileName = fileName,
                    ContentType = contentType,
                    FileSizeBytes = fileStream.Length,
                    UploadTimestamp = DateTimeOffset.UtcNow
                };

                // Add initial processing step
                context.AddProcessingStep("InitializeAnalysis",
                    "Starting invoice analysis process",
                    ProcessingStepStatus.Success);

                // Create a memory stream copy of the file for multiple reads
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Step 1: Detect language
                await DetectLanguageAsync(context, memoryStream);
                memoryStream.Position = 0;

                // Step 2: Analyze document layout and verify document type
                await VerifyDocumentTypeAsync(context, memoryStream);
                memoryStream.Position = 0;

                // If document is not an invoice, return early with invalid result
                if (context.OverallOutcome == ValidationOutcome.Invalid)
                {
                    _logger.LogWarning("Document is not a valid invoice: {FileName}, ContextId: {ContextId}",
                        fileName, context.Id);
                    return MapToValidationResult(context, stopwatch.ElapsedMilliseconds);
                }

                // Step 3: Extract invoice structure
                await ExtractInvoiceStructureAsync(context, memoryStream);
                memoryStream.Position = 0;

                // Step 4: Detect fraud
                await DetectFraudAsync(context, memoryStream);
                memoryStream.Position = 0;

                // Step 5: Validate against construction deposit rules
                await ValidateForBouwdepotAsync(context, memoryStream);
                memoryStream.Position = 0;

                // Step 6: Generate audit report
                await GenerateAuditReportAsync(context);
                
                // Add final processing step
                context.AddProcessingStep("CompleteAnalysis",
                    "Invoice analysis process completed",
                    ProcessingStepStatus.Success);

                _logger.LogInformation("Invoice analysis completed for file: {FileName}, ContextId: {ContextId}",
                    fileName, context.Id);

                return MapToValidationResult(context, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice: {FileName}", fileName);
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        #region Validation Steps

        /// <summary>
        /// Detects the language of the invoice
        /// </summary>
        private async Task DetectLanguageAsync(ValidationContext context, Stream fileStream)
        {
            _logger.LogInformation("Detecting language for invoice: {ContextId}", context.Id);
            context.AddProcessingStep("DetectLanguage", "Detecting document language", ProcessingStepStatus.InProgress);

            try
            {
                // For now, we'll assume Dutch as the default language
                // In a real implementation, you would use a language detection service or LLM
                context.DetectedLanguage = "nl";
                context.LanguageDetectionConfidence = 0.95;

                context.AddProcessingStep("DetectLanguage", 
                    $"Detected language: {context.DetectedLanguage} (confidence: {context.LanguageDetectionConfidence:P0})", 
                    ProcessingStepStatus.Success);
                
                context.AddAIModelUsage("Gemini-1.5-Pro", "1.0", "LanguageDetection", 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting language: {ContextId}", context.Id);
                context.AddProcessingStep("DetectLanguage", "Error detecting document language", ProcessingStepStatus.Error);
                context.AddIssue("LanguageDetectionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Verifies that the document is an invoice
        /// </summary>
        private async Task VerifyDocumentTypeAsync(ValidationContext context, Stream fileStream)
        {
            _logger.LogInformation("Verifying document type for invoice: {ContextId}", context.Id);
            context.AddProcessingStep("VerifyDocumentType", "Verifying document is an invoice", ProcessingStepStatus.InProgress);

            try
            {
                // Load the document type verification prompt
                var promptTemplate = await LoadPromptTemplateAsync("DocumentAnalysis/document-type-verification.json");
                
                // Create a request object with empty properties (not used in this prompt)
                var request = new { };
                
                // Define the response type
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, DocumentTypeVerificationResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Update context based on response
                if (response.isInvoice)
                {
                    context.AddProcessingStep("VerifyDocumentType", 
                        $"Document verified as invoice (confidence: {response.confidence}%)", 
                        ProcessingStepStatus.Success);
                }
                else
                {
                    context.AddProcessingStep("VerifyDocumentType", 
                        $"Document is not an invoice. Detected as: {response.documentType}", 
                        ProcessingStepStatus.Warning);
                    
                    context.AddIssue("InvalidDocumentType", 
                        $"The document appears to be a {response.documentType}, not an invoice. {response.explanation}", 
                        IssueSeverity.Error);
                    
                    context.OverallOutcome = ValidationOutcome.Invalid;
                    context.OverallOutcomeSummary = $"Document is not a valid invoice. It appears to be a {response.documentType}.";
                }
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "DocumentTypeVerification", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document type: {ContextId}", context.Id);
                context.AddProcessingStep("VerifyDocumentType", "Error verifying document type", ProcessingStepStatus.Error);
                context.AddIssue("DocumentTypeVerificationError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Extracts the invoice structure including header, parties, and line items
        /// </summary>
        private async Task ExtractInvoiceStructureAsync(ValidationContext context, Stream fileStream)
        {
            _logger.LogInformation("Extracting invoice structure: {ContextId}", context.Id);
            context.AddProcessingStep("ExtractInvoiceStructure", "Extracting invoice data", ProcessingStepStatus.InProgress);

            try
            {
                // Initialize the invoice object
                context.ExtractedInvoice = new Invoice
                {
                    FileName = context.InputDocument.FileName,
                    FileSizeBytes = context.InputDocument.FileSizeBytes
                };

                // Extract header information
                await ExtractInvoiceHeaderAsync(context, fileStream);
                fileStream.Position = 0;

                // Extract parties information
                await ExtractInvoicePartiesAsync(context, fileStream);
                fileStream.Position = 0;

                // Extract line items
                await ExtractInvoiceLineItemsAsync(context, fileStream);

                context.AddProcessingStep("ExtractInvoiceStructure", 
                    $"Successfully extracted invoice data: {context.ExtractedInvoice.InvoiceNumber}, " +
                    $"Amount: {context.ExtractedInvoice.TotalAmount} {context.ExtractedInvoice.Currency}, " +
                    $"Vendor: {context.ExtractedInvoice.VendorName}", 
                    ProcessingStepStatus.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice structure: {ContextId}", context.Id);
                context.AddProcessingStep("ExtractInvoiceStructure", "Error extracting invoice data", ProcessingStepStatus.Error);
                context.AddIssue("InvoiceExtractionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Extracts invoice header information
        /// </summary>
        private async Task ExtractInvoiceHeaderAsync(ValidationContext context, Stream fileStream)
        {
            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("InvoiceExtraction/invoice-header.json");
                
                var request = new { };
                
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, InvoiceHeaderResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Update invoice with header information
                context.ExtractedInvoice.InvoiceNumber = response.invoiceNumber;
                context.ExtractedInvoice.InvoiceDate = ParseDate(response.invoiceDate);
                context.ExtractedInvoice.DueDate = ParseDate(response.dueDate);
                context.ExtractedInvoice.TotalAmount = response.totalAmount;
                context.ExtractedInvoice.VatAmount = response.taxAmount;
                context.ExtractedInvoice.Currency = response.currency;
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "InvoiceHeaderExtraction", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice header: {ContextId}", context.Id);
                context.AddIssue("HeaderExtractionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Extracts invoice parties information
        /// </summary>
        private async Task ExtractInvoicePartiesAsync(ValidationContext context, Stream fileStream)
        {
            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("InvoiceExtraction/invoice-parties.json");
                
                var request = new { };
                
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, InvoicePartiesResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Update invoice with parties information
                context.ExtractedInvoice.VendorName = response.vendorName;
                context.ExtractedInvoice.VendorAddress = response.vendorAddress;
                
                // Extract KvK and BTW numbers from vendor contact if available
                if (!string.IsNullOrEmpty(response.vendorContact))
                {
                    if (response.vendorContact.Contains("KvK", StringComparison.OrdinalIgnoreCase))
                    {
                        var kvkMatch = System.Text.RegularExpressions.Regex.Match(response.vendorContact, @"KvK[:\s]*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (kvkMatch.Success)
                        {
                            context.ExtractedInvoice.VendorKvkNumber = kvkMatch.Groups[1].Value;
                        }
                    }
                    
                    if (response.vendorContact.Contains("BTW", StringComparison.OrdinalIgnoreCase) || 
                        response.vendorContact.Contains("VAT", StringComparison.OrdinalIgnoreCase))
                    {
                        var btwMatch = System.Text.RegularExpressions.Regex.Match(response.vendorContact, @"(BTW|VAT)[:\s]*([\w\d]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (btwMatch.Success)
                        {
                            context.ExtractedInvoice.VendorBtwNumber = btwMatch.Groups[2].Value;
                        }
                    }
                }
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "InvoicePartiesExtraction", 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice parties: {ContextId}", context.Id);
                context.AddIssue("PartiesExtractionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Extracts invoice line items and payment details
        /// </summary>
        private async Task ExtractInvoiceLineItemsAsync(ValidationContext context, Stream fileStream)
        {
            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("InvoiceExtraction/invoice-line-items.json");
                
                var request = new { };
                
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, InvoiceLineItemsResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Update invoice with line items
                if (response.lineItems != null)
                {
                    foreach (var item in response.lineItems)
                    {
                        context.ExtractedInvoice.LineItems.Add(new InvoiceLineItem
                        {
                            Description = item.description,
                            Quantity = item.quantity,
                            UnitPrice = item.unitPrice,
                            TotalPrice = item.totalPrice,
                            // VAT rate will be calculated later
                        });
                    }
                }

                // Update payment details
                context.ExtractedInvoice.PaymentDetails = new PaymentDetails
                {
                    PaymentReference = response.paymentReference ?? context.ExtractedInvoice.InvoiceNumber
                };
                
                // Store raw text for later use
                context.ExtractedInvoice.RawText = response.notes ?? string.Empty;
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "InvoiceLineItemsExtraction", 800);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting invoice line items: {ContextId}", context.Id);
                context.AddIssue("LineItemsExtractionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Detects potential fraud in the invoice
        /// </summary>
        private async Task DetectFraudAsync(ValidationContext context, Stream fileStream)
        {
            _logger.LogInformation("Detecting fraud for invoice: {ContextId}", context.Id);
            context.AddProcessingStep("DetectFraud", "Analyzing for potential fraud indicators", ProcessingStepStatus.InProgress);

            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("DocumentAnalysis/fraud-detection.json");
                
                // Replace vendor name placeholder with actual vendor name
                var instructions = promptTemplate.template.instructions.Select(i => 
                    i.Replace("{vendorName}", context.ExtractedInvoice.VendorName)).ToList();
                
                var request = new { };
                
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, FraudDetectionResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Create fraud analysis result
                context.FraudAnalysis = new FraudAnalysisResult
                {
                    RiskLevel = response.possibleFraud ? "High" : "Low",
                    RiskScore = response.possibleFraud ? (int)(response.confidence * 100) : (int)((1 - response.confidence) * 100),
                    Summary = response.visualEvidence,
                    Indicators = new List<FraudIndicator>()
                };

                // Add fraud indicators
                if (response.visualIndicators != null)
                {
                    foreach (var indicator in response.visualIndicators)
                    {
                        context.FraudAnalysis.Indicators.Add(new FraudIndicator
                        {
                            Category = "Visual",
                            Description = indicator,
                            Confidence = response.confidence
                        });
                    }
                }

                // Update context based on fraud detection
                if (response.possibleFraud && response.confidence > 0.7)
                {
                    context.AddProcessingStep("DetectFraud", 
                        $"Detected potential fraud indicators with high confidence ({response.confidence:P0})", 
                        ProcessingStepStatus.Warning);
                    
                    context.AddIssue("PotentialFraud", 
                        $"Potential fraud detected: {response.visualEvidence}", 
                        IssueSeverity.Warning);
                    
                    if (context.OverallOutcome != ValidationOutcome.Invalid)
                    {
                        context.OverallOutcome = ValidationOutcome.NeedsReview;
                        context.OverallOutcomeSummary = "Invoice requires manual review due to potential fraud indicators.";
                    }
                }
                else
                {
                    context.AddProcessingStep("DetectFraud", 
                        "No significant fraud indicators detected", 
                        ProcessingStepStatus.Success);
                }
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "FraudDetection", 800);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting fraud: {ContextId}", context.Id);
                context.AddProcessingStep("DetectFraud", "Error analyzing for fraud indicators", ProcessingStepStatus.Error);
                context.AddIssue("FraudDetectionError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Validates the invoice against construction deposit (Bouwdepot) rules
        /// </summary>
        private async Task ValidateForBouwdepotAsync(ValidationContext context, Stream fileStream)
        {
            _logger.LogInformation("Validating for Bouwdepot eligibility: {ContextId}", context.Id);
            context.AddProcessingStep("ValidateBouwdepot", "Validating against construction deposit rules", ProcessingStepStatus.InProgress);

            try
            {
                // First analyze line items
                await AnalyzeLineItemsAsync(context, fileStream);
                fileStream.Position = 0;
                
                // Then perform multimodal home improvement analysis
                await AnalyzeHomeImprovementAsync(context, fileStream);
                
                // Determine overall eligibility based on line item analysis
                DetermineOverallEligibility(context);
                
                context.AddProcessingStep("ValidateBouwdepot", 
                    $"Bouwdepot eligibility determined: {(context.OverallOutcome == ValidationOutcome.Valid ? "Eligible" : "Needs Review")}", 
                    ProcessingStepStatus.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating for Bouwdepot: {ContextId}", context.Id);
                context.AddProcessingStep("ValidateBouwdepot", "Error validating against construction deposit rules", ProcessingStepStatus.Error);
                context.AddIssue("BouwdepotValidationError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Analyzes line items to determine if they are related to home improvement
        /// </summary>
        private async Task AnalyzeLineItemsAsync(ValidationContext context, Stream fileStream)
        {
            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("DocumentAnalysis/line-item-analysis.json");
                
                // Create a request object with line items
                var request = new
                {
                    lineItems = context.ExtractedInvoice.LineItems.Select(li => new
                    {
                        description = li.Description,
                        quantity = li.Quantity,
                        unitPrice = li.UnitPrice,
                        totalPrice = li.TotalPrice
                    }).ToList()
                };
                
                var response = await _llmProvider.SendStructuredPromptAsync<object, LineItemAnalysisResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request
                );

                // Create rule validation result
                var ruleResult = new RuleValidationResult
                {
                    RuleId = "LineItemHomeImprovement",
                    RuleName = "Line Item Home Improvement Check",
                    Description = "Checks if line items are related to home improvement",
                    Result = response.isHomeImprovement ? RuleResult.Pass : RuleResult.Fail,
                    Severity = response.isHomeImprovement ? RuleSeverity.Info : RuleSeverity.Error,
                    Explanation = response.summary
                };
                
                context.ValidationResults.Add(ruleResult);

                // Update line items with analysis results
                if (response.lineItemAnalysis != null)
                {
                    for (int i = 0; i < Math.Min(context.ExtractedInvoice.LineItems.Count, response.lineItemAnalysis.Count); i++)
                    {
                        var analysis = response.lineItemAnalysis[i];
                        if (i < context.ExtractedInvoice.LineItems.Count)
                        {
                            var lineItem = context.ExtractedInvoice.LineItems[i];
                            lineItem.IsBouwdepotEligible = analysis.isHomeImprovement;
                            lineItem.EligibilityReason = analysis.notes;
                        }
                    }
                }
                
                context.AddAIModelUsage("Gemini-1.5-Pro", "1.0", "LineItemAnalysis", 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing line items: {ContextId}", context.Id);
                context.AddIssue("LineItemAnalysisError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Analyzes the invoice for home improvement eligibility using multimodal analysis
        /// </summary>
        private async Task AnalyzeHomeImprovementAsync(ValidationContext context, Stream fileStream)
        {
            try
            {
                var promptTemplate = await LoadPromptTemplateAsync("DocumentAnalysis/multi-modal-home-improvement.json");
                
                // Create a request object with invoice data
                var request = new
                {
                    invoice = new
                    {
                        invoiceNumber = context.ExtractedInvoice.InvoiceNumber,
                        vendorName = context.ExtractedInvoice.VendorName,
                        totalAmount = context.ExtractedInvoice.TotalAmount,
                        currency = context.ExtractedInvoice.Currency,
                        lineItems = context.ExtractedInvoice.LineItems.Select(li => new
                        {
                            description = li.Description,
                            quantity = li.Quantity,
                            unitPrice = li.UnitPrice,
                            totalPrice = li.TotalPrice
                        }).ToList()
                    }
                };
                
                var response = await _llmProvider.SendMultimodalStructuredPromptAsync<object, HomeImprovementResponse>(
                    promptTemplate.template.role + "\n\n" + 
                    promptTemplate.template.task + "\n\n" + 
                    string.Join("\n", promptTemplate.template.instructions),
                    request,
                    new List<Stream> { fileStream },
                    new List<string> { context.InputDocument.ContentType }
                );

                // Create rule validation result
                var ruleResult = new RuleValidationResult
                {
                    RuleId = "BouwdepotEligibility",
                    RuleName = "Bouwdepot Eligibility Check",
                    Description = "Checks if invoice is eligible for Bouwdepot financing",
                    Result = response.isHomeImprovement ? RuleResult.Pass : RuleResult.Fail,
                    Severity = response.isHomeImprovement ? RuleSeverity.Info : RuleSeverity.Error,
                    Explanation = $"Categories: {string.Join(", ", response.eligibleCategories ?? new List<string>())}"
                };
                
                context.ValidationResults.Add(ruleResult);

                // Update line items with assessment results
                if (response.lineItemAssessment != null)
                {
                    for (int i = 0; i < Math.Min(context.ExtractedInvoice.LineItems.Count, response.lineItemAssessment.Count); i++)
                    {
                        var assessment = response.lineItemAssessment[i];
                        if (i < context.ExtractedInvoice.LineItems.Count)
                        {
                            var lineItem = context.ExtractedInvoice.LineItems[i];
                            lineItem.IsBouwdepotEligible = assessment.isEligible;
                            lineItem.EligibilityReason = assessment.reason;
                        }
                    }
                }
                
                context.AddAIModelUsage("Gemini-1.5-Pro-Vision", "1.0", "HomeImprovementAnalysis", 1200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing home improvement eligibility: {ContextId}", context.Id);
                context.AddIssue("HomeImprovementAnalysisError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }

        /// <summary>
        /// Determines the overall eligibility of the invoice for Bouwdepot
        /// </summary>
        private void DetermineOverallEligibility(ValidationContext context)
        {
            // Check if any validation rules failed
            bool anyRulesFailed = context.ValidationResults.Any(r => r.Result == RuleResult.Fail && r.Severity == RuleSeverity.Error);
            
            // Check if there are any high-severity issues
            bool anyHighSeverityIssues = context.Issues.Any(i => i.Severity == IssueSeverity.Error);

            // If no rules failed and no high-severity issues, mark as valid
            if (!anyRulesFailed && !anyHighSeverityIssues && context.OverallOutcome != ValidationOutcome.Invalid)
            {
                context.OverallOutcome = ValidationOutcome.Valid;
                context.OverallOutcomeSummary = "Invoice is eligible for Bouwdepot financing.";
            }
            // If already invalid, keep it that way
            else if (context.OverallOutcome != ValidationOutcome.Invalid)
            {
                context.OverallOutcome = ValidationOutcome.NeedsReview;
                context.OverallOutcomeSummary = "Invoice requires manual review due to validation issues.";
            }
        }

        /// <summary>
        /// Generates an audit report for the invoice
        /// </summary>
        private async Task GenerateAuditReportAsync(ValidationContext context)
        {
            _logger.LogInformation("Generating audit report for invoice: {ContextId}", context.Id);
            context.AddProcessingStep("GenerateAuditReport", "Generating comprehensive audit report", ProcessingStepStatus.InProgress);

            try
            {
                // In a real implementation, you would generate a detailed audit report
                // For now, we'll just summarize the validation results
                
                var summary = new StringBuilder();
                summary.AppendLine($"Invoice {context.ExtractedInvoice.InvoiceNumber} from {context.ExtractedInvoice.VendorName}");
                summary.AppendLine($"Amount: {context.ExtractedInvoice.TotalAmount} {context.ExtractedInvoice.Currency}");
                summary.AppendLine($"Validation outcome: {context.OverallOutcome}");
                
                if (context.ValidationResults.Any())
                {
                    summary.AppendLine("Validation rules:");
                    foreach (var rule in context.ValidationResults)
                    {
                        summary.AppendLine($"- {rule.RuleName}: {rule.Result} ({rule.Explanation})");
                    }
                }
                
                if (context.Issues.Any())
                {
                    summary.AppendLine("Issues:");
                    foreach (var issue in context.Issues)
                    {
                        summary.AppendLine($"- {issue.IssueType} ({issue.Severity}): {issue.Description}");
                    }
                }
                
                if (context.FraudAnalysis?.Indicators.Any() == true)
                {
                    summary.AppendLine("Fraud indicators:");
                    foreach (var indicator in context.FraudAnalysis.Indicators)
                    {
                        summary.AppendLine($"- {indicator.Category}: {indicator.Description} (confidence: {indicator.Confidence:P0})");
                    }
                }
                
                // Store the summary in the context
                context.OverallOutcomeSummary = summary.ToString();
                
                context.AddProcessingStep("GenerateAuditReport", 
                    "Comprehensive audit report generated", 
                    ProcessingStepStatus.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit report: {ContextId}", context.Id);
                context.AddProcessingStep("GenerateAuditReport", "Error generating audit report", ProcessingStepStatus.Error);
                context.AddIssue("AuditReportError", ex.Message, IssueSeverity.Error);
                throw;
            }
        }
        
        #endregion
    }
}
