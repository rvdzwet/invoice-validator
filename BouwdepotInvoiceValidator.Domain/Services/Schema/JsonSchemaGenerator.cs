using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using BouwdepotInvoiceValidator.Domain.Attributes;
using Microsoft.Extensions.Logging;

namespace BouwdepotInvoiceValidator.Domain.Services.Schema
{
    /// <summary>
    /// Generates JSON schemas from annotated POCO classes
    /// </summary>
    public class JsonSchemaGenerator : IJsonSchemaGenerator
    {
        private readonly ILogger<JsonSchemaGenerator> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSchemaGenerator"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public JsonSchemaGenerator(ILogger<JsonSchemaGenerator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc/>
        public string GenerateSchema<T>()
        {
            return GenerateSchema(typeof(T));
        }
        
        /// <inheritdoc/>
        public string GenerateSchema(Type type)
        {
            try
            {
                var schema = new StringBuilder();
                schema.AppendLine("{");
                
                // Add schema metadata
                schema.AppendLine("  \"$schema\": \"http://json-schema.org/draft-07/schema#\",");
                schema.AppendLine("  \"type\": \"object\",");
                
                // Get class description from PromptSchemaAttribute if available
                var schemaAttribute = type.GetCustomAttribute<PromptSchemaAttribute>();
                if (schemaAttribute != null && !string.IsNullOrEmpty(schemaAttribute.Description))
                {
                    schema.AppendLine($"  \"description\": \"{schemaAttribute.Description}\",");
                }
                
                // Add properties
                schema.AppendLine("  \"properties\": {");
                
                var properties = type.GetProperties()
                    .Where(p => !p.GetCustomAttributes<PromptIgnoreAttribute>().Any())
                    .ToList();
                
                var requiredProperties = new List<string>();
                
                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var propertyAttribute = property.GetCustomAttribute<PromptPropertyAttribute>();
                    
                    // Skip properties with PromptIgnoreAttribute
                    if (property.GetCustomAttribute<PromptIgnoreAttribute>() != null)
                    {
                        continue;
                    }
                    
                    // Add to required properties list if marked as required
                    if (propertyAttribute?.Required ?? false)
                    {
                        requiredProperties.Add(GetJsonPropertyName(property));
                    }
                    
                    // Add property schema
                    schema.AppendLine($"    \"{GetJsonPropertyName(property)}\": {{");
                    
                    // Add property type
                    schema.AppendLine($"      \"type\": \"{GetJsonType(property.PropertyType)}\",");
                    
                    // Add property description if available
                    if (propertyAttribute != null && !string.IsNullOrEmpty(propertyAttribute.Description))
                    {
                        schema.AppendLine($"      \"description\": \"{propertyAttribute.Description}\",");
                    }
                    
                    // Add default value if available
                    if (propertyAttribute?.DefaultValue != null)
                    {
                        var defaultValue = FormatJsonValue(propertyAttribute.DefaultValue, property.PropertyType);
                        schema.AppendLine($"      \"default\": {defaultValue},");
                    }
                    
                    // Add additional type-specific constraints
                    AddTypeSpecificConstraints(schema, property);
                    
                    // Close property definition
                    schema.Append("    }");
                    
                    // Add comma if not the last property
                    if (i < properties.Count - 1)
                    {
                        schema.AppendLine(",");
                    }
                    else
                    {
                        schema.AppendLine();
                    }
                }
                
                // Close properties object
                schema.AppendLine("  },");
                
                // Add required properties if any
                if (requiredProperties.Count > 0)
                {
                    schema.AppendLine("  \"required\": [");
                    for (int i = 0; i < requiredProperties.Count; i++)
                    {
                        schema.Append($"    \"{requiredProperties[i]}\"");
                        if (i < requiredProperties.Count - 1)
                        {
                            schema.AppendLine(",");
                        }
                        else
                        {
                            schema.AppendLine();
                        }
                    }
                    schema.AppendLine("  ],");
                }
                
                // Add additionalProperties constraint
                schema.AppendLine("  \"additionalProperties\": false");
                
                // Close schema object
                schema.AppendLine("}");
                
                return schema.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JSON schema for type {Type}", type.Name);
                throw new InvalidOperationException($"Error generating JSON schema for type {type.Name}", ex);
            }
        }
        
        /// <inheritdoc/>
        public string GenerateExampleJson<T>()
        {
            return GenerateExampleJson(typeof(T));
        }
        
        /// <inheritdoc/>
        public string GenerateExampleJson(Type type)
        {
            try
            {
                var example = new Dictionary<string, object>();
                
                var properties = type.GetProperties()
                    .Where(p => !p.GetCustomAttributes<PromptIgnoreAttribute>().Any())
                    .ToList();
                
                foreach (var property in properties)
                {
                    var propertyAttribute = property.GetCustomAttribute<PromptPropertyAttribute>();
                    
                    // Skip properties with PromptIgnoreAttribute
                    if (property.GetCustomAttribute<PromptIgnoreAttribute>() != null)
                    {
                        continue;
                    }
                    
                    // Use example value if provided, otherwise generate a default example
                    object exampleValue;
                    if (propertyAttribute?.ExampleValue != null)
                    {
                        exampleValue = propertyAttribute.ExampleValue;
                    }
                    else
                    {
                        exampleValue = GenerateDefaultExampleValue(property.PropertyType);
                    }
                    
                    example[GetJsonPropertyName(property)] = exampleValue;
                }
                
                return JsonSerializer.Serialize(example, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating example JSON for type {Type}", type.Name);
                throw new InvalidOperationException($"Error generating example JSON for type {type.Name}", ex);
            }
        }
        
        /// <summary>
        /// Gets the JSON property name for a property
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The JSON property name</returns>
        private string GetJsonPropertyName(PropertyInfo property)
        {
            // Use System.Text.Json.Serialization.JsonPropertyNameAttribute if available
            var jsonPropertyNameAttribute = property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
            if (jsonPropertyNameAttribute != null)
            {
                return jsonPropertyNameAttribute.Name;
            }
            
            // We're not using Newtonsoft.Json to avoid additional dependencies
            
            // Default to camelCase property name
            return char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
        }
        
        /// <summary>
        /// Gets the JSON type for a .NET type
        /// </summary>
        /// <param name="type">The .NET type</param>
        /// <returns>The JSON type</returns>
        private string GetJsonType(Type type)
        {
            if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
            {
                return "string";
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || 
                     type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte))
            {
                return "integer";
            }
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                return "number";
            }
            else if (type == typeof(bool))
            {
                return "boolean";
            }
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                return "array";
            }
            else if (type.IsClass && type != typeof(string))
            {
                return "object";
            }
            else
            {
                return "string"; // Default to string for unknown types
            }
        }
        
        /// <summary>
        /// Adds type-specific constraints to the schema
        /// </summary>
        /// <param name="schema">The schema builder</param>
        /// <param name="property">The property</param>
        private void AddTypeSpecificConstraints(StringBuilder schema, PropertyInfo property)
        {
            var type = property.PropertyType;
            
            if (type == typeof(string))
            {
                // Add string-specific constraints
                // (e.g., minLength, maxLength, pattern)
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                     type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte))
            {
                // Add integer-specific constraints
                // (e.g., minimum, maximum, multipleOf)
            }
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                // Add number-specific constraints
                // (e.g., minimum, maximum, multipleOf)
            }
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                // Add array-specific constraints
                schema.AppendLine("      \"items\": {");
                
                Type elementType;
                if (type.IsArray)
                {
                    elementType = type.GetElementType();
                }
                else
                {
                    elementType = type.GetGenericArguments()[0];
                }
                
                schema.AppendLine($"        \"type\": \"{GetJsonType(elementType)}\"");
                schema.AppendLine("      }");
            }
        }
        
        /// <summary>
        /// Formats a value for use in JSON
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="type">The type of the value</param>
        /// <returns>The formatted value</returns>
        private string FormatJsonValue(object value, Type type)
        {
            if (value == null)
            {
                return "null";
            }
            
            if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
            {
                return $"\"{value}\"";
            }
            else if (type == typeof(bool))
            {
                return value.ToString().ToLowerInvariant();
            }
            else
            {
                return value.ToString();
            }
        }
        
        /// <summary>
        /// Generates a default example value for a type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>A default example value</returns>
        private object GenerateDefaultExampleValue(Type type)
        {
            // Special cases for specific types to provide better examples
            if (type.Name == "LineItemResponse")
            {
                return new Dictionary<string, object>
                {
                    ["description"] = "Kitchen renovation labor",
                    ["quantity"] = 1,
                    ["unitPrice"] = 45.00,
                    ["totalPrice"] = 45.00,
                    ["vatRate"] = 21.00
                };
            }
            else if (type.Name == "InvoiceLineItemsResponse")
            {
                var lineItem = new Dictionary<string, object>
                {
                    ["description"] = "Kitchen renovation labor",
                    ["quantity"] = 40,
                    ["unitPrice"] = 45.00,
                    ["totalPrice"] = 1800.00,
                    ["vatRate"] = 21.00
                };
                
                return new Dictionary<string, object>
                {
                    ["lineItems"] = new List<object> { lineItem },
                    ["paymentTerms"] = "Net 30 days",
                    ["paymentMethod"] = "Bank transfer",
                    ["paymentReference"] = "INV-2025-0042",
                    ["notes"] = "Installation completed on April 10, 2025",
                    ["confidence"] = 0.95
                };
            }
            
            if (type == typeof(string))
            {
                return "Example string";
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                     type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte))
            {
                return 42;
            }
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                return 42.42;
            }
            else if (type == typeof(bool))
            {
                return true;
            }
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
            else if (type == typeof(Guid))
            {
                return Guid.NewGuid().ToString();
            }
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                // Create an example array with one item
                Type elementType;
                if (type.IsArray)
                {
                    elementType = type.GetElementType();
                }
                else
                {
                    elementType = type.GetGenericArguments()[0];
                }
                
                var list = new List<object> { GenerateDefaultExampleValue(elementType) };
                return list;
            }
            else if (type.IsClass && type != typeof(string))
            {
                // For complex objects, create a new instance and populate its properties
                var instance = new Dictionary<string, object>();
                
                var properties = type.GetProperties()
                    .Where(p => !p.GetCustomAttributes<PromptIgnoreAttribute>().Any())
                    .ToList();
                
                foreach (var property in properties)
                {
                    var propertyAttribute = property.GetCustomAttribute<PromptPropertyAttribute>();
                    
                    // Skip properties with PromptIgnoreAttribute
                    if (property.GetCustomAttribute<PromptIgnoreAttribute>() != null)
                    {
                        continue;
                    }
                    
                    // Use example value if provided, otherwise generate a default example
                    object exampleValue;
                    if (propertyAttribute?.ExampleValue != null)
                    {
                        exampleValue = propertyAttribute.ExampleValue;
                    }
                    else
                    {
                        exampleValue = GenerateDefaultExampleValue(property.PropertyType);
                    }
                    
                    instance[GetJsonPropertyName(property)] = exampleValue;
                }
                
                return instance;
            }
            else
            {
                return null;
            }
        }
    }
}
