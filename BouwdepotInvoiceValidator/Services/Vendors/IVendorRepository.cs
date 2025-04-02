using System.Collections.Generic;
using System.Threading.Tasks;
using BouwdepotInvoiceValidator.Models;

namespace BouwdepotInvoiceValidator.Services.Vendors
{
    /// <summary>
    /// Interface for vendor repository operations
    /// </summary>
    public interface IVendorRepository
    {
        /// <summary>
        /// Gets a vendor by ID
        /// </summary>
        /// <param name="id">The vendor ID</param>
        /// <returns>The vendor profile or null if not found</returns>
        Task<VendorProfile> GetVendorAsync(string id);
        
        /// <summary>
        /// Gets a vendor by tax identifiers (KvK and/or VAT number)
        /// </summary>
        /// <param name="kvkNumber">Chamber of Commerce number</param>
        /// <param name="vatNumber">VAT number</param>
        /// <returns>The vendor profile or null if not found</returns>
        Task<VendorProfile> GetVendorByTaxIdAsync(string kvkNumber, string vatNumber);
        
        /// <summary>
        /// Gets a vendor by name (using normalized name for matching)
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <returns>The vendor profile or null if not found</returns>
        Task<VendorProfile> GetVendorByNameAsync(string name);
        
        /// <summary>
        /// Searches for vendors by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>Matching vendor profiles</returns>
        Task<List<VendorProfile>> SearchVendorsAsync(string searchTerm);
        
        /// <summary>
        /// Creates or updates a vendor profile
        /// </summary>
        /// <param name="vendor">The vendor profile to upsert</param>
        /// <returns>The vendor ID</returns>
        Task<string> UpsertVendorAsync(VendorProfile vendor);
        
        /// <summary>
        /// Gets vendors by business category
        /// </summary>
        /// <param name="category">Business category</param>
        /// <returns>Matching vendor profiles</returns>
        Task<List<VendorProfile>> GetVendorsByBusinessCategoryAsync(string category);
        
        /// <summary>
        /// Gets vendors with detected anomalies
        /// </summary>
        /// <returns>Vendor profiles with anomalies</returns>
        Task<List<VendorProfile>> GetVendorsWithAnomaliesAsync();
        
        /// <summary>
        /// Gets common services in a specific business category
        /// </summary>
        /// <param name="category">Business category</param>
        /// <returns>Common service patterns in the category</returns>
        Task<List<ServicePattern>> GetCommonServicesInCategoryAsync(string category);
        
        /// <summary>
        /// Gets average price ranges across the industry
        /// </summary>
        /// <returns>Industry price ranges by item category</returns>
        Task<Dictionary<string, PriceRange>> GetIndustryPriceRangesAsync();
        
        /// <summary>
        /// Gets the total count of vendors
        /// </summary>
        /// <returns>Number of vendor profiles</returns>
        Task<int> GetVendorCountAsync();
        
        /// <summary>
        /// Deletes a vendor profile
        /// </summary>
        /// <param name="id">The vendor ID to delete</param>
        /// <returns>Whether the deletion was successful</returns>
        Task<bool> DeleteVendorAsync(string id);
    }
}
