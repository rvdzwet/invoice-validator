using System;

namespace BouwdepotInvoiceValidator.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark a class as a prompt schema for JSON generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PromptSchemaAttribute : Attribute
    {
        /// <summary>
        /// Description of the schema
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptSchemaAttribute"/> class
        /// </summary>
        /// <param name="description">Description of the schema</param>
        public PromptSchemaAttribute(string description = null)
        {
            Description = description;
        }
    }
}
