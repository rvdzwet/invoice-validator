namespace BouwdepotInvoiceValidator.Domain.Attributes
{
    /// <summary>
    /// Attribute to mark a property to be ignored in prompt schema generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PromptIgnoreAttribute : Attribute
    {
        // This attribute doesn't need any properties or methods
        // It's just a marker to indicate that a property should be ignored
    }
}
