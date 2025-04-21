using BouwdepotInvoiceValidator.Domain.Models;
using System.Text.RegularExpressions;

namespace BouwdepotInvoiceValidator.Domain.Services.Pipeline.Steps
{
    /// <summary>
    /// Helper methods for withdrawal eligibility step
    /// </summary>
    public static class WithdrawalEligibilityHelper
    {
        /// <summary>
        /// Maps a category string to the corresponding construction activity category enum value
        /// </summary>
        /// <param name="categoryString">String representation of the category</param>
        /// <returns>Corresponding enum value</returns>
        public static ConstructionActivityCategory MapCategoryStringToEnum(string categoryString)
        {
            if (string.IsNullOrWhiteSpace(categoryString))
                return ConstructionActivityCategory.Other;
                
            categoryString = categoryString.ToLowerInvariant().Trim();
            
            if (categoryString.Contains("structural") || 
                categoryString.Contains("foundation") || 
                categoryString.Contains("wall") || 
                categoryString.Contains("roof") ||
                categoryString.Contains("frame"))
                return ConstructionActivityCategory.Structural;
                
            if (categoryString.Contains("mep") || 
                categoryString.Contains("mechanical") || 
                categoryString.Contains("electrical") || 
                categoryString.Contains("plumbing") ||
                categoryString.Contains("hvac") ||
                categoryString.Contains("wiring") ||
                categoryString.Contains("piping"))
                return ConstructionActivityCategory.MEP;
                
            if (categoryString.Contains("interior") || 
                categoryString.Contains("finishing") || 
                categoryString.Contains("paint") || 
                categoryString.Contains("drywall") ||
                categoryString.Contains("ceiling") ||
                categoryString.Contains("floor"))
                return ConstructionActivityCategory.InteriorFinishing;
                
            if (categoryString.Contains("exterior") || 
                categoryString.Contains("landscaping") || 
                categoryString.Contains("driveway") || 
                categoryString.Contains("garden") ||
                categoryString.Contains("fence"))
                return ConstructionActivityCategory.Exterior;
                
            if (categoryString.Contains("kitchen") || 
                categoryString.Contains("bathroom") || 
                categoryString.Contains("sink") || 
                categoryString.Contains("toilet") ||
                categoryString.Contains("cabinets") ||
                categoryString.Contains("bathtub") ||
                categoryString.Contains("shower"))
                return ConstructionActivityCategory.KitchenBathroom;
                
            if (categoryString.Contains("energy") || 
                categoryString.Contains("efficiency") || 
                categoryString.Contains("insulation") || 
                categoryString.Contains("solar") ||
                categoryString.Contains("green") ||
                categoryString.Contains("sustainable"))
                return ConstructionActivityCategory.EnergyEfficiency;
                
            if (categoryString.Contains("security") || 
                categoryString.Contains("alarm") || 
                categoryString.Contains("camera") || 
                categoryString.Contains("surveillance") ||
                categoryString.Contains("lock") ||
                categoryString.Contains("safety"))
                return ConstructionActivityCategory.Security;
                
            if (categoryString.Contains("smart") || 
                categoryString.Contains("automation") || 
                categoryString.Contains("iot") || 
                categoryString.Contains("internet of things") ||
                categoryString.Contains("control system"))
                return ConstructionActivityCategory.SmartHome;
                
            if (categoryString.Contains("permit") || 
                categoryString.Contains("fee") || 
                categoryString.Contains("inspection") || 
                categoryString.Contains("regulatory") ||
                categoryString.Contains("compliance") ||
                categoryString.Contains("approval"))
                return ConstructionActivityCategory.PermitsFees;
                
            return ConstructionActivityCategory.Other;
        }
        
        /// <summary>
        /// Generates a summary of identified construction activities
        /// </summary>
        /// <param name="activities">List of construction activities</param>
        /// <returns>A human-readable summary</returns>
        public static string GenerateActivitySummary(List<ConstructionActivity> activities)
        {
            if (activities == null || !activities.Any())
                return "No specific construction activities identified.";
                
            var categories = activities
                .GroupBy(a => a.Category)
                .OrderByDescending(g => g.Count())
                .Select(g => new {
                    Category = g.Key,
                    Count = g.Count(),
                    Items = g.Take(3).Select(a => a.ActivityName).ToList()
                })
                .ToList();
                
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Identified {activities.Count} construction-related activities across {categories.Count} categories:");
            
            foreach (var category in categories)
            {
                summary.AppendLine($"- {category.Category} ({category.Count} items): {string.Join(", ", category.Items)}");
                if (category.Count > 3)
                    summary.Append(" and more...");
            }
            
            return summary.ToString().TrimEnd();
        }
    }
}
