namespace BouwdepotInvoiceValidator.Domain.Services.Schema
{
    /// <summary>
    /// Interface for generating JSON schemas from annotated POCO classes
    /// </summary>
    public interface IJsonSchemaGenerator
    {
        /// <summary>
        /// Generates a JSON schema for the specified type
        /// </summary>
        /// <typeparam name="T">The type to generate a schema for</typeparam>
        /// <returns>A JSON schema as a string</returns>
        string GenerateSchema<T>();
        
        /// <summary>
        /// Generates a JSON schema for the specified type
        /// </summary>
        /// <param name="type">The type to generate a schema for</param>
        /// <returns>A JSON schema as a string</returns>
        string GenerateSchema(Type type);
        
        /// <summary>
        /// Generates an example JSON object for the specified type
        /// </summary>
        /// <typeparam name="T">The type to generate an example for</typeparam>
        /// <returns>An example JSON object as a string</returns>
        string GenerateExampleJson<T>();
        
        /// <summary>
        /// Generates an example JSON object for the specified type
        /// </summary>
        /// <param name="type">The type to generate an example for</param>
        /// <returns>An example JSON object as a string</returns>
        string GenerateExampleJson(Type type);
    }
}
