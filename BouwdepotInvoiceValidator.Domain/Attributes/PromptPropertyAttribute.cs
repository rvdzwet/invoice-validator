using System;

namespace BouwdepotInvoiceValidator.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark a property for inclusion in prompt schema generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PromptPropertyAttribute : Attribute
    {
        /// <summary>
        /// Description of the property
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the property is required
        /// </summary>
        public bool Required { get; set; }
        
        /// <summary>
        /// Default value for the property
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// Example value for the property (used in schema examples)
        /// </summary>
        public object ExampleValue { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptPropertyAttribute"/> class
        /// </summary>
        /// <param name="description">Description of the property</param>
        /// <param name="required">Whether the property is required</param>
        public PromptPropertyAttribute(string description = null, bool required = true)
        {
            Description = description;
            Required = required;
            DefaultValue = null;
            ExampleValue = null;
        }
    }
}
