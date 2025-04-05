using BouwdepotInvoiceValidator.Models;
using BouwdepotInvoiceValidator.Models.Enhanced; // Add this using statement

namespace BouwdepotInvoiceValidator.Services.Vendors
{
    /// <summary>
    /// Service for vendor profiling and analysis
    /// </summary>
    public class VendorProfileService : IVendorProfileService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<VendorProfileService> _logger;
        
        public VendorProfileService(
            IVendorRepository vendorRepository, 
            ILogger<VendorProfileService> logger)
        {
            _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation("Vendor profile service initialized");
        }
        
        /// <summary>
        /// Gets a vendor profile based on an invoice
        /// </summary>
        public async Task<VendorProfile> GetVendorProfileAsync(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }
            
            _logger.LogInformation("Looking up vendor profile for: {VendorName}, KvK: {KvkNumber}", 
                invoice.VendorName, invoice.VendorKvkNumber);
                
            VendorProfile profile = null;
            
            // Try to find by KVK/VAT number first (most reliable)
            if (!string.IsNullOrEmpty(invoice.VendorKvkNumber) || !string.IsNullOrEmpty(invoice.VendorBtwNumber))
            {
                _logger.LogDebug("Attempting to find vendor by tax IDs: KvK: {KvkNumber}, VAT: {VatNumber}", 
                    invoice.VendorKvkNumber, invoice.VendorBtwNumber);
                    
                profile = await _vendorRepository.GetVendorByTaxIdAsync(invoice.VendorKvkNumber, invoice.VendorBtwNumber);
                
                if (profile != null)
                {
                    _logger.LogInformation("Found vendor profile by tax ID: {VendorName}", profile.VendorName);
                }
            }
            
            // If not found, try by name
            if (profile == null && !string.IsNullOrEmpty(invoice.VendorName))
            {
                _logger.LogDebug("Attempting to find vendor by name: {VendorName}", invoice.VendorName);
                
                profile = await _vendorRepository.GetVendorByNameAsync(invoice.VendorName);
                
                if (profile != null)
                {
                    _logger.LogInformation("Found vendor profile by name: {VendorName}", profile.VendorName);
                }
            }
            
            // If still not found, create a new profile
            if (profile == null)
            {
                _logger.LogInformation("Creating new vendor profile for: {VendorName}", invoice.VendorName);
                
                profile = new VendorProfile
                {
                    VendorName = invoice.VendorName,
                    NormalizedName = VendorProfile.NormalizeVendorName(invoice.VendorName),
                    KvkNumber = invoice.VendorKvkNumber,
                    VatNumber = invoice.VendorBtwNumber,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };
                
                if (!string.IsNullOrEmpty(invoice.VendorAddress))
                {
                    profile.KnownAddresses.Add(invoice.VendorAddress);
                }
                
                // Initialize with default trust metrics
                profile.TrustMetrics = new VendorTrustMetrics
                {
                    ReliabilityScore = 0.5,
                    ConsistencyScore = 0.5,
                    PriceStabilityScore = 0.5,
                    DocumentQualityScore = 0.5
                };
                
                // Add payment details if present
                if (invoice.PaymentDetails != null && !string.IsNullOrEmpty(invoice.PaymentDetails.IBAN))
                {
                    profile.PaymentDetails.Add(invoice.PaymentDetails);
                }
            }
            
            return profile;
        }
        
        /// <summary>
        /// Updates a vendor profile with new invoice data
        /// </summary>
        public async Task<VendorProfile> UpdateVendorProfileAsync(Invoice invoice, ValidationResult validationResult)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }
            
            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }
            
            // Get current profile (or create new one)
            var profile = await GetVendorProfileAsync(invoice);
            
            _logger.LogInformation("Updating vendor profile for: {VendorName}, ID: {VendorId}",
                profile.VendorName, profile.Id);
                
            // Update basic stats
            profile.InvoiceCount++;
            profile.LastSeen = DateTime.UtcNow;
            profile.LastUpdated = DateTime.UtcNow;
            
            // Update address if new
            if (!string.IsNullOrEmpty(invoice.VendorAddress) && 
                !profile.KnownAddresses.Contains(invoice.VendorAddress))
            {
                profile.KnownAddresses.Add(invoice.VendorAddress);
            }
            
            // Update payment details if new
            if (invoice.PaymentDetails != null && !string.IsNullOrEmpty(invoice.PaymentDetails.IBAN))
            {
                bool hasMatchingPaymentDetails = profile.PaymentDetails.Any(p => 
                    p.IBAN == invoice.PaymentDetails.IBAN);
                    
                if (!hasMatchingPaymentDetails)
                {
                    _logger.LogInformation("Adding new payment details for vendor {VendorName}: {IBAN}", 
                        profile.VendorName, invoice.PaymentDetails.MaskedIBAN);
                        
                    profile.PaymentDetails.Add(invoice.PaymentDetails);
                }
            }
            
            // Update service patterns
            await UpdateServicePatternsAsync(profile, invoice);
            
            // Update price ranges
            await UpdatePriceRangesAsync(profile, invoice);
            
            // Update business categories based on validation result
            await UpdateBusinessCategoriesAsync(profile, invoice, validationResult);
            
            // Update trust metrics
            await UpdateTrustMetricsAsync(profile, invoice, validationResult);
            
            // Save updated profile
            await _vendorRepository.UpsertVendorAsync(profile);
            
            _logger.LogInformation("Vendor profile updated: {VendorName}, InvoiceCount: {InvoiceCount}",
                profile.VendorName, profile.InvoiceCount);
                
            return profile;
        }
        
        /// <summary>
        /// Analyzes a vendor's trustworthiness using historical data
        /// </summary>
        public async Task<VendorTrustAnalysisResult> AnalyzeVendorTrustworthinessAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing vendor trustworthiness: {VendorName}", invoice.VendorName);
            
            // Get the vendor profile
            var profile = await GetVendorProfileAsync(invoice);
            
            var result = new VendorTrustAnalysisResult
            {
                InvoiceCount = profile.InvoiceCount
            };
            
            // For new vendors, we have limited trust data
            if (profile.InvoiceCount <= 1)
            {
                _logger.LogInformation("Limited trust data for new vendor: {VendorName}", profile.VendorName);
                
                result.OverallTrustScore = 0.5;
                result.TrustScores["reliability"] = 0.5;
                result.TrustScores["consistency"] = 0.5;
                result.TrustScores["price_stability"] = 0.5;
                result.TrustScores["document_quality"] = 0.5;
                
                result.TrustFactors.Add("New vendor with limited history");
                
                return result;
            }
            
            // Copy the trust metrics from the profile
            result.TrustScores["reliability"] = profile.TrustMetrics.ReliabilityScore;
            result.TrustScores["consistency"] = profile.TrustMetrics.ConsistencyScore;
            result.TrustScores["price_stability"] = profile.TrustMetrics.PriceStabilityScore;
            result.TrustScores["document_quality"] = profile.TrustMetrics.DocumentQualityScore;
            
            // Calculate overall trust score (weighted average)
            result.OverallTrustScore = (
                (profile.TrustMetrics.ReliabilityScore * 0.4) +
                (profile.TrustMetrics.ConsistencyScore * 0.2) +
                (profile.TrustMetrics.PriceStabilityScore * 0.2) +
                (profile.TrustMetrics.DocumentQualityScore * 0.2)
            );
            
            // Add trust factors
            if (profile.TrustMetrics.ReliabilityScore >= 0.8)
            {
                result.TrustFactors.Add("High reliability score based on historical invoices");
            }
            
            if (profile.TrustMetrics.PriceStabilityScore >= 0.8)
            {
                result.TrustFactors.Add("Consistent pricing across historical invoices");
            }
            
            if (profile.TrustMetrics.DocumentQualityScore >= 0.8)
            {
                result.TrustFactors.Add("High quality invoice documentation");
            }
            
            if (profile.InvoiceCount > 10)
            {
                result.TrustFactors.Add($"Established vendor with {profile.InvoiceCount} previous invoices");
            }
            
            // Add concern factors
            if (profile.TrustMetrics.ReliabilityScore < 0.4)
            {
                result.ConcernFactors.Add("Low reliability score based on historical invoices");
            }
            
            if (profile.TrustMetrics.PriceStabilityScore < 0.4)
            {
                result.ConcernFactors.Add("Inconsistent pricing across historical invoices");
            }
            
            // Check for unresolved anomalies
            var unresolvedAnomalies = profile.TrustMetrics.DetectedAnomalies
                .Where(a => !a.Resolved)
                .ToList();
                
            if (unresolvedAnomalies.Any())
            {
                result.ConcernFactors.Add($"{unresolvedAnomalies.Count} unresolved anomalies detected in previous invoices");
                
                // List the most severe anomalies
                foreach (var anomaly in unresolvedAnomalies.OrderByDescending(a => a.Severity).Take(2))
                {
                    result.ConcernFactors.Add($"Unresolved anomaly: {anomaly.Description}");
                }
            }
            
            _logger.LogInformation("Vendor trust analysis complete for {VendorName}: " +
                "Overall score: {OverallScore}, Trust factors: {TrustFactorCount}, Concern factors: {ConcernFactorCount}",
                profile.VendorName, result.OverallTrustScore, result.TrustFactors.Count, result.ConcernFactors.Count);
                
            return result;
        }
        
        /// <summary>
        /// Analyzes whether prices in the invoice are reasonable based on vendor history
        /// </summary>
        public async Task<PriceAnalysisResult> AnalyzePriceReasonablenessAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing price reasonableness for invoice: {VendorName}", invoice.VendorName);
            
            var result = new PriceAnalysisResult();
            
            // Get the vendor profile
            var profile = await GetVendorProfileAsync(invoice);
            
            // For new vendors, we have no price history
            if (profile.InvoiceCount <= 1 || profile.PriceRanges.Count == 0)
            {
                _logger.LogInformation("Limited price history for vendor: {VendorName}", profile.VendorName);
                
                // Get industry averages instead
                var industryPriceRanges = await _vendorRepository.GetIndustryPriceRangesAsync();
                
                // Compare against industry averages
                await AnalyzePricesAgainstIndustryAsync(invoice, industryPriceRanges, result);
                
                return result;
            }
            
            // Check each line item against vendor's price history
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                // Calculate unit price
                decimal unitPrice = lineItem.Quantity > 0 
                    ? lineItem.TotalPrice / lineItem.Quantity 
                    : lineItem.TotalPrice;
                
                // Try to find matching item category in price history
                PriceRange matchingRange = null;
                
                // First try exact match
                if (profile.PriceRanges.TryGetValue(lineItem.Description, out var exactRange))
                {
                    matchingRange = exactRange;
                }
                else
                {
                    // Try fuzzy matching
                    foreach (var range in profile.PriceRanges)
                    {
                        if (range.Key.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                            lineItem.Description.Contains(range.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingRange = range.Value;
                            break;
                        }
                    }
                }
                
                // If we found a matching range, check if the price is reasonable
                if (matchingRange != null && matchingRange.SampleSize > 0)
                {
                    // Calculate permitted price ranges (with 30% buffer)
                    decimal minAcceptable = matchingRange.MinPrice * 0.7m;
                    decimal maxAcceptable = matchingRange.MaxPrice * 1.3m;
                    
                    if (unitPrice < minAcceptable || unitPrice > maxAcceptable)
                    {
                        // Calculate deviation percentage
                        double deviationPercentage = 0;
                        
                        if (unitPrice < minAcceptable && matchingRange.MinPrice > 0)
                        {
                            deviationPercentage = (double)((matchingRange.MinPrice - unitPrice) / matchingRange.MinPrice);
                        }
                        else if (unitPrice > maxAcceptable && matchingRange.MaxPrice > 0)
                        {
                            deviationPercentage = (double)((unitPrice - matchingRange.MaxPrice) / matchingRange.MaxPrice);
                        }
                        
                        // Add to unreasonably priced items
                        result.UnreasonablyPricedItems.Add(new UnreasonablyPricedItem
                        {
                            Description = lineItem.Description,
                            TotalPrice = lineItem.TotalPrice,
                            ExpectedMinPrice = matchingRange.MinPrice,
                            ExpectedMaxPrice = matchingRange.MaxPrice,
                            DeviationPercentage = deviationPercentage
                        });
                        
                        // Update maximum deviation
                        if (deviationPercentage > result.MaxPriceDeviation)
                        {
                            result.MaxPriceDeviation = deviationPercentage;
                        }
                        
                        // Contribute to average deviation
                        result.AveragePriceDeviation += deviationPercentage;
                    }
                }
            }
            
            // Calculate average deviation
            if (result.UnreasonablyPricedItems.Count > 0)
            {
                result.AveragePriceDeviation /= result.UnreasonablyPricedItems.Count;
            }
            
            // Set flag if any unreasonable prices were detected
            result.UnreasonablePricesDetected = result.UnreasonablyPricedItems.Count > 0;
            
            _logger.LogInformation("Price analysis complete for {VendorName}: " +
                "Unreasonable items: {UnreasonableItemCount}, Max deviation: {MaxDeviation:P0}",
                profile.VendorName, result.UnreasonablyPricedItems.Count, result.MaxPriceDeviation);
                
            return result;
        }
        
        /// <summary>
        /// Analyzes whether services in the invoice match the vendor's usual patterns
        /// </summary>
        public async Task<ServiceAnalysisResult> AnalyzeServicePatternsAsync(Invoice invoice)
        {
            _logger.LogInformation("Analyzing service patterns for invoice: {VendorName}", invoice.VendorName);
            
            var result = new ServiceAnalysisResult();
            
            // Get the vendor profile
            var profile = await GetVendorProfileAsync(invoice);
            
            // For new vendors, we have no service pattern history
            if (profile.InvoiceCount <= 1 || profile.CommonServices.Count == 0)
            {
                _logger.LogInformation("Limited service pattern history for vendor: {VendorName}", profile.VendorName);
                
                // No historical comparison possible
                result.ServicePatternScore = 0.5; // Neutral score
                
                // If we have business categories, check if the services match the categories
                if (profile.BusinessCategories.Count > 0)
                {
                    // Get common services in these categories
                    var commonServices = new List<string>();
                    
                    foreach (var category in profile.BusinessCategories)
                    {
                        var categoryServices = await _vendorRepository.GetCommonServicesInCategoryAsync(category);
                        commonServices.AddRange(categoryServices.Select(s => s.ServiceName));
                    }
                    
                    result.CommonServices = commonServices.Distinct().ToList();
                    
                    if (result.CommonServices.Count > 0)
                    {
                        AnalyzeServicesAgainstCommonList(invoice, result.CommonServices, result);
                    }
                }
                
                return result;
            }
            
            // Extract common services from profile
            result.CommonServices = profile.CommonServices.Values
                .OrderByDescending(s => s.Frequency)
                .Select(s => s.ServiceName)
                .ToList();
                
            // Match each line item against common services
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                bool matchFound = false;
                
                // Check for exact or fuzzy match in common services
                foreach (var service in profile.CommonServices.Values)
                {
                    if (service.ServiceName.Equals(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        service.ServiceName.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        lineItem.Description.Contains(service.ServiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        matchFound = true;
                        result.MatchingServices.Add(lineItem.Description);
                        break;
                    }
                }
                
                // If no match found, it's an unusual service
                if (!matchFound)
                {
                    result.UnusualServices.Add(lineItem.Description);
                }
            }
            
            // Set the flag for unusual services
            result.UnusualServicesDetected = result.UnusualServices.Count > 0;
            
            // Calculate the service pattern score
            if (invoice.LineItems.Count > 0)
            {
                result.ServicePatternScore = (double)result.MatchingServices.Count / invoice.LineItems.Count;
            }
            else
            {
                result.ServicePatternScore = 0.5; // Neutral if no line items
            }
            
            _logger.LogInformation("Service pattern analysis complete for {VendorName}: " +
                "Score: {Score:P0}, Matching: {MatchingCount}, Unusual: {UnusualCount}",
                profile.VendorName, result.ServicePatternScore, result.MatchingServices.Count, result.UnusualServices.Count);
                
            return result;
        }
        
        /// <summary>
        /// Detects anomalies in the invoice compared to the vendor's profile
        /// </summary>
        public async Task<List<AnomalyRecord>> DetectVendorAnomaliesAsync(Invoice invoice, VendorProfile profile)
        {
            _logger.LogInformation("Detecting vendor anomalies for invoice: {VendorName}", invoice.VendorName);
            
            var anomalies = new List<AnomalyRecord>();
            
            // For new vendors, we have limited anomaly detection
            if (profile.InvoiceCount <= 1)
            {
                _logger.LogInformation("Limited anomaly detection for new vendor: {VendorName}", profile.VendorName);
                
                // Check for basic anomalies even for new vendors
                await DetectBasicAnomaliesAsync(invoice, profile, anomalies);
                
                return anomalies;
            }
            
            // Check for payment details anomalies
            await DetectPaymentAnomaliesAsync(invoice, profile, anomalies);
            
            // Check for price anomalies
            await DetectPriceAnomaliesAsync(invoice, profile, anomalies);
            
            // Check for service pattern anomalies
            await DetectServiceAnomaliesAsync(invoice, profile, anomalies);
            
            // Check for basic anomalies
            await DetectBasicAnomaliesAsync(invoice, profile, anomalies);
            
            _logger.LogInformation("Vendor anomaly detection complete for {VendorName}: {AnomalyCount} anomalies found",
                profile.VendorName, anomalies.Count);
                
            return anomalies;
        }
        
        /// <summary>
        /// Gets insights about a vendor for display/reporting
        /// </summary>
        public async Task<VendorInsights> GetVendorInsightsAsync(string vendorId)
        {
            var profile = await _vendorRepository.GetVendorAsync(vendorId);
            
            if (profile == null)
            {
                return null;
            }
            
            
            
            // Ensure VendorInsights class exists and has these properties
            // Assuming VendorInsights has properties matching VendorProfile's structure
            var insights = new VendorInsights
            {
                VendorName = profile.VendorName,
                BusinessCategories = profile.BusinessCategories, // Correct: Directly on VendorProfile
                InvoiceCount = profile.InvoiceCount,             // Correct: Directly on VendorProfile
                FirstSeen = profile.FirstSeen,                   // Correct: Directly on VendorProfile
                LastSeen = profile.LastSeen,                     // Correct: Directly on VendorProfile
                ReliabilityScore = profile.TrustMetrics.ReliabilityScore, // Access via TrustMetrics
                PriceStabilityScore = profile.TrustMetrics.PriceStabilityScore, // Access via TrustMetrics
                DocumentQualityScore = profile.TrustMetrics.DocumentQualityScore, // Access via TrustMetrics
                TotalAnomalyCount = profile.TrustMetrics.DetectedAnomalies.Count, // Calculate from TrustMetrics list
                VendorSpecialties = profile.SpecialtyServices // Correct: Use SpecialtyServices from VendorProfile
            };
            
            return insights;
        }
        
        /// <summary>
        /// Gets a count of known vendors in the system
        /// </summary>
        public Task<int> GetVendorCountAsync()
        {
            return _vendorRepository.GetVendorCountAsync();
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Updates the service patterns for a vendor profile
        /// </summary>
        private async Task UpdateServicePatternsAsync(VendorProfile profile, Invoice invoice)
        {
            // For each line item, update the service patterns
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                // Normalize description
                string description = lineItem.Description.Trim();
                
                // Check if this service already exists in the profile
                if (profile.CommonServices.TryGetValue(description, out var servicePattern))
                {
                    // Update existing pattern
                    servicePattern.Frequency++;
                    servicePattern.LastSeen = DateTime.UtcNow;
                }
                else
                {
                    // Create new pattern
                    profile.CommonServices[description] = new ServicePattern
                    {
                        ServiceName = description,
                        Frequency = 1,
                        FirstSeen = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };
                }
            }
        }
        
        /// <summary>
        /// Updates the price ranges for a vendor profile
        /// </summary>
        private async Task UpdatePriceRangesAsync(VendorProfile profile, Invoice invoice)
        {
            // For each line item, update the price ranges
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                // Use description as the category key
                string category = lineItem.Description.Trim();
                
                // Calculate unit price when quantity > 1
                decimal unitPrice = lineItem.Quantity > 0 
                    ? lineItem.TotalPrice / lineItem.Quantity 
                    : lineItem.TotalPrice;
                    
                // Update price range for this category
                if (profile.PriceRanges.TryGetValue(category, out var priceRange))
                {
                    // Update existing price range
                    priceRange.MinPrice = Math.Min(priceRange.MinPrice, unitPrice);
                    priceRange.MaxPrice = Math.Max(priceRange.MaxPrice, unitPrice);
                    
                    // Update running average
                    decimal totalValue = priceRange.AveragePrice * priceRange.SampleSize;
                    priceRange.SampleSize++;
                    priceRange.AveragePrice = (totalValue + unitPrice) / priceRange.SampleSize;
                    
                    // Standard deviation calculation would be more complex and require storing all prices
                    // For simplicity, we're just updating min/max/avg here
                }
                else
                {
                    // Create new price range
                    profile.PriceRanges[category] = new PriceRange
                    {
                        ItemCategory = category,
                        MinPrice = unitPrice,
                        MaxPrice = unitPrice,
                        AveragePrice = unitPrice,
                        SampleSize = 1
                    };
                }
            }
        }
        
        /// <summary>
        /// Updates business categories based on validation result
        /// </summary>
        private async Task UpdateBusinessCategoriesAsync(VendorProfile profile, Invoice invoice, ValidationResult validationResult)
        {
            // Extract categories from validation result
            if (validationResult.PurchaseAnalysis?.Categories != null)
            {
                foreach (var category in validationResult.PurchaseAnalysis.Categories)
                {
                    if (!string.IsNullOrEmpty(category) && !profile.BusinessCategories.Contains(category))
                    {
                        profile.BusinessCategories.Add(category);
                    }
                }
            }
            
            // If we have a project category in the summary, add that too
            if (!string.IsNullOrEmpty(validationResult.Summary?.ProjectCategory) &&
                !profile.BusinessCategories.Contains(validationResult.Summary.ProjectCategory))
            {
                profile.BusinessCategories.Add(validationResult.Summary.ProjectCategory);
            }
        }
        
        /// <summary>
        /// Updates trust metrics based on validation result
        /// </summary>
        private async Task UpdateTrustMetricsAsync(VendorProfile profile, Invoice invoice, ValidationResult validationResult)
        {
            // Update reliability score based on validation confidence
            if (validationResult.ConfidenceScore > 0)
            {
                double newReliability = validationResult.ConfidenceScore / 100.0;
                
                // Weighted average, giving more weight to historical data as we collect more samples
                double historicalWeight = Math.Min(0.8, profile.InvoiceCount / 10.0);
                double newDataWeight = 1.0 - historicalWeight;
                
                profile.TrustMetrics.ReliabilityScore = 
                    (profile.TrustMetrics.ReliabilityScore * historicalWeight) + 
                    (newReliability * newDataWeight);
            }
            
            // Check for price consistency
            await UpdatePriceConsistencyScoreAsync(profile);
            
            // Check document quality
            if (invoice.VisualAnalysis != null)
            {
                double documentQuality = 0.5; // Default
                
                if (invoice.VisualAnalysis.HasLogo) documentQuality += 0.1;
                if (invoice.VisualAnalysis.HasStamp) documentQuality += 0.1;
                if (invoice.VisualAnalysis.HasTableStructure) documentQuality += 0.1;
                
                // Reduce score for detected anomalies
                documentQuality -= (0.1 * invoice.VisualAnalysis.DetectedAnomalies.Count);
                
                // Ensure bounds
                documentQuality = Math.Max(0.0, Math.Min(1.0, documentQuality));
                
                // Update with weighted average
                double historicalWeight = Math.Min(0.8, profile.InvoiceCount / 10.0);
                double newDataWeight = 1.0 - historicalWeight;
                
                profile.TrustMetrics.DocumentQualityScore = 
                    (profile.TrustMetrics.DocumentQualityScore * historicalWeight) + 
                    (documentQuality * newDataWeight);
            }
            
            // Record any detected anomalies
            if (validationResult.Issues != null)
            {
                foreach (var issue in validationResult.Issues)
                {
                    if (issue.Severity == ValidationSeverity.Warning || issue.Severity == ValidationSeverity.Error)
                    {
                        profile.TrustMetrics.DetectedAnomalies.Add(new AnomalyRecord
                        {
                            AnomalyType = issue.Severity.ToString(),
                            Description = issue.Message,
                            DetectedDate = DateTime.UtcNow,
                            Severity = issue.Severity == ValidationSeverity.Error ? 0.8 : 0.4,
                            Resolved = false
                        });
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the price consistency score based on price range stability
        /// </summary>
        private async Task UpdatePriceConsistencyScoreAsync(VendorProfile profile)
        {
            if (profile.PriceRanges.Count == 0)
                return;
                
            // Calculate average deviation across all categories
            double totalDeviation = 0;
            int categoryCount = 0;
            
            foreach (var priceRange in profile.PriceRanges.Values)
            {
                if (priceRange.SampleSize > 1 && priceRange.AveragePrice > 0)
                {
                    // Calculate max deviation percentage
                    double maxDeviation = Math.Max(
                        Math.Abs((double)(priceRange.MaxPrice - priceRange.AveragePrice) / (double)priceRange.AveragePrice),
                        Math.Abs((double)(priceRange.MinPrice - priceRange.AveragePrice) / (double)priceRange.AveragePrice)
                    );
                    
                    totalDeviation += maxDeviation;
                    categoryCount++;
                }
            }
            
            if (categoryCount > 0)
            {
                double avgDeviation = totalDeviation / categoryCount;
                
                // Convert to a score (lower deviation = higher score)
                double consistencyScore = 1.0 - Math.Min(1.0, avgDeviation);
                
                // Update with weighted average
                double historicalWeight = Math.Min(0.8, profile.InvoiceCount / 10.0);
                double newDataWeight = 1.0 - historicalWeight;
                
                profile.TrustMetrics.PriceStabilityScore = 
                    (profile.TrustMetrics.PriceStabilityScore * historicalWeight) + 
                    (consistencyScore * newDataWeight);
            }
        }
        
        /// <summary>
        /// Analyzes prices against industry averages
        /// </summary>
        private async Task AnalyzePricesAgainstIndustryAsync(
            Invoice invoice, 
            Dictionary<string, PriceRange> industryPriceRanges, 
            PriceAnalysisResult result)
        {
            // Check each line item against industry averages
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                // Calculate unit price
                decimal unitPrice = lineItem.Quantity > 0 
                    ? lineItem.TotalPrice / lineItem.Quantity 
                    : lineItem.TotalPrice;
                
                // Try to find matching item category in industry price ranges
                PriceRange matchingRange = null;
                
                // First try exact match
                if (industryPriceRanges.TryGetValue(lineItem.Description, out var exactRange))
                {
                    matchingRange = exactRange;
                }
                else
                {
                    // Try fuzzy matching
                    foreach (var range in industryPriceRanges)
                    {
                        if (range.Key.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                            lineItem.Description.Contains(range.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingRange = range.Value;
                            break;
                        }
                    }
                }
                
                // If we found a matching range, check if the price is reasonable
                if (matchingRange != null && matchingRange.SampleSize > 0)
                {
                    // Calculate permitted price ranges (with 50% buffer for industry averages)
                    decimal minAcceptable = matchingRange.MinPrice * 0.5m;
                    decimal maxAcceptable = matchingRange.MaxPrice * 1.5m;
                    
                    if (unitPrice < minAcceptable || unitPrice > maxAcceptable)
                    {
                        // Calculate deviation percentage
                        double deviationPercentage = 0;
                        
                        if (unitPrice < minAcceptable && matchingRange.MinPrice > 0)
                        {
                            deviationPercentage = (double)((matchingRange.MinPrice - unitPrice) / matchingRange.MinPrice);
                        }
                        else if (unitPrice > maxAcceptable && matchingRange.MaxPrice > 0)
                        {
                            deviationPercentage = (double)((unitPrice - matchingRange.MaxPrice) / matchingRange.MaxPrice);
                        }
                        
                        // Add to unreasonably priced items
                        result.UnreasonablyPricedItems.Add(new UnreasonablyPricedItem
                        {
                            Description = lineItem.Description,
                            TotalPrice = lineItem.TotalPrice,
                            ExpectedMinPrice = matchingRange.MinPrice,
                            ExpectedMaxPrice = matchingRange.MaxPrice,
                            DeviationPercentage = deviationPercentage
                        });
                        
                        // Update maximum deviation
                        if (deviationPercentage > result.MaxPriceDeviation)
                        {
                            result.MaxPriceDeviation = deviationPercentage;
                        }
                        
                        // Contribute to average deviation
                        result.AveragePriceDeviation += deviationPercentage;
                    }
                }
            }
            
            // Calculate average deviation
            if (result.UnreasonablyPricedItems.Count > 0)
            {
                result.AveragePriceDeviation /= result.UnreasonablyPricedItems.Count;
            }
            
            // Set flag if any unreasonable prices were detected
            result.UnreasonablePricesDetected = result.UnreasonablyPricedItems.Count > 0;
        }
        
        /// <summary>
        /// Analyzes services against a common list
        /// </summary>
        private void AnalyzeServicesAgainstCommonList(
            Invoice invoice, 
            List<string> commonServices, 
            ServiceAnalysisResult result)
        {
            // Match each line item against common services
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                bool matchFound = false;
                
                // Check for exact or fuzzy match in common services
                foreach (var service in commonServices)
                {
                    if (service.Equals(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        service.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        lineItem.Description.Contains(service, StringComparison.OrdinalIgnoreCase))
                    {
                        matchFound = true;
                        result.MatchingServices.Add(lineItem.Description);
                        break;
                    }
                }
                
                // If no match found, it's an unusual service
                if (!matchFound)
                {
                    result.UnusualServices.Add(lineItem.Description);
                }
            }
            
            // Set the flag for unusual services
            result.UnusualServicesDetected = result.UnusualServices.Count > 0;
            
            // Calculate the service pattern score
            if (invoice.LineItems.Count > 0)
            {
                result.ServicePatternScore = (double)result.MatchingServices.Count / invoice.LineItems.Count;
            }
            else
            {
                result.ServicePatternScore = 0.5; // Neutral if no line items
            }
        }
        
        /// <summary>
        /// Detects basic anomalies in an invoice
        /// </summary>
        private async Task DetectBasicAnomaliesAsync(
            Invoice invoice, 
            VendorProfile profile, 
            List<AnomalyRecord> anomalies)
        {
            // Check for missing KvK/VAT numbers
            if (string.IsNullOrEmpty(invoice.VendorKvkNumber) && string.IsNullOrEmpty(invoice.VendorBtwNumber))
            {
                anomalies.Add(new AnomalyRecord
                {
                    AnomalyType = "MissingRegistration",
                    Description = "Vendor is missing both KvK and VAT registration numbers",
                    DetectedDate = DateTime.UtcNow,
                    Severity = 0.6,
                    Resolved = false
                });
            }
            
            // Check for missing address
            if (string.IsNullOrEmpty(invoice.VendorAddress))
            {
                anomalies.Add(new AnomalyRecord
                {
                    AnomalyType = "MissingAddress",
                    Description = "Vendor address is missing from the invoice",
                    DetectedDate = DateTime.UtcNow,
                    Severity = 0.3,
                    Resolved = false
                });
            }
            
            // Check for round numbers in prices (often a sign of estimation rather than actual costs)
            var roundNumberItems = invoice.LineItems
                .Where(item => item.TotalPrice % 10 == 0 && item.TotalPrice > 50)
                .ToList();
                
            if (roundNumberItems.Count >= 3 || (invoice.LineItems.Count > 0 && roundNumberItems.Count == invoice.LineItems.Count))
            {
                anomalies.Add(new AnomalyRecord
                {
                    AnomalyType = "RoundNumbers",
                    Description = $"Invoice contains {roundNumberItems.Count} line items with suspiciously round prices",
                    DetectedDate = DateTime.UtcNow,
                    Severity = 0.4,
                    Resolved = false
                });
            }
        }
        
        /// <summary>
        /// Detects payment anomalies in an invoice
        /// </summary>
        private async Task DetectPaymentAnomaliesAsync(
            Invoice invoice, 
            VendorProfile profile, 
            List<AnomalyRecord> anomalies)
        {
            // Skip if no payment details in the invoice
            if (invoice.PaymentDetails == null || string.IsNullOrEmpty(invoice.PaymentDetails.IBAN))
                return;
                
            // Check if this is a new payment account for an established vendor
            if (profile.PaymentDetails.Count > 0)
            {
                bool isKnownAccount = profile.PaymentDetails.Any(p => 
                    p.IBAN == invoice.PaymentDetails.IBAN);
                    
                if (!isKnownAccount)
                {
                    anomalies.Add(new AnomalyRecord
                    {
                        AnomalyType = "NewBankAccount",
                        Description = $"New bank account detected for established vendor: {invoice.PaymentDetails.MaskedIBAN}",
                        DetectedDate = DateTime.UtcNow,
                        Severity = 0.7,
                        Resolved = false
                    });
                }
            }
            
            // Check for account holder name mismatch
            if (!string.IsNullOrEmpty(invoice.PaymentDetails.AccountHolderName) && 
                !string.IsNullOrEmpty(invoice.VendorName))
            {
                // Simple name similarity check
                string normalizedAccountName = VendorProfile.NormalizeVendorName(invoice.PaymentDetails.AccountHolderName);
                string normalizedVendorName = VendorProfile.NormalizeVendorName(invoice.VendorName);
                
                bool namesSimilar = normalizedAccountName.Contains(normalizedVendorName, StringComparison.OrdinalIgnoreCase) ||
                                    normalizedVendorName.Contains(normalizedAccountName, StringComparison.OrdinalIgnoreCase);
                                    
                if (!namesSimilar)
                {
                    anomalies.Add(new AnomalyRecord
                    {
                        AnomalyType = "AccountNameMismatch",
                        Description = $"Account holder name '{invoice.PaymentDetails.AccountHolderName}' does not match vendor name '{invoice.VendorName}'",
                        DetectedDate = DateTime.UtcNow,
                        Severity = 0.8,
                        Resolved = false
                    });
                }
            }
        }
        
        /// <summary>
        /// Detects price anomalies in an invoice
        /// </summary>
        private async Task DetectPriceAnomaliesAsync(
            Invoice invoice, 
            VendorProfile profile, 
            List<AnomalyRecord> anomalies)
        {
            // Skip for vendors with no price history
            if (profile.PriceRanges.Count == 0)
                return;
                
            // Analyze each line item for price anomalies
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                // Calculate unit price
                decimal unitPrice = lineItem.Quantity > 0 
                    ? lineItem.TotalPrice / lineItem.Quantity 
                    : lineItem.TotalPrice;
                
                // Try to find matching price range
                PriceRange matchingRange = null;
                string matchedCategory = null;
                
                // Try exact match first
                if (profile.PriceRanges.TryGetValue(lineItem.Description, out var exactRange))
                {
                    matchingRange = exactRange;
                    matchedCategory = lineItem.Description;
                }
                else
                {
                    // Try fuzzy matching
                    foreach (var range in profile.PriceRanges)
                    {
                        if (range.Key.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                            lineItem.Description.Contains(range.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingRange = range.Value;
                            matchedCategory = range.Key;
                            break;
                        }
                    }
                }
                
                // Check for significant price deviations
                if (matchingRange != null && matchingRange.SampleSize >= 2)
                {
                    // Calculate acceptable price range (with 40% buffer)
                    decimal minAcceptable = matchingRange.MinPrice * 0.6m;
                    decimal maxAcceptable = matchingRange.MaxPrice * 1.4m;
                    
                    if (unitPrice < minAcceptable)
                    {
                        // Suspiciously low price
                        decimal deviation = (minAcceptable - unitPrice) / minAcceptable;
                        
                        if (deviation > 0.2m) // More than 20% below acceptable minimum
                        {
                            anomalies.Add(new AnomalyRecord
                            {
                                AnomalyType = "SuspiciouslyLowPrice",
                                Description = $"Price for '{lineItem.Description}' is {deviation:P0} below the minimum expected price",
                                DetectedDate = DateTime.UtcNow,
                                Severity = Math.Min(0.3 + (double)deviation, 0.7),
                                Resolved = false
                            });
                        }
                    }
                    else if (unitPrice > maxAcceptable)
                    {
                        // Suspiciously high price
                        decimal deviation = (unitPrice - maxAcceptable) / maxAcceptable;
                        
                        if (deviation > 0.2m) // More than 20% above acceptable maximum
                        {
                            anomalies.Add(new AnomalyRecord
                            {
                                AnomalyType = "SuspiciouslyHighPrice",
                                Description = $"Price for '{lineItem.Description}' is {deviation:P0} above the maximum expected price",
                                DetectedDate = DateTime.UtcNow,
                                Severity = Math.Min(0.4 + (double)deviation, 0.8),
                                Resolved = false
                            });
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Detects service pattern anomalies in an invoice
        /// </summary>
        private async Task DetectServiceAnomaliesAsync(
            Invoice invoice, 
            VendorProfile profile, 
            List<AnomalyRecord> anomalies)
        {
            // Skip for vendors with no service history
            if (profile.CommonServices.Count == 0)
                return;
                
            // Get the common services offered by this vendor
            var commonServices = profile.CommonServices.Values
                .OrderByDescending(s => s.Frequency)
                .Select(s => s.ServiceName)
                .ToList();
                
            // Find unusual services in this invoice
            var unusualServices = new List<string>();
            
            foreach (var lineItem in invoice.LineItems)
            {
                // Skip items without description
                if (string.IsNullOrEmpty(lineItem.Description))
                    continue;
                    
                bool isCommonService = false;
                
                // Check if this service matches any of the common services
                foreach (var service in commonServices)
                {
                    if (service.Equals(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        service.Contains(lineItem.Description, StringComparison.OrdinalIgnoreCase) ||
                        lineItem.Description.Contains(service, StringComparison.OrdinalIgnoreCase))
                    {
                        isCommonService = true;
                        break;
                    }
                }
                
                if (!isCommonService)
                {
                    unusualServices.Add(lineItem.Description);
                }
            }
            
            // Add anomaly if unusual services are detected
            if (unusualServices.Count > 0)
            {
                string unusualServicesList = string.Join(", ", unusualServices.Take(3));
                if (unusualServices.Count > 3)
                {
                    unusualServicesList += $", and {unusualServices.Count - 3} more";
                }
                
                anomalies.Add(new AnomalyRecord
                {
                    AnomalyType = "UnusualServices",
                    Description = $"Unusual services for this vendor: {unusualServicesList}",
                    DetectedDate = DateTime.UtcNow,
                    Severity = Math.Min(0.3 + (unusualServices.Count * 0.1), 0.7),
                    Resolved = false
                });
            }
            
            // Check for known vendor specialties
            if (profile.SpecialtyServices.Count > 0)
            {
                bool hasSpecialtyService = invoice.LineItems
                    .Any(item => profile.SpecialtyServices
                        .Any(specialty => 
                            item.Description.Contains(specialty, StringComparison.OrdinalIgnoreCase) ||
                            specialty.Contains(item.Description, StringComparison.OrdinalIgnoreCase)));
                            
                if (!hasSpecialtyService && invoice.LineItems.Count > 0)
                {
                    anomalies.Add(new AnomalyRecord
                    {
                        AnomalyType = "NoSpecialtyServices",
                        Description = $"Invoice does not contain any of vendor's specialty services: {string.Join(", ", profile.SpecialtyServices.Take(3))}",
                        DetectedDate = DateTime.UtcNow,
                        Severity = 0.5,
                        Resolved = false
                    });
                }
            }
        }
        
        #endregion
    }
}
