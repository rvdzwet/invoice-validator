using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Audit;

namespace BouwdepotInvoiceValidator.Services
{
    /// <summary>
    /// Service implementation for generating consolidated audit reports from validation results
    /// </summary>
    public class AuditReportService : IAuditReportService
    {
        private readonly ILogger<AuditReportService> _logger;
        private readonly IGeminiService _geminiService;
        
        public AuditReportService(
            ILogger<AuditReportService> logger,
            IGeminiService geminiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        }
        
        /// <inheritdoc />
        public async Task<ConsolidatedAuditReport> GenerateAuditReportAsync(ValidationResult validationResult)
        {
            _logger.LogInformation("Generating consolidated audit report for invoice: {InvoiceNumber}", 
                validationResult?.ExtractedInvoice?.InvoiceNumber ?? "Unknown");
            
            if (validationResult == null)
            {
                _logger.LogWarning("Cannot generate audit report: validation result is null");
                throw new ArgumentNullException(nameof(validationResult));
            }
            
            // Use Gemini AI to provide a comprehensive audit assessment if not already done
            if (validationResult.ExtractedInvoice != null)
            {
                try
                {
                    _logger.LogInformation("Getting audit-ready assessment from Gemini AI");
                    
                    if (string.IsNullOrEmpty(validationResult.AuditJustification))
                    {
                        var geminiResult = await _geminiService.GetAuditReadyAssessmentAsync(validationResult.ExtractedInvoice);
                        if (!string.IsNullOrEmpty(geminiResult.AuditJustification))
                        {
                            // Copy relevant fields from Gemini analysis
                            validationResult.AuditJustification = geminiResult.AuditJustification;
                            validationResult.CriteriaAssessments = geminiResult.CriteriaAssessments;
                            validationResult.WeightedScore = geminiResult.WeightedScore;
                            validationResult.RegulatoryNotes = geminiResult.RegulatoryNotes;
                        }
                    }
                    
                    // Perform multi-modal analysis if there are page images
                    if (validationResult.ExtractedInvoice.PageImages?.Count > 0 && 
                        (validationResult.VisualAssessments == null || validationResult.VisualAssessments.Count == 0))
                    {
                        _logger.LogInformation("Performing multi-modal analysis with Gemini AI");
                        var multiModalResult = await _geminiService.ValidateWithMultiModalAnalysisAsync(validationResult.ExtractedInvoice);
                        
                        // Update visual assessments if available
                        if (multiModalResult.VisualAssessments?.Count > 0)
                        {
                            validationResult.VisualAssessments = multiModalResult.VisualAssessments;
                        }
                    }
                    
                    _logger.LogInformation("Gemini AI analysis completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error using Gemini AI for analysis - using rule-based analysis instead");
                    // Continue with available data if Gemini fails
                }
            }
            
            var report = new ConsolidatedAuditReport
            {
                // Basic invoice data
                InvoiceNumber = validationResult.ExtractedInvoice?.InvoiceNumber ?? "Unknown",
                InvoiceDate = validationResult.ExtractedInvoice?.InvoiceDate ?? DateTime.Now,
                VendorName = validationResult.ExtractedInvoice?.VendorName ?? "Unknown Vendor",
                VendorAddress = validationResult.ExtractedInvoice?.VendorAddress ?? string.Empty,
                TotalAmount = validationResult.ExtractedInvoice?.TotalAmount ?? 0,
                
                // Bank details
                BankDetails = new VendorBankDetails 
                {
                    // Extract bank details from invoice if available (example implementation)
                    IBAN = ExtractIBANFromInvoice(validationResult.ExtractedInvoice),
                    BankName = ExtractBankNameFromInvoice(validationResult.ExtractedInvoice),
                    IsVerified = false // Default to false until verification process is implemented
                },
                
                // Overall assessment
                IsApproved = validationResult.IsBouwdepotCompliant,
                OverallScore = CalculateOverallScore(validationResult),
                SummaryText = GenerateSummaryText(validationResult),
                
                // Document integrity assessment
                DocumentValidation = new DocumentValidation
                {
                    IsValid = validationResult.IsValid && 
                              validationResult.IsVerifiedInvoice.GetValueOrDefault(),
                    Score = CalculateDocumentValidityScore(validationResult),
                    PrimaryReason = validationResult.IsValid ? "Document passes verification checks" : "Document fails verification",
                    ValidationDetails = ExtractDocumentValidityDetails(validationResult)
                },
                
                // Bouwdepot eligibility
                BouwdepotEligibility = new BouwdepotEligibility
                {
                    IsEligible = validationResult.IsBouwdepotCompliant,
                    Score = CalculateBouwdepotEligibilityScore(validationResult),
                    MeetsQualityImprovement = validationResult.BouwdepotValidation?.QualityImprovementRule ?? false,
                    MeetsPermanentAttachment = validationResult.BouwdepotValidation?.PermanentAttachmentRule ?? false,
                    ConstructionType = DetermineConstructionType(validationResult),
                    PaymentPriority = "Standard", // Default to standard
                    SpecialConditions = string.Empty // Default to none
                },
                
                // Audit information for legal compliance
                AuditInformation = new AuditTrail
                {
                    ValidationTimestamp = DateTime.UtcNow,
                    ValidationSystemVersion = "1.0.0",
                    ValidatedBy = "Automated System",
                    ValidatorRole = "AI Validator",
                    SystemIPAddress = Environment.MachineName,
                    OriginalDocumentHash = ComputeDocumentHash(validationResult.ExtractedInvoice),
                    ConclusionExplanation = validationResult.IsBouwdepotCompliant ? 
                        "Invoice meets all requirements for bouwdepot funding" : 
                        "Invoice does not meet required criteria for bouwdepot funding",
                    KeyFactors = GenerateKeyFactors(validationResult),
                    LegalBasis = GenerateLegalBasis(validationResult),
                    ProcessingSteps = GenerateProcessingSteps(validationResult)
                },
                
                // Technical details
                TechnicalDetails = new TechnicalDetails
                {
                    DetailedMetrics = GenerateDetailedMetrics(validationResult),
                    ProcessingNotes = GenerateProcessingNotes(validationResult)
                }
            };
            
            // Process line items
            ProcessLineItems(validationResult, report);
            
            // Generate detailed analysis sections
            GenerateDetailedAnalysis(validationResult, report);
            
            return report;
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Generates a summary text based on the validation result
        /// </summary>
        private string GenerateSummaryText(ValidationResult result)
        {
            if (result.IsBouwdepotCompliant)
            {
                return "This invoice contains valid home improvement items that qualify for construction fund (bouwdepot) payment. " +
                       "No action needed - proceed with funding request.";
            }
            
            var issues = new List<string>();
            
            if (!result.BouwdepotValidation.PermanentAttachmentRule)
            {
                issues.Add("items are not permanently attached to the house");
            }
            
            if (!result.BouwdepotValidation.QualityImprovementRule)
            {
                issues.Add("items do not improve home quality or value");
            }
            
            if (result.PossibleTampering)
            {
                issues.Add("possible document tampering detected");
            }
            
            if (!result.IsVerifiedInvoice.GetValueOrDefault())
            {
                issues.Add("document is not a valid invoice");
            }
            
            string issueText = issues.Any() 
                ? $"Issues: {string.Join(", ", issues)}."
                : "Please review the validation details for more information.";
            
            return $"This invoice does not meet the requirements for construction fund (bouwdepot) payment. {issueText}";
        }
        
        /// <summary>
        /// Calculates a document validity score based on the validation result
        /// </summary>
        private int CalculateDocumentValidityScore(ValidationResult result)
        {
            // Base score starts at 100
            int score = 100;
            
            // Deduct points for issues
            if (result.PossibleTampering)
            {
                score -= 50; // Major deduction for potential tampering
            }
            
            if (!result.IsVerifiedInvoice.GetValueOrDefault())
            {
                score -= 40; // Major deduction if not a valid invoice
            }
            
            if (result.MissingInvoiceElements?.Any() == true)
            {
                // Deduct points for each missing element, up to a maximum of 30
                score -= Math.Min(result.MissingInvoiceElements.Count * 5, 30);
            }
            
            // Use document verification confidence if available
            if (result.DocumentVerificationConfidence > 0)
            {
                // Weight the algorithmic confidence
                int confidenceScore = (int)(result.DocumentVerificationConfidence * 100);
                score = (score + confidenceScore) / 2;
            }
            
            // Error issues drastically reduce score
            int errorCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Error);
            if (errorCount > 0)
            {
                score -= Math.Min(errorCount * 15, 50);
            }
            
            // Warning issues moderately reduce score
            int warningCount = result.Issues.Count(i => i.Severity == ValidationSeverity.Warning);
            if (warningCount > 0)
            {
                score -= Math.Min(warningCount * 5, 25);
            }
            
            // Ensure score stays within 0-100 range
            return Math.Max(0, Math.Min(100, score));
        }
        
        /// <summary>
        /// Extracts document validity details from the validation result
        /// </summary>
        private List<string> ExtractDocumentValidityDetails(ValidationResult result)
        {
            var details = new List<string>();
            
            if (result.IsVerifiedInvoice.GetValueOrDefault())
            {
                details.Add("Verified invoice");
            }
            
            if (!result.PossibleTampering)
            {
                details.Add("No tampering detected");
            }
            
            if (result.PresentInvoiceElements?.Any() == true)
            {
                // Instead of listing all elements, give a summary
                details.Add("All required elements present");
            }
            
            if (result.IsValid)
            {
                details.Add("Passes basic validation checks");
            }
            
            return details;
        }
        
        /// <summary>
        /// Calculates a bouwdepot eligibility score based on the validation result
        /// </summary>
        private int CalculateBouwdepotEligibilityScore(ValidationResult result)
        {
            if (!result.BouwdepotValidation.QualityImprovementRule && 
                !result.BouwdepotValidation.PermanentAttachmentRule)
            {
                return 0; // Fails both rules
            }
            
            if (!result.BouwdepotValidation.QualityImprovementRule || 
                !result.BouwdepotValidation.PermanentAttachmentRule)
            {
                return 50; // Fails one rule
            }
            
            // Calculate what percentage of line items comply with both rules
            var lineItems = result.BouwdepotValidation.LineItemValidations;
            
            if (lineItems.Count == 0)
            {
                return result.IsBouwdepotCompliant ? 100 : 0;
            }
            
            int compliantItemCount = lineItems.Count(item => 
                item.IsPermanentlyAttached && item.ImproveHomeQuality);
                
            int score = (int)((double)compliantItemCount / lineItems.Count * 100);
            
            return score;
        }
        
        /// <summary>
        /// Determines the construction type based on the validation result
        /// </summary>
        private string DetermineConstructionType(ValidationResult result)
        {
            // Look for hints in purchase analysis
            if (result.PurchaseAnalysis?.Categories?.Any() == true)
            {
                var categories = result.PurchaseAnalysis.Categories;
                
                if (categories.Any(c => c.Contains("kitchen", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Renovation - Kitchen";
                }
                
                if (categories.Any(c => c.Contains("bathroom", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Renovation - Bathroom";
                }
                
                if (categories.Any(c => c.Contains("exterior", StringComparison.OrdinalIgnoreCase) || 
                                       c.Contains("roof", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("facade", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Renovation - Exterior";
                }
                
                if (categories.Any(c => c.Contains("floor", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("wall", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("ceiling", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Renovation - Interior";
                }
                
                if (categories.Any(c => c.Contains("installation", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("plumbing", StringComparison.OrdinalIgnoreCase) ||
                                       c.Contains("electrical", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Installation - Technical";
                }
            }
            
            // Default categorization
            return "Renovation - General";
        }
        
        /// <summary>
        /// Processes line items from the validation result into the report
        /// </summary>
        private void ProcessLineItems(ValidationResult result, ConsolidatedAuditReport report)
        {
            // Skip if no line items or invoice
            if (result.ExtractedInvoice?.LineItems == null || 
                result.ExtractedInvoice.LineItems.Count == 0 ||
                result.BouwdepotValidation.LineItemValidations.Count == 0)
            {
                return;
            }
            
            // Clear any existing line items
            report.LineItems.Clear();
            
            // Process each line item
            for (int i = 0; i < Math.Min(result.ExtractedInvoice.LineItems.Count, 
                                        result.BouwdepotValidation.LineItemValidations.Count); i++)
            {
                var invoiceItem = result.ExtractedInvoice.LineItems[i];
                var validationItem = result.BouwdepotValidation.LineItemValidations[i];
                
                var lineItem = new ValidatedLineItem
                {
                    Description = invoiceItem.Description,
                    Amount = invoiceItem.TotalPrice,
                    ValidationNote = validationItem.ValidationNotes,
                    IsApproved = validationItem.IsPermanentlyAttached && validationItem.ImproveHomeQuality
                };
                
                // If validation notes are empty, add a generic reason when not approved
                if (!lineItem.IsApproved && string.IsNullOrEmpty(lineItem.ValidationNote))
                {
                    if (!validationItem.IsPermanentlyAttached && !validationItem.ImproveHomeQuality)
                    {
                        lineItem.ValidationNote = "Not permanently attached and does not improve home quality";
                    }
                    else if (!validationItem.IsPermanentlyAttached)
                    {
                        lineItem.ValidationNote = "Not permanently attached to the house";
                    }
                    else if (!validationItem.ImproveHomeQuality)
                    {
                        lineItem.ValidationNote = "Does not improve home quality or value";
                    }
                    else
                    {
                        lineItem.ValidationNote = "Purpose unclear";
                    }
                }
                
                // Add to the consolidated list of line items
                report.LineItems.Add(lineItem);
            }
        }
        
        /// <summary>
        /// Generates detailed analysis sections from the validation result
        /// </summary>
        private void GenerateDetailedAnalysis(ValidationResult result, ConsolidatedAuditReport report)
        {
            // Document verification section
            var documentSection = new DetailedAnalysisSection
            {
                SectionName = "DOCUMENT VERIFICATION"
            };
            
            documentSection.AnalysisItems.Add("Document Type", 
                result.IsVerifiedInvoice.GetValueOrDefault() ? 
                "Invoice" : (result.DetectedDocumentType ?? "Unknown Document Type"));
            
            documentSection.AnalysisItems.Add("Missing Elements", 
                result.MissingInvoiceElements?.Any() == true ? 
                string.Join(", ", result.MissingInvoiceElements) : "None");
            
            if (result.PossibleTampering)
            {
                documentSection.AnalysisItems.Add("Warning", "Possible document tampering detected");
            }
            else if (result.Issues.Any(i => i.Severity == ValidationSeverity.Warning))
            {
                var warning = result.Issues.FirstOrDefault(i => i.Severity == ValidationSeverity.Warning);
                documentSection.AnalysisItems.Add("Warning", warning?.Message ?? "Non-standard invoice format (minor issue)");
            }
            
            documentSection.AnalysisItems.Add("Fraud Indicators", 
                result.FraudIndicatorAssessments?.Any() == true ? 
                "Detected - see details below" : "None detected");
            
            report.DetailedAnalysis.Add(documentSection);
            
            // Bouwdepot rules validation section
            var bouwdepotSection = new DetailedAnalysisSection
            {
                SectionName = "BOUWDEPOT RULES VALIDATION"
            };
            
            bouwdepotSection.AnalysisItems.Add("Quality Improvement", 
                result.BouwdepotValidation.QualityImprovementRule ? 
                "All items will increase home value" : "Some items do not improve home value");
            
            bouwdepotSection.AnalysisItems.Add("Permanent Attachment", 
                result.BouwdepotValidation.PermanentAttachmentRule ? 
                "All key items are permanently installed" : "Some items are not permanently attached");
                
            // Get keywords from line item descriptions
            var keywords = ExtractKeywordsFromLineItems(result);
            if (keywords.Any())
            {
                bouwdepotSection.AnalysisItems.Add("Keywords Detected", 
                    string.Join(", ", keywords.Take(5)));
            }
            
            report.DetailedAnalysis.Add(bouwdepotSection);
            
            // Line item analysis section
            if (result.ExtractedInvoice?.LineItems != null && 
                result.BouwdepotValidation.LineItemValidations.Count > 0)
            {
                for (int i = 0; i < Math.Min(result.ExtractedInvoice.LineItems.Count, 
                                          result.BouwdepotValidation.LineItemValidations.Count); i++)
                {
                    var invoiceItem = result.ExtractedInvoice.LineItems[i];
                    var validationItem = result.BouwdepotValidation.LineItemValidations[i];
                    
                    var lineItemSection = new DetailedAnalysisSection
                    {
                        SectionName = $"{i + 1}. \"{invoiceItem.Description}\" (€{invoiceItem.TotalPrice:F2})"
                    };
                    
                    lineItemSection.AnalysisItems.Add("Permanently attached", 
                        validationItem.IsPermanentlyAttached ? "Yes" : "No");
                    
                    lineItemSection.AnalysisItems.Add("Quality improvement", 
                        validationItem.ImproveHomeQuality ? "Yes" : "No");
                    
                    // Determine construction category
                    string category = "General";
                    
                    if (i < result.PurchaseAnalysis?.LineItemDetails?.Count)
                    {
                        category = result.PurchaseAnalysis.LineItemDetails[i].Category;
                    }
                    else if (!string.IsNullOrEmpty(validationItem.VerduurzamingsdepotCategory))
                    {
                        category = validationItem.VerduurzamingsdepotCategory;
                    }
                    
                    if (string.IsNullOrEmpty(category))
                    {
                        category = DetermineCategoryFromDescription(invoiceItem.Description);
                    }
                    
                    lineItemSection.AnalysisItems.Add("Construction category", category);
                    
                    lineItemSection.AnalysisItems.Add("Recommendation", 
                        validationItem.IsPermanentlyAttached && validationItem.ImproveHomeQuality ? 
                        "Approve" : (!string.IsNullOrEmpty(validationItem.ValidationNotes) ? 
                                   validationItem.ValidationNotes : "Request clarification"));
                    
                    report.DetailedAnalysis.Add(lineItemSection);
                }
            }
        }
        
        /// <summary>
        /// Extracts relevant keywords from line item descriptions
        /// </summary>
        private List<string> ExtractKeywordsFromLineItems(ValidationResult result)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Construction keywords to check for
            var constructionKeywords = new List<string>
            {
                "installation", "renovation", "construction", "building", "remodel",
                "kitchen", "bathroom", "flooring", "tiling", "plumbing", "electric",
                "roof", "wall", "window", "door", "insulation", "heating"
            };
            
            // Check line items
            if (result.ExtractedInvoice?.LineItems != null)
            {
                foreach (var item in result.ExtractedInvoice.LineItems)
                {
                    foreach (var keyword in constructionKeywords)
                    {
                        if (item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            keywords.Add($"\"{keyword}\"");
                        }
                    }
                }
            }
            
            return keywords.ToList();
        }
        
        /// <summary>
        /// Determines a construction category based on item description
        /// </summary>
        private string DetermineCategoryFromDescription(string description)
        {
            description = description.ToLowerInvariant();
            
            if (description.Contains("kitchen") || description.Contains("cabinet") || 
                description.Contains("countertop") || description.Contains("sink"))
            {
                return "Interior renovation - Kitchen";
            }
            
            if (description.Contains("bathroom") || description.Contains("shower") || 
                description.Contains("toilet") || description.Contains("bath"))
            {
                return "Interior renovation - Bathroom";
            }
            
            if (description.Contains("floor") || description.Contains("tile") || 
                description.Contains("carpet") || description.Contains("laminate"))
            {
                return "Interior renovation - Flooring";
            }
            
            if (description.Contains("wall") || description.Contains("paint") || 
                description.Contains("plaster") || description.Contains("drywall"))
            {
                return "Interior renovation - Walls";
            }
            
            if (description.Contains("roof") || description.Contains("gutter") || 
                description.Contains("chimney") || description.Contains("skylight"))
            {
                return "Exterior renovation";
            }
            
            if (description.Contains("window") || description.Contains("door") || 
                description.Contains("frame") || description.Contains("glass"))
            {
                return "Windows and doors";
            }
            
            if (description.Contains("plumbing") || description.Contains("electrical") || 
                description.Contains("wiring") || description.Contains("pipe"))
            {
                return "Technical installation";
            }
            
            if (description.Contains("hardware") || description.Contains("material") || 
                description.Contains("tool") || description.Contains("supply"))
            {
                return "Supporting materials";
            }
            
            return "General construction";
        }
        /// <summary>
        /// Calculates the overall validation score based on multiple factors
        /// </summary>
        private int CalculateOverallScore(ValidationResult result)
        {
            int documentScore = CalculateDocumentValidityScore(result);
            int eligibilityScore = CalculateBouwdepotEligibilityScore(result);
            
            // Weighted average: document validity is 40%, eligibility is 60%
            int overallScore = (int)(documentScore * 0.4 + eligibilityScore * 0.6);
            
            // Adjust for AI confidence if available
            if (result.WeightedScore > 0)
            {
                // Blend with AI score
                overallScore = (overallScore + (int)result.WeightedScore) / 2;
            }
            
            // Ensure score stays within 0-100 range
            return Math.Max(0, Math.Min(100, overallScore));
        }
        
        /// <summary>
        /// Extracts IBAN from invoice if available
        /// </summary>
        private string ExtractIBANFromInvoice(Invoice invoice)
        {
            // Implementation would search for IBAN in invoice text using regex
            // This is a placeholder implementation
            string rawText = invoice?.RawText ?? string.Empty;
            
            // Simple regex pattern for IBAN (not comprehensive)
            var ibanPattern = @"(?:IBAN|Account)\s*:?\s*([A-Z]{2}\d{2}(?:\s*[A-Z0-9]){10,30})";
            var match = System.Text.RegularExpressions.Regex.Match(rawText, ibanPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Extracts bank name from invoice if available
        /// </summary>
        private string ExtractBankNameFromInvoice(Invoice invoice)
        {
            // Implementation would search for bank name in invoice text
            // This is a placeholder implementation
            string rawText = invoice?.RawText ?? string.Empty;
            
            // Common Dutch banks
            string[] bankNames = {"ING", "ABN AMRO", "Rabobank", "SNS", "Triodos", "Knab", "ASN Bank"};
            
            foreach (var bank in bankNames)
            {
                if (rawText.Contains(bank, StringComparison.OrdinalIgnoreCase))
                {
                    return bank;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Computes a hash of the invoice document for audit purposes
        /// </summary>
        private string ComputeDocumentHash(Invoice invoice)
        {
            if (invoice == null)
            {
                return string.Empty;
            }
            
            // In a real implementation, this would hash the original PDF binary data
            // This is a simplified version that creates a hash of the invoice data
            var hashData = $"{invoice.InvoiceNumber}|{invoice.InvoiceDate}|{invoice.VendorName}|{invoice.TotalAmount}";
            
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashData));
                return Convert.ToBase64String(hashBytes);
            }
        }
        
        /// <summary>
        /// Generates key factors that influenced the validation decision
        /// </summary>
        private List<string> GenerateKeyFactors(ValidationResult result)
        {
            var factors = new List<string>();
            
            // Document validation factors
            if (result.IsVerifiedInvoice.GetValueOrDefault())
            {
                factors.Add("Document verified as authentic invoice");
            }
            else if (!string.IsNullOrEmpty(result.DetectedDocumentType))
            {
                factors.Add($"Document detected as: {result.DetectedDocumentType}");
            }
            
            if (result.PossibleTampering)
            {
                factors.Add("Signs of document tampering detected");
            }
            
            // Bouwdepot rule factors
            if (result.BouwdepotValidation != null)
            {
                if (result.BouwdepotValidation.QualityImprovementRule)
                {
                    factors.Add("Items improve home quality and value");
                }
                else
                {
                    factors.Add("Items do not sufficiently improve home quality");
                }
                
                if (result.BouwdepotValidation.PermanentAttachmentRule)
                {
                    factors.Add("Items are permanently attached to the house");
                }
                else
                {
                    factors.Add("Items are not permanently attached to the house");
                }
            }
            
            // Add factors based on item analysis
            if (result.PurchaseAnalysis?.HomeImprovementPercentage > 0)
            {
                int percentage = (int)(result.PurchaseAnalysis.HomeImprovementPercentage * 100);
                factors.Add($"{percentage}% of invoice items related to home improvement");
            }
            
            return factors;
        }
        
        /// <summary>
        /// Generates legal basis references for the validation decision
        /// </summary>
        private List<LegalReference> GenerateLegalBasis(ValidationResult result)
        {
            var legalBases = new List<LegalReference>();
            
            // Add core bouwdepot regulations
            legalBases.Add(new LegalReference
            {
                ReferenceCode = "Dutch Civil Code 7:765",
                Description = "Provisions governing construction deposits (bouwdepot)",
                Applicability = "Legal basis for determining construction expense eligibility"
            });
            
            // Add mortgage terms reference
            legalBases.Add(new LegalReference
            {
                ReferenceCode = "Mortgage Terms §3.1.2",
                Description = "Permanent attachment rule for home improvements",
                Applicability = "Contractual basis for determining if expenses qualify for payment"
            });
            
            // Add quality improvement reference
            legalBases.Add(new LegalReference
            {
                ReferenceCode = "Mortgage Terms §3.1.1",
                Description = "Quality improvement rule for home renovations",
                Applicability = "Contractual basis requiring renovations to improve home value"
            });
            
            // Add document authenticity reference
            legalBases.Add(new LegalReference
            {
                ReferenceCode = "Dutch VAT Act Article 35a",
                Description = "Requirements for valid invoices",
                Applicability = "Legal standard for verifying invoice authenticity"
            });
            
            return legalBases;
        }
        
        /// <summary>
        /// Generates processing steps for the audit trail
        /// </summary>
        private List<ProcessingStep> GenerateProcessingSteps(ValidationResult result)
        {
            var steps = new List<ProcessingStep>();
            
            // Document receipt step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-60),
                ProcessName = "DocumentReceipt",
                ProcessorIdentifier = "InvoiceUploadComponent",
                OutputState = "PDF document received for processing"
            });
            
            // Document validation step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-50),
                ProcessName = "DocumentValidation",
                ProcessorIdentifier = "PdfExtractionService",
                OutputState = $"Document validation complete. IsValid: {result.IsValid}"
            });
            
            // Text extraction step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-40),
                ProcessName = "TextExtraction",
                ProcessorIdentifier = "PdfExtractionService",
                OutputState = "Text content extracted from PDF"
            });
            
            // AI analysis step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-30),
                ProcessName = "AiAnalysis",
                ProcessorIdentifier = "GeminiService",
                OutputState = $"AI analysis complete. Score: {result.WeightedScore}"
            });
            
            // Bouwdepot rules validation step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-20),
                ProcessName = "BouwdepotRulesValidation",
                ProcessorIdentifier = "BouwdepotRulesValidationService",
                OutputState = $"Bouwdepot rules validation complete. IsCompliant: {result.IsBouwdepotCompliant}"
            });
            
            // Audit generation step
            steps.Add(new ProcessingStep
            {
                Timestamp = DateTime.UtcNow,
                ProcessName = "AuditGeneration",
                ProcessorIdentifier = "AuditReportService",
                OutputState = "Legal audit report generated"
            });
            
            return steps;
        }
        
        /// <summary>
        /// Generates detailed metrics for technical analysis
        /// </summary>
        private Dictionary<string, string> GenerateDetailedMetrics(ValidationResult result)
        {
            var metrics = new Dictionary<string, string>();
            
            // Document metrics
            metrics.Add("Document Type Confidence", $"{result.DocumentVerificationConfidence * 100:F1}%");
            
            if (result.MissingInvoiceElements?.Any() == true)
            {
                metrics.Add("Missing Elements Count", result.MissingInvoiceElements.Count.ToString());
            }
            
            if (result.PresentInvoiceElements?.Any() == true)
            {
                metrics.Add("Present Elements Count", result.PresentInvoiceElements.Count.ToString());
            }
            
            // AI analysis metrics
            if (result.WeightedScore > 0)
            {
                metrics.Add("AI Confidence Score", $"{result.WeightedScore:F1}");
            }
            
            if (result.CriteriaAssessments?.Any() == true)
            {
                metrics.Add("Criteria Assessment Count", result.CriteriaAssessments.Count.ToString());
                
                // Average confidence across all criteria
                double avgConfidence = result.CriteriaAssessments.Average(c => c.Confidence);
                metrics.Add("Average Criteria Confidence", $"{avgConfidence:P1}");
            }
            
            // Line item metrics
            if (result.BouwdepotValidation?.LineItemValidations?.Any() == true)
            {
                var itemValidations = result.BouwdepotValidation.LineItemValidations;
                int permanentCount = itemValidations.Count(i => i.IsPermanentlyAttached);
                int qualityCount = itemValidations.Count(i => i.ImproveHomeQuality);
                
                metrics.Add("Items Permanently Attached", $"{permanentCount}/{itemValidations.Count}");
                metrics.Add("Items Improving Quality", $"{qualityCount}/{itemValidations.Count}");
            }
            
            return metrics;
        }
        
        /// <summary>
        /// Generates processing notes for technical analysis
        /// </summary>
        private List<string> GenerateProcessingNotes(ValidationResult result)
        {
            var notes = new List<string>();
            
            // Add processing info
            notes.Add($"Validation performed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            notes.Add($"Document analysis engine: Gemini AI");
            
            // Add document analysis notes
            if (result.IsVerifiedInvoice.GetValueOrDefault())
            {
                notes.Add("Document verification: Passed authentication checks");
            }
            else
            {
                notes.Add("Document verification: Failed to verify as authentic invoice");
            }
            
            // Add warning notes
            foreach (var issue in result.Issues.Where(i => i.Severity == ValidationSeverity.Warning))
            {
                notes.Add($"Warning: {issue.Message}");
            }
            
            // Add error notes
            foreach (var issue in result.Issues.Where(i => i.Severity == ValidationSeverity.Error))
            {
                notes.Add($"Error: {issue.Message}");
            }
            
            // Add performance notes
            if (result.ExtractedInvoice?.PageImages?.Any() == true)
            {
                notes.Add($"Document pages processed: {result.ExtractedInvoice.PageImages.Count}");
            }
            
            return notes;
        }
        
        #endregion
    }
}
