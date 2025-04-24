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
                    schema.AppendLine($"  \"description\": \"{schemaAttribute.Description}\\n\\nThis schema was generated using prompt annotations:\\n- [PromptSchema] marks classes for schema generation and provides overall description\\n- [PromptProperty] marks properties with descriptions, required state, defaults, and examples\\n- [PromptIgnore] marks properties to exclude from the schema\",");
                }
                else
                {
                    schema.AppendLine($"  \"description\": \"This schema was generated using prompt annotations:\\n- [PromptSchema] marks classes for schema generation and provides overall description\\n- [PromptProperty] marks properties with descriptions, required state, defaults, and examples\\n- [PromptIgnore] marks properties to exclude from the schema\",");
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
            // Check if type is nullable
            bool isNullable = false;
            Type underlyingType = type;
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;
                underlyingType = Nullable.GetUnderlyingType(type);
            }
            
            string baseType;
            if (underlyingType == typeof(string) || underlyingType == typeof(Guid) || underlyingType == typeof(DateTime) || 
                underlyingType == typeof(DateTimeOffset) || underlyingType == typeof(TimeSpan))
            {
                baseType = "string";
            }
            else if (underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short) || 
                     underlyingType == typeof(byte) || underlyingType == typeof(uint) || underlyingType == typeof(ulong) || 
                     underlyingType == typeof(ushort) || underlyingType == typeof(sbyte))
            {
                baseType = "integer";
            }
            else if (underlyingType == typeof(float) || underlyingType == typeof(double) || underlyingType == typeof(decimal))
            {
                baseType = "number";
            }
            else if (underlyingType == typeof(bool))
            {
                baseType = "boolean";
            }
            else if (underlyingType.IsArray || (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                baseType = "array";
            }
            else if (underlyingType.IsClass && underlyingType != typeof(string))
            {
                baseType = "object";
            }
            else
            {
                baseType = "string"; // Default to string for unknown types
            }
            
            return baseType;
        }
        
        /// <summary>
        /// Checks if a type is nullable
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type is nullable</returns>
        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        /// <summary>
        /// Adds type-specific constraints to the schema
        /// </summary>
        /// <param name="schema">The schema builder</param>
        /// <param name="property">The property</param>
        private void AddTypeSpecificConstraints(StringBuilder schema, PropertyInfo property)
        {
            var type = property.PropertyType;
            bool isNullable = IsNullableType(type);
            
            // If the property is nullable, add null to the type array
            if (isNullable || type.IsClass || type.IsInterface)
            {
                // Removing trailing comma if exists in the previous line
                string prevLine = schema.ToString().TrimEnd();
                if (prevLine.EndsWith(","))
                {
                    schema.Remove(schema.Length - (prevLine.Length - prevLine.LastIndexOf(',')), 1);
                    schema.AppendLine();
                }
                
                // Replace "type": "X" with "type": ["X", "null"]
                schema.AppendLine("      \"type\": [");
                schema.AppendLine($"        \"{GetJsonType(type)}\",");
                schema.AppendLine("        \"null\"");
                schema.AppendLine("      ],");
            }
            
            if (type == typeof(string) || (isNullable && Nullable.GetUnderlyingType(type) == typeof(string)))
            {
                // Add string-specific constraints
                // (e.g., minLength, maxLength, pattern)
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                     type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
                     (isNullable && Nullable.GetUnderlyingType(type) != null && 
                      (Nullable.GetUnderlyingType(type) == typeof(int) || Nullable.GetUnderlyingType(type) == typeof(long) ||
                       Nullable.GetUnderlyingType(type) == typeof(short) || Nullable.GetUnderlyingType(type) == typeof(byte) ||
                       Nullable.GetUnderlyingType(type) == typeof(uint) || Nullable.GetUnderlyingType(type) == typeof(ulong) ||
                       Nullable.GetUnderlyingType(type) == typeof(ushort) || Nullable.GetUnderlyingType(type) == typeof(sbyte))))
            {
                // Add integer-specific constraints
                // (e.g., minimum, maximum, multipleOf)
            }
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal) ||
                    (isNullable && Nullable.GetUnderlyingType(type) != null && 
                     (Nullable.GetUnderlyingType(type) == typeof(float) || Nullable.GetUnderlyingType(type) == typeof(double) ||
                      Nullable.GetUnderlyingType(type) == typeof(decimal))))
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
                
                // Set the type of array items
                if (IsNullableType(elementType) || elementType.IsClass || elementType.IsInterface)
                {
                    schema.AppendLine("        \"type\": [");
                    schema.AppendLine($"          \"{GetJsonType(elementType)}\",");
                    schema.AppendLine("          \"null\"");
                    schema.AppendLine("        ]");
                }
                else
                {
                    schema.AppendLine($"        \"type\": \"{GetJsonType(elementType)}\"");
                }
                
                // Handle arrays of complex objects
                if (elementType.IsClass && elementType != typeof(string))
                {
                    // Generate schema for array items that are objects
                    schema.AppendLine(",        \"properties\": {");
                    var requiredProperties = new List<string>();
                    GenerateObjectSchema(schema, elementType, 10, requiredProperties); // Deeper indentation for array items
                    schema.AppendLine("        }");
                    
                    // Add required properties for array items if any
                    if (requiredProperties.Count > 0)
                    {
                        schema.AppendLine(",        \"required\": [");
                        for (int i = 0; i < requiredProperties.Count; i++)
                        {
                            schema.Append($"          \"{requiredProperties[i]}\"");
                            if (i < requiredProperties.Count - 1)
                            {
                                schema.AppendLine(",");
                            }
                            else
                            {
                                schema.AppendLine();
                            }
                        }
                        schema.AppendLine("        ]");
                    }
                }
                
                schema.AppendLine("      }");
            }
            else if (type.IsClass && type != typeof(string))
            {
                // Handle nested objects
                schema.AppendLine("      \"type\": \"object\",");
                schema.AppendLine("      \"properties\": {");
                
                // Generate nested object schema
                var requiredProperties = new List<string>();
                GenerateObjectSchema(schema, type, 8, requiredProperties);
                
                schema.AppendLine("      }");
                
                // Add required properties for nested objects if any
                if (requiredProperties.Count > 0)
                {
                    schema.AppendLine(",      \"required\": [");
                    for (int i = 0; i < requiredProperties.Count; i++)
                    {
                        schema.Append($"        \"{requiredProperties[i]}\"");
                        if (i < requiredProperties.Count - 1)
                        {
                            schema.AppendLine(",");
                        }
                        else
                        {
                            schema.AppendLine();
                        }
                    }
                    schema.AppendLine("      ]");
                }
            }
        }
        
        /// <summary>
        /// Generates schema properties for a nested object type
        /// </summary>
        /// <param name="schema">The schema builder</param>
        /// <param name="type">The type to generate schema for</param>
        /// <param name="indentLevel">Number of spaces to indent</param>
        /// <param name="requiredProperties">List to add required property names to</param>
        private void GenerateObjectSchema(StringBuilder schema, Type type, int indentLevel, List<string> requiredProperties)
        {
            var properties = type.GetProperties()
                .Where(p => !p.GetCustomAttributes<PromptIgnoreAttribute>().Any())
                .ToList();
                
            string indent = new string(' ', indentLevel);
            
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var propertyAttribute = property.GetCustomAttribute<PromptPropertyAttribute>();
                
                // Skip properties with PromptIgnoreAttribute
                if (property.GetCustomAttribute<PromptIgnoreAttribute>() != null)
                {
                    continue;
                }
                
                // Check if property is required and add to required properties list
                if (propertyAttribute?.Required ?? false)
                {
                    requiredProperties.Add(GetJsonPropertyName(property));
                }
                
                // Add property schema
                schema.AppendLine($"{indent}\"{GetJsonPropertyName(property)}\": {{");
                
                // If nullable, use type array with null
                if (IsNullableType(property.PropertyType) || property.PropertyType.IsClass || property.PropertyType.IsInterface)
                {
                    schema.AppendLine($"{indent}  \"type\": [");
                    schema.AppendLine($"{indent}    \"{GetJsonType(property.PropertyType)}\",");
                    schema.AppendLine($"{indent}    \"null\"");
                    schema.AppendLine($"{indent}  ],");
                }
                else
                {
                    schema.AppendLine($"{indent}  \"type\": \"{GetJsonType(property.PropertyType)}\",");
                }
                
                // Add property description if available
                if (propertyAttribute != null && !string.IsNullOrEmpty(propertyAttribute.Description))
                {
                    schema.AppendLine($"{indent}  \"description\": \"{propertyAttribute.Description}\",");
                }
                
                // Add default value if available
                if (propertyAttribute?.DefaultValue != null)
                {
                    var defaultValue = FormatJsonValue(propertyAttribute.DefaultValue, property.PropertyType);
                    schema.AppendLine($"{indent}  \"default\": {defaultValue},");
                }
                
                // Add nested type-specific constraints with correct indentation
                var nestedSchema = new StringBuilder();
                AddNestedTypeSpecificConstraints(nestedSchema, property, indentLevel + 2);
                if (nestedSchema.Length > 0)
                {
                    schema.Append(nestedSchema);
                }
                
                // Close property definition
                schema.Append($"{indent}}}");
                
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
        }
        
        /// <summary>
        /// Adds type-specific constraints to a nested property
        /// </summary>
        /// <param name="schema">The schema builder</param>
        /// <param name="property">The property</param>
        /// <param name="indentLevel">Number of spaces to indent</param>
        private void AddNestedTypeSpecificConstraints(StringBuilder schema, PropertyInfo property, int indentLevel)
        {
            var type = property.PropertyType;
            string indent = new string(' ', indentLevel);
            
            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                // Add array-specific constraints
                schema.AppendLine($"{indent}\"items\": {{");
                
                Type elementType;
                if (type.IsArray)
                {
                    elementType = type.GetElementType();
                }
                else
                {
                    elementType = type.GetGenericArguments()[0];
                }
                
                // Handle nullable element types
                if (IsNullableType(elementType) || elementType.IsClass || elementType.IsInterface)
                {
                    schema.AppendLine($"{indent}  \"type\": [");
                    schema.AppendLine($"{indent}    \"{GetJsonType(elementType)}\",");
                    schema.AppendLine($"{indent}    \"null\"");
                    schema.AppendLine($"{indent}  ]");
                }
                else
                {
                    schema.AppendLine($"{indent}  \"type\": \"{GetJsonType(elementType)}\"");
                }
                
                // Handle arrays of complex objects
                if (elementType.IsClass && elementType != typeof(string))
                {
                    schema.AppendLine($"{indent}  ,\"properties\": {{");
                    var requiredProperties = new List<string>();
                    GenerateObjectSchema(schema, elementType, indentLevel + 4, requiredProperties);
                    schema.AppendLine($"{indent}  }}");
                    
                    // Add required properties for array items if any
                    if (requiredProperties.Count > 0)
                    {
                        schema.AppendLine($"{indent}  ,\"required\": [");
                        for (int i = 0; i < requiredProperties.Count; i++)
                        {
                            schema.Append($"{indent}    \"{requiredProperties[i]}\"");
                            if (i < requiredProperties.Count - 1)
                            {
                                schema.AppendLine(",");
                            }
                            else
                            {
                                schema.AppendLine();
                            }
                        }
                        schema.AppendLine($"{indent}  ]");
                    }
                }
                
                schema.AppendLine($"{indent}}}");
            }
            else if (type.IsClass && type != typeof(string))
            {
                schema.AppendLine($"{indent}\"type\": \"object\",");
                schema.AppendLine($"{indent}\"properties\": {{");
                
                // Generate nested object schema
                var requiredProperties = new List<string>();
                GenerateObjectSchema(schema, type, indentLevel + 2, requiredProperties);
                
                schema.AppendLine($"{indent}}}");
                
                // Add required properties for nested objects if any
                if (requiredProperties.Count > 0)
                {
                    schema.AppendLine($"{indent},\"required\": [");
                    for (int i = 0; i < requiredProperties.Count; i++)
                    {
                        schema.Append($"{indent}  \"{requiredProperties[i]}\"");
                        if (i < requiredProperties.Count - 1)
                        {
                            schema.AppendLine(",");
                        }
                        else
                        {
                            schema.AppendLine();
                        }
                    }
                    schema.AppendLine($"{indent}]");
                }
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
        /// <param name="depth">Current depth of object nesting to prevent infinite recursion</param>
        /// <returns>A default example value</returns>
        private object GenerateDefaultExampleValue(Type type, int depth = 0)
        {
            // Prevent infinite recursion with circular references
            if (depth > 10)
            {
                _logger.LogWarning("Maximum depth reached when generating example for type {Type}, returning null", type.Name);
                return null;
            }
            
            // Special cases for comprehensive models
            if (type.Name == "ComprehensiveWithdrawalProofResponse" || type.FullName == "BouwdepotInvoiceValidator.Domain.Services.ComprehensiveWithdrawalProofResponse")
            {
                return CreateComprehensiveWithdrawalProofExample();
            }
            // Special cases for specific types to provide better examples
            else if (type.Name == "LineItemResponse")
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
            else if (type.Name == "DocumentAnalysis" || type.FullName?.EndsWith(".DocumentAnalysis") == true)
            {
                return CreateDocumentAnalysisExample();
            }
            else if (type.Name == "VendorInfo" || type.FullName?.EndsWith(".VendorInfo") == true)
            {
                return CreateVendorInfoExample();
            }
            else if (type.Name == "CustomerInfo" || type.FullName?.EndsWith(".CustomerInfo") == true)
            {
                return CreateCustomerInfoExample();
            }
            else if (type.Name == "DocumentLineItem" || type.FullName?.EndsWith(".DocumentLineItem") == true)
            {
                return CreateDocumentLineItemExample();
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
                
                var list = new List<object> { GenerateDefaultExampleValue(elementType, depth + 1) };
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
                        exampleValue = GenerateDefaultExampleValue(property.PropertyType, depth + 1);
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
        
        /// <summary>
        /// Creates a comprehensive example for WithdrawalProofResponse
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateComprehensiveWithdrawalProofExample()
        {
            return new Dictionary<string, object>
            {
                ["documentAnalysis"] = CreateDocumentAnalysisExample(),
                ["constructionActivities"] = CreateConstructionActivitiesExample(),
                ["fraudAnalysis"] = CreateFraudAnalysisExample(),
                ["eligibilityDetermination"] = CreateEligibilityDeterminationExample(),
                ["auditSummary"] = CreateAuditSummaryExample()
            };
        }
        
        /// <summary>
        /// Creates an example for DocumentAnalysis
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateDocumentAnalysisExample()
        {
            return new Dictionary<string, object>
            {
                ["documentType"] = "Invoice",
                ["confidence"] = 98,
                ["language"] = "Dutch",
                ["documentNumber"] = "INV-2025-0042",
                ["issueDate"] = "2025-01-15",
                ["dueDate"] = "2025-02-15",
                ["totalAmount"] = 4235.00m,
                ["currency"] = "EUR",
                ["vendor"] = CreateVendorInfoExample(),
                ["customer"] = CreateCustomerInfoExample(),
                ["lineItems"] = new List<object> 
                {
                    CreateDocumentLineItemExample(),
                    CreateDocumentLineItemExample("Bathroom fixtures installation", 1, 1250.00m)
                },
                ["subtotal"] = 3500.00m,
                ["taxAmount"] = 735.00m,
                ["paymentTerms"] = "Net 30 days",
                ["notes"] = "Please reference invoice number with payment",
                ["multipleDocumentsDetected"] = false,
                ["detectedDocumentCount"] = 1
            };
        }
        
        /// <summary>
        /// Creates an example for VendorInfo
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateVendorInfoExample()
        {
            return new Dictionary<string, object>
            {
                ["name"] = "Amsterdam Construction B.V.",
                ["address"] = "Keizersgracht 123, 1015 CW Amsterdam",
                ["kvkNumber"] = "12345678",
                ["btwNumber"] = "NL123456789B01",
                ["contact"] = "info@amsterdamconstruction.example.com"
            };
        }
        
        /// <summary>
        /// Creates an example for CustomerInfo
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateCustomerInfoExample()
        {
            return new Dictionary<string, object>
            {
                ["name"] = "Jan de Vries",
                ["address"] = "Herengracht 458, 1017 CA Amsterdam"
            };
        }
        
        /// <summary>
        /// Creates an example for DocumentLineItem
        /// </summary>
        /// <param name="description">Optional description override</param>
        /// <param name="quantity">Optional quantity override</param>
        /// <param name="unitPrice">Optional unit price override</param>
        /// <returns>An example object</returns>
        private object CreateDocumentLineItemExample(string description = "Kitchen renovation labor", int quantity = 40, decimal unitPrice = 45.00m)
        {
            var totalPrice = quantity * unitPrice;
            var taxRate = 21.00m;
            var taxAmount = totalPrice * (taxRate / 100);
            
            return new Dictionary<string, object>
            {
                ["description"] = description,
                ["quantity"] = quantity,
                ["unitPrice"] = unitPrice,
                ["taxRate"] = taxRate,
                ["totalPrice"] = totalPrice,
                ["lineItemTaxAmount"] = taxAmount,
                ["lineItemTotalWithTax"] = totalPrice + taxAmount
            };
        }
        
        /// <summary>
        /// Creates an example for ConstructionActivities
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateConstructionActivitiesExample()
        {
            return new Dictionary<string, object>
            {
                ["isConstructionRelatedOverall"] = true,
                ["totalEligibleAmountCalculated"] = 3950.00m,
                ["percentageEligibleCalculated"] = 93.27m,
                ["detailedActivityAnalysis"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["originalDescription"] = "Kitchen renovation labor",
                        ["categorization"] = "Interior Renovation",
                        ["isEligible"] = true,
                        ["eligibleAmountForItem"] = 1800.00m,
                        ["ineligibleAmountForItem"] = 0.00m,
                        ["confidence"] = 0.96m,
                        ["reasoningForEligibility"] = "Kitchen renovation is a standard eligible construction activity"
                    },
                    new Dictionary<string, object>
                    {
                        ["originalDescription"] = "Bathroom fixtures installation",
                        ["categorization"] = "Plumbing",
                        ["isEligible"] = true,
                        ["eligibleAmountForItem"] = 1250.00m,
                        ["ineligibleAmountForItem"] = 0.00m,
                        ["confidence"] = 0.98m,
                        ["reasoningForEligibility"] = "Bathroom installation is a standard eligible construction activity"
                    }
                }
            };
        }
        
        /// <summary>
        /// Creates an example for FraudAnalysis
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateFraudAnalysisExample()
        {
            return new Dictionary<string, object>
            {
                ["fraudRiskLevel"] = "Low",
                ["fraudRiskScore"] = 0.15m,
                ["indicatorsFound"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["category"] = "Document Quality",
                        ["description"] = "Minor discrepancy in line item total calculation",
                        ["confidence"] = 0.65m,
                        ["implication"] = "Likely calculation rounding error rather than deliberate fraud"
                    }
                },
                ["summary"] = "Low fraud risk detected. Document appears legitimate with standard construction services and pricing."
            };
        }
        
        /// <summary>
        /// Creates an example for EligibilityDetermination
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateEligibilityDeterminationExample()
        {
            return new Dictionary<string, object>
            {
                ["overallStatus"] = "Eligible",
                ["decisionConfidenceScore"] = 0.95m,
                ["totalEligibleAmountDetermined"] = 3950.00m,
                ["totalIneligibleAmountDetermined"] = 285.00m,
                ["totalDocumentAmountReviewed"] = 4235.00m,
                ["rationaleCategory"] = "Standard Construction Services",
                ["rationaleSummary"] = "Invoice contains standard eligible construction activities for kitchen and bathroom renovation.",
                ["requiredActions"] = new List<string>
                {
                    "Approve withdrawal for eligible amount of €3,950.00"
                },
                ["notesForAuditor"] = "Vendor is known and has good compliance history"
            };
        }
        
        /// <summary>
        /// Creates an example for AuditSummary
        /// </summary>
        /// <returns>An example object</returns>
        private object CreateAuditSummaryExample()
        {
            return new Dictionary<string, object>
            {
                ["overallValidationSummary"] = "Document validated successfully as an eligible construction invoice with standard renovation activities.",
                ["keyFindingsSummary"] = "All major line items are for eligible construction activities. 93.27% of invoice total is eligible for withdrawal.",
                ["regulatoryComplianceNotes"] = new List<string>
                {
                    "Invoice complies with Dutch tax regulations",
                    "Vendor has valid KvK and BTW numbers"
                },
                ["auditSupportingEvidenceReferences"] = new List<string>
                {
                    "Valid invoice number format and sequence",
                    "Consistent with prior withdrawals for this project"
                }
            };
        }
    }
}
