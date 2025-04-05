using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Vendors
{
    /// <summary>
    /// In-memory implementation of the vendor repository for development and testing
    /// </summary>
    public class InMemoryVendorRepository : IVendorRepository
    {
        private readonly Dictionary<string, VendorProfile> _vendorsById = new();
        private readonly Dictionary<string, string> _vendorIdsByTaxId = new();
        private readonly Dictionary<string, string> _vendorIdsByNormalizedName = new();
        private readonly ILogger<InMemoryVendorRepository> _logger;
        
        public InMemoryVendorRepository(ILogger<InMemoryVendorRepository> logger)
        {
            _logger = logger;
            
            // Add some sample data in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                InitializeSampleData();
            }
            
            _logger.LogInformation("Initialized InMemoryVendorRepository with {Count} vendors", _vendorsById.Count);
        }
        
        /// <summary>
        /// Gets a vendor by ID
        /// </summary>
        public Task<VendorProfile> GetVendorAsync(string id)
        {
            if (string.IsNullOrEmpty(id) || !_vendorsById.TryGetValue(id, out var vendor))
            {
                return Task.FromResult<VendorProfile>(null);
            }
            
            return Task.FromResult(vendor);
        }
        
        /// <summary>
        /// Gets a vendor by tax identifiers (KvK and/or VAT number)
        /// </summary>
        public Task<VendorProfile> GetVendorByTaxIdAsync(string kvkNumber, string vatNumber)
        {
            string taxKey = GetTaxIdKey(kvkNumber, vatNumber);
            
            if (!string.IsNullOrEmpty(taxKey) && _vendorIdsByTaxId.TryGetValue(taxKey, out var id))
            {
                return GetVendorAsync(id);
            }
            
            return Task.FromResult<VendorProfile>(null);
        }
        
        /// <summary>
        /// Gets a vendor by name (using normalized name for matching)
        /// </summary>
        public Task<VendorProfile> GetVendorByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Task.FromResult<VendorProfile>(null);
            }
            
            string normalizedName = VendorProfile.NormalizeVendorName(name);
            
            if (_vendorIdsByNormalizedName.TryGetValue(normalizedName, out var id))
            {
                return GetVendorAsync(id);
            }
            
            // Try fuzzy matching if exact match fails
            foreach (var entry in _vendorIdsByNormalizedName)
            {
                // Simple contains check for fuzzy matching
                if (entry.Key.Contains(normalizedName) || normalizedName.Contains(entry.Key))
                {
                    return GetVendorAsync(entry.Value);
                }
            }
            
            return Task.FromResult<VendorProfile>(null);
        }
        
        /// <summary>
        /// Searches for vendors by name
        /// </summary>
        public Task<List<VendorProfile>> SearchVendorsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Task.FromResult(new List<VendorProfile>());
            }
            
            string normalizedSearchTerm = VendorProfile.NormalizeVendorName(searchTerm).ToLowerInvariant();
            
            var results = _vendorsById.Values
                .Where(v => v.NormalizedName.Contains(normalizedSearchTerm) || 
                           normalizedSearchTerm.Contains(v.NormalizedName) ||
                           v.VendorName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                .OrderByDescending(v => v.InvoiceCount)
                .ToList();
                
            return Task.FromResult(results);
        }
        
        /// <summary>
        /// Creates or updates a vendor profile
        /// </summary>
        public Task<string> UpsertVendorAsync(VendorProfile vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException(nameof(vendor));
            }
            
            // Ensure the vendor has an ID
            if (string.IsNullOrEmpty(vendor.Id))
            {
                vendor.Id = Guid.NewGuid().ToString();
            }
            
            // Store or update the vendor
            _vendorsById[vendor.Id] = vendor;
            
            // Update tax ID index
            string taxKey = GetTaxIdKey(vendor.KvkNumber, vendor.VatNumber);
            if (!string.IsNullOrEmpty(taxKey))
            {
                _vendorIdsByTaxId[taxKey] = vendor.Id;
            }
            
            // Update name index
            if (!string.IsNullOrEmpty(vendor.NormalizedName))
            {
                _vendorIdsByNormalizedName[vendor.NormalizedName] = vendor.Id;
            }
            
            _logger.LogInformation("Upserted vendor profile: {VendorName}, Id: {VendorId}", 
                vendor.VendorName, vendor.Id);
                
            return Task.FromResult(vendor.Id);
        }
        
        /// <summary>
        /// Gets vendors by business category
        /// </summary>
        public Task<List<VendorProfile>> GetVendorsByBusinessCategoryAsync(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return Task.FromResult(new List<VendorProfile>());
            }
            
            var results = _vendorsById.Values
                .Where(v => v.BusinessCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(v => v.InvoiceCount)
                .ToList();
                
            return Task.FromResult(results);
        }
        
        /// <summary>
        /// Gets vendors with detected anomalies
        /// </summary>
        public Task<List<VendorProfile>> GetVendorsWithAnomaliesAsync()
        {
            var results = _vendorsById.Values
                .Where(v => v.TrustMetrics.DetectedAnomalies.Count > 0)
                .OrderByDescending(v => v.TrustMetrics.DetectedAnomalies
                    .Where(a => !a.Resolved)
                    .Sum(a => a.Severity))
                .ToList();
                
            return Task.FromResult(results);
        }
        
        /// <summary>
        /// Gets common services in a specific business category
        /// </summary>
        public Task<List<ServicePattern>> GetCommonServicesInCategoryAsync(string category)
        {
            var categoryVendors = _vendorsById.Values
                .Where(v => v.BusinessCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                .ToList();
                
            // Collect all service patterns from vendors in this category
            var servicePatterns = new Dictionary<string, ServicePattern>();
            
            foreach (var vendor in categoryVendors)
            {
                foreach (var service in vendor.CommonServices.Values)
                {
                    if (servicePatterns.TryGetValue(service.ServiceName, out var existingPattern))
                    {
                        // Combine frequency
                        existingPattern.Frequency += service.Frequency;
                        
                        // Update dates
                        existingPattern.FirstSeen = existingPattern.FirstSeen < service.FirstSeen 
                            ? existingPattern.FirstSeen : service.FirstSeen;
                        existingPattern.LastSeen = existingPattern.LastSeen > service.LastSeen 
                            ? existingPattern.LastSeen : service.LastSeen;
                            
                        // Merge keywords
                        foreach (var keyword in service.RelatedKeywords)
                        {
                            if (!existingPattern.RelatedKeywords.Contains(keyword))
                            {
                                existingPattern.RelatedKeywords.Add(keyword);
                            }
                        }
                    }
                    else
                    {
                        // Clone the service pattern
                        servicePatterns[service.ServiceName] = new ServicePattern
                        {
                            ServiceName = service.ServiceName,
                            Frequency = service.Frequency,
                            FirstSeen = service.FirstSeen,
                            LastSeen = service.LastSeen,
                            RelatedKeywords = new List<string>(service.RelatedKeywords)
                        };
                    }
                }
            }
            
            // Return the combined service patterns, sorted by frequency
            var results = servicePatterns.Values
                .OrderByDescending(p => p.Frequency)
                .ToList();
                
            return Task.FromResult(results);
        }
        
        /// <summary>
        /// Gets average price ranges across the industry
        /// </summary>
        public Task<Dictionary<string, PriceRange>> GetIndustryPriceRangesAsync()
        {
            // Collect all price ranges from all vendors
            var industryPriceRanges = new Dictionary<string, List<PriceRange>>();
            
            foreach (var vendor in _vendorsById.Values)
            {
                foreach (var priceRange in vendor.PriceRanges)
                {
                    string category = priceRange.Key;
                    var range = priceRange.Value;
                    
                    if (!industryPriceRanges.ContainsKey(category))
                    {
                        industryPriceRanges[category] = new List<PriceRange>();
                    }
                    
                    industryPriceRanges[category].Add(range);
                }
            }
            
            // Calculate industry-wide price ranges
            var results = new Dictionary<string, PriceRange>();
            
            foreach (var category in industryPriceRanges.Keys)
            {
                var ranges = industryPriceRanges[category];
                
                if (ranges.Count == 0)
                {
                    continue;
                }
                
                // Calculate aggregated stats
                decimal minPrice = ranges.Min(r => r.MinPrice);
                decimal maxPrice = ranges.Max(r => r.MaxPrice);
                decimal totalAverage = 0;
                int totalSamples = 0;
                
                foreach (var range in ranges)
                {
                    totalAverage += range.AveragePrice * range.SampleSize;
                    totalSamples += range.SampleSize;
                }
                
                decimal averagePrice = totalSamples > 0 
                    ? totalAverage / totalSamples 
                    : 0;
                    
                // Create the industry-wide price range
                results[category] = new PriceRange
                {
                    ItemCategory = category,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    AveragePrice = averagePrice,
                    SampleSize = totalSamples
                };
            }
            
            return Task.FromResult(results);
        }
        
        /// <summary>
        /// Gets the total count of vendors
        /// </summary>
        public Task<int> GetVendorCountAsync()
        {
            return Task.FromResult(_vendorsById.Count);
        }
        
        /// <summary>
        /// Deletes a vendor profile
        /// </summary>
        public Task<bool> DeleteVendorAsync(string id)
        {
            if (string.IsNullOrEmpty(id) || !_vendorsById.TryGetValue(id, out var vendor))
            {
                return Task.FromResult(false);
            }
            
            // Remove from the main dictionary
            _vendorsById.Remove(id);
            
            // Remove from tax ID index
            string taxKey = GetTaxIdKey(vendor.KvkNumber, vendor.VatNumber);
            if (!string.IsNullOrEmpty(taxKey))
            {
                _vendorIdsByTaxId.Remove(taxKey);
            }
            
            // Remove from name index
            if (!string.IsNullOrEmpty(vendor.NormalizedName))
            {
                _vendorIdsByNormalizedName.Remove(vendor.NormalizedName);
            }
            
            _logger.LogInformation("Deleted vendor profile: {VendorName}, Id: {VendorId}", 
                vendor.VendorName, vendor.Id);
                
            return Task.FromResult(true);
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Creates a key for tax ID lookups
        /// </summary>
        private string GetTaxIdKey(string kvkNumber, string vatNumber)
        {
            if (!string.IsNullOrEmpty(kvkNumber))
            {
                return $"KVK:{kvkNumber}";
            }
            
            if (!string.IsNullOrEmpty(vatNumber))
            {
                return $"VAT:{vatNumber}";
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Initializes sample data for development
        /// </summary>
        private void InitializeSampleData()
        {
            var sampleVendors = new List<VendorProfile>
            {
                // Construction company
                new VendorProfile
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorName = "Bouwbedrijf Jansen B.V.",
                    NormalizedName = VendorProfile.NormalizeVendorName("Bouwbedrijf Jansen B.V."),
                    KvkNumber = "12345678",
                    VatNumber = "NL123456789B01",
                    KnownAddresses = new HashSet<string> { "Bouwweg 123, 1234 AB Amsterdam" },
                    BusinessCategories = new List<string> { "Construction", "Renovation", "Carpentry" },
                    SpecialtyServices = new List<string> { "Home renovation", "Extension building", "Foundation work" },
                    InvoiceCount = 15,
                    FirstSeen = DateTime.UtcNow.AddMonths(-6),
                    LastSeen = DateTime.UtcNow.AddDays(-5),
                    TrustMetrics = new VendorTrustMetrics
                    {
                        ReliabilityScore = 0.92,
                        ConsistencyScore = 0.85,
                        PriceStabilityScore = 0.78,
                        DocumentQualityScore = 0.88
                    },
                    PaymentDetails = new List<PaymentDetails>
                    {
                        new PaymentDetails
                        {
                            AccountHolderName = "Bouwbedrijf Jansen B.V.",
                            IBAN = "NL91ABNA0417164300",
                            BIC = "ABNANL2A",
                            BankName = "ABN AMRO"
                        }
                    }
                },
                
                // Plumbing company
                new VendorProfile
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorName = "Loodgietersbedrijf De Vries",
                    NormalizedName = VendorProfile.NormalizeVendorName("Loodgietersbedrijf De Vries"),
                    KvkNumber = "87654321",
                    VatNumber = "NL987654321B01",
                    KnownAddresses = new HashSet<string> { "Waterstraat 45, 2345 BC Rotterdam" },
                    BusinessCategories = new List<string> { "Plumbing", "Bathroom Renovation", "Heating Systems" },
                    SpecialtyServices = new List<string> { "Leak repair", "Bathroom installation", "Central heating" },
                    InvoiceCount = 8,
                    FirstSeen = DateTime.UtcNow.AddMonths(-4),
                    LastSeen = DateTime.UtcNow.AddDays(-12),
                    TrustMetrics = new VendorTrustMetrics
                    {
                        ReliabilityScore = 0.85,
                        ConsistencyScore = 0.82,
                        PriceStabilityScore = 0.90,
                        DocumentQualityScore = 0.75
                    },
                    PaymentDetails = new List<PaymentDetails>
                    {
                        new PaymentDetails
                        {
                            AccountHolderName = "L. de Vries",
                            IBAN = "NL39RABO0300065264",
                            BIC = "RABONL2U",
                            BankName = "Rabobank"
                        }
                    },
                    CommonServices = new Dictionary<string, ServicePattern>
                    {
                        ["Bathroom installation"] = new ServicePattern
                        {
                            ServiceName = "Bathroom installation",
                            Frequency = 5,
                            FirstSeen = DateTime.UtcNow.AddMonths(-4),
                            LastSeen = DateTime.UtcNow.AddDays(-12)
                        },
                        ["Central heating repair"] = new ServicePattern
                        {
                            ServiceName = "Central heating repair",
                            Frequency = 3,
                            FirstSeen = DateTime.UtcNow.AddMonths(-3),
                            LastSeen = DateTime.UtcNow.AddDays(-35)
                        }
                    },
                    PriceRanges = new Dictionary<string, PriceRange>
                    {
                        ["Bathroom installation"] = new PriceRange
                        {
                            ItemCategory = "Bathroom installation",
                            MinPrice = 2500.00m,
                            MaxPrice = 7500.00m,
                            AveragePrice = 4200.00m,
                            SampleSize = 5
                        },
                        ["Central heating repair"] = new PriceRange
                        {
                            ItemCategory = "Central heating repair",
                            MinPrice = 150.00m,
                            MaxPrice = 350.00m,
                            AveragePrice = 225.00m,
                            SampleSize = 3
                        }
                    }
                },
                
                // Painting company
                new VendorProfile
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorName = "Schildersbedrijf Kleurrijk VOF",
                    NormalizedName = VendorProfile.NormalizeVendorName("Schildersbedrijf Kleurrijk VOF"),
                    KvkNumber = "23456789",
                    VatNumber = "NL234567890B01",
                    KnownAddresses = new HashSet<string> { "Verfstraat 67, 3456 CD Utrecht" },
                    BusinessCategories = new List<string> { "Painting", "Wallpapering", "Plastering" },
                    SpecialtyServices = new List<string> { "Interior painting", "Exterior painting", "Decorative painting" },
                    InvoiceCount = 12,
                    FirstSeen = DateTime.UtcNow.AddMonths(-5),
                    LastSeen = DateTime.UtcNow.AddDays(-8),
                    TrustMetrics = new VendorTrustMetrics
                    {
                        ReliabilityScore = 0.95,
                        ConsistencyScore = 0.93,
                        PriceStabilityScore = 0.88,
                        DocumentQualityScore = 0.82,
                        DetectedAnomalies = new List<AnomalyRecord>
                        {
                            new AnomalyRecord
                            {
                                AnomalyType = "PriceIncrease",
                                Description = "Significant price increase for interior painting",
                                DetectedDate = DateTime.UtcNow.AddDays(-8),
                                Severity = 0.4,
                                Resolved = false
                            }
                        }
                    }
                }
            };
            
            // Add the sample vendors to the repository
            foreach (var vendor in sampleVendors)
            {
                _vendorsById[vendor.Id] = vendor;
                
                // Index by tax ID
                string taxKey = GetTaxIdKey(vendor.KvkNumber, vendor.VatNumber);
                if (!string.IsNullOrEmpty(taxKey))
                {
                    _vendorIdsByTaxId[taxKey] = vendor.Id;
                }
                
                // Index by name
                if (!string.IsNullOrEmpty(vendor.NormalizedName))
                {
                    _vendorIdsByNormalizedName[vendor.NormalizedName] = vendor.Id;
                }
            }
            
            _logger.LogInformation("Initialized {Count} sample vendors for development", sampleVendors.Count);
        }
        
        #endregion
    }
}
