namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Represents a construction activity category for Bouwdepot eligibility
    /// </summary>
    public class ConstructionActivity
    {
        /// <summary>
        /// Unique identifier for the construction activity
        /// </summary>
        public string ActivityId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the construction activity
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Description of what the activity entails
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of the construction activity
        /// </summary>
        public ConstructionActivityCategory Category { get; set; }

        /// <summary>
        /// Whether this activity is eligible for construction fund financing
        /// </summary>
        public bool IsEligible { get; set; } = true;

        /// <summary>
        /// Keywords that can help identify this activity in document text
        /// </summary>
        public List<string> IdentificationKeywords { get; set; } = new List<string>();
    }

    /// <summary>
    /// Categories of construction activities for Bouwdepot financing
    /// </summary>
    public enum ConstructionActivityCategory
    {
        /// <summary>
        /// Structural work (foundations, walls, roof)
        /// </summary>
        Structural,

        /// <summary>
        /// Mechanical, electrical, and plumbing work
        /// </summary>
        MEP,

        /// <summary>
        /// Interior finishing (floors, walls, ceilings)
        /// </summary>
        InteriorFinishing,

        /// <summary>
        /// Exterior work (landscaping, driveways)
        /// </summary>
        Exterior,

        /// <summary>
        /// Kitchen and bathroom installations
        /// </summary>
        KitchenBathroom,

        /// <summary>
        /// Energy efficiency improvements
        /// </summary>
        EnergyEfficiency,

        /// <summary>
        /// Home security systems
        /// </summary>
        Security,

        /// <summary>
        /// Smart home technology
        /// </summary>
        SmartHome,

        /// <summary>
        /// Permit and inspection fees
        /// </summary>
        PermitsFees,

        /// <summary>
        /// Other eligible activities
        /// </summary>
        Other
    }
}
