# Construction Fund Withdrawal Proof Validator

A comprehensive document validation system for construction fund (bouwdepot) withdrawals that uses AI to analyze and validate invoices, receipts, and quotations as proof for withdrawal requests.

## Recent Changes

### Prompt Service Integration (April 21, 2025)

- Fixed prompt service integration with pipeline steps:
  - Updated all pipeline steps to use the PromptService instead of loading prompt files directly
  - Changed steps to use prompt metadata names instead of file paths
  - Added proper dependency injection of PromptService in all steps
  - Ensured consistent naming between prompt metadata and step references
  - Eliminated errors caused by mismatched prompt paths and names

### Conversation Context Preservation (April 21, 2025)

- Added conversation context preservation across LLM calls:
  - Created ConversationContext class to maintain conversation history
  - Updated ILLMProvider interface to support conversation history
  - Modified GeminiClient to maintain and pass conversation history
  - Updated ValidationPipeline to initialize and pass conversation context
  - Enhanced API response with detailed validation information
  - Improved LLM accuracy by providing context from previous steps

### Enhanced API Response (April 21, 2025)

- Enhanced API response with more detailed validation information:
  - Added detailed processing steps with status, description, and timestamp
  - Added detailed AI model usage information
  - Added conversation history to the API response
  - Updated pipeline steps to use the correct prompt template paths
  - Improved transparency and auditability of the validation process

### Dynamic Schema Generation (April 21, 2025)

- Added dynamic JSON schema generation from POCO classes:
  - Created custom attributes (PromptSchemaAttribute, PromptPropertyAttribute, PromptIgnoreAttribute) for annotating POCO classes
  - Implemented JsonSchemaGenerator to generate JSON schemas from annotated classes
  - Created DynamicPromptService to build prompts with dynamically generated schemas
  - Updated DocumentStructureExtractionStep to use dynamic schemas for line items extraction
  - Added VatRate property to LineItemResponse to match InvoiceLineItem model
  - Ensures that prompt JSON output always matches POCO classes, preventing deserialization errors

### Single Prompt Pipeline Architecture (April 21, 2025)

- Modified validation pipeline to ensure each step can only do one prompt:
  - Updated IValidationPipelineStep interface to include prompt-related properties and methods
  - Modified ValidationPipeline to handle prompt execution
  - Updated ILLMProvider interface to support Type-based prompt execution
  - Refactored all pipeline steps to comply with the new interface
  - Improved adherence to SOLID principles with better separation of concerns

### Pipeline Architecture and Receipt Support (April 21, 2025)

- Added support for receipt validation alongside invoices:
  - Updated document-type-verification.json prompt to detect receipts
  - Added isReceipt property to DocumentTypeVerificationResponse
  - Modified validation logic to accept both invoices and receipts

- Refactored validation service to use a builder pattern with pipeline steps:
  - Created IValidationPipelineStep interface for individual validation steps
  - Created IValidationPipeline interface for the validation pipeline
  - Implemented ValidationPipeline to execute steps in order
  - Split validation logic into separate pipeline steps:
    - LanguageDetectionStep
    - DocumentTypeVerificationStep
    - DocumentStructureExtractionStep
    - FraudDetectionStep
    - BouwdepotEligibilityStep
    - AuditReportGenerationStep
  - Added DocumentType property to ValidationContext to track document type
  - Renamed InvoiceValidationService to DocumentValidationService
  - Updated ServiceCollectionExtensions to register pipeline components
  - Removed PDF to image conversion logic (no longer needed)

### Prompt Improvements (April 21, 2025)

- Enhanced all prompt templates to ensure consistent JSON responses:
  - Added explicit JSON formatting instructions to all prompts
  - Added examples to prompts that were missing them
  - Standardized JSON response format requirements across all prompts
  - Created a dedicated language-detection.json prompt file
  - Updated ComprehensiveDocumentIntelligence.json with clearer output formatting instructions

### Build Fix (April 6, 2025)

- Fixed build errors in InvoiceValidationHelpers.cs
- Updated MapToValidationResult method to match ValidationResult class properties
- Fixed property names and structure to align with domain models

### Invoice Validation Implementation (April 6, 2025)

- Implemented comprehensive `InvoiceValidationService` for construction fund invoice validation
- Created supporting classes for validation context, responses, and helper methods
- Implemented a structured workflow for invoice validation:
  - Language detection
  - Document type verification
  - Invoice structure extraction
  - Fraud detection
  - Bouwdepot eligibility validation
  - Audit report generation
- Updated prompt templates to better support the validation workflow
- Added CHANGELOG.md file to track project changes

### README Update (April 5, 2025)

- Updated the README.md file to include a section on logging conventions.

### Prompt Template System Improvements (May 2025)

- Implemented a structured prompt template system for all AI interactions
- Created standardized JSON-based prompt templates for different analysis types:
  - Document type verification
  - Invoice header extraction
  - Invoice parties extraction
  - Invoice line items analysis
  - Fraud detection
  - Home improvement validation
- Improved prompt management with centralized template service
- Added fallback mechanisms for backward compatibility

### Architecture Improvements (May 2025)

- Implemented Domain-Driven Design (DDD) architecture to better separate concerns
- Created a clean domain layer with well-defined interfaces and models
- Simplified the invoice analysis process with a streamlined pipeline approach
- Improved error handling and logging throughout the application
- Added comprehensive documentation to all interfaces and models

### Code Refactoring (May 2025)

- Simplified the invoice analysis process by introducing a step-by-step pipeline:
  1. Language detection
  2. Document layout analysis
  3. Invoice structure extraction
  4. Fraud detection
  5. Bouwdepot rules validation
  6. Audit report generation

- Created domain interfaces for AI services:
  - `ILanguageDetector`: Detects the primary language of invoice documents
  - `IDocumentLayoutAnalyzer`: Analyzes the layout of invoice documents
  - `IInvoiceStructureExtractor`: Extracts structured data from invoice documents
  - `IFraudDetector`: Detects potential fraud in invoice documents
  - `IBouwdepotRulesValidator`: Validates invoices against Bouwdepot-specific rules
  - `IAuditReportService`: Generates and persists audit reports

- Simplified API response models:
  - Created a clean `SimplifiedValidationResponse` model for API responses
  - Removed redundant and unused properties from response models
  - Improved documentation of response models

### Bug Fixes (April 2025)

- Fixed build errors by updating model classes to match service implementation
- Added missing properties to TechnicalDetails class
- Updated ProcessingStep property names to align with service usage

### Code Refactoring (April 2025)

The codebase has been refactored to improve maintainability by splitting large model files into smaller, more focused files:

1. **EnhancedValidationModels.cs** has been split into:
   - Models/Enhanced/Content/ContentSummary.cs
   - Models/Enhanced/Content/ConfidenceFactor.cs
   - Models/Enhanced/Fraud/FraudRiskLevel.cs
   - Models/Enhanced/Fraud/FraudIndicatorCategory.cs
   - Models/Enhanced/Fraud/FraudIndicator.cs
   - Models/Enhanced/Fraud/DocumentIntegrityAssessment.cs
   - Models/Enhanced/Fraud/ContentAssessment.cs
   - Models/Enhanced/Fraud/FraudDetection.cs
   - Models/Enhanced/Fraud/FraudDetectionResult.cs
   - Models/Enhanced/Vendors/VendorInsights.cs
   - Models/Enhanced/Audit/ProcessingEvent.cs
   - Models/Enhanced/Audit/RegulationReference.cs
   - Models/Enhanced/Audit/RuleAssessment.cs
   - Models/Enhanced/Audit/ValidationThresholdConfiguration.cs
   - Models/Enhanced/Audit/AuditReport.cs
   - Models/Enhanced/Security/DigitalSignature.cs
   - Models/Enhanced/Security/SignatureVerificationStatus.cs

2. **ConsolidatedAuditReport.cs** has been split into:
   - Models/Audit/ConsolidatedAuditReport.cs
   - Models/Audit/VendorBankDetails.cs
   - Models/Audit/DocumentValidation.cs
   - Models/Audit/BouwdepotEligibility.cs
   - Models/Audit/ValidatedLineItem.cs
   - Models/Audit/DetailedAnalysisSection.cs
   - Models/Audit/AuditTrail.cs
   - Models/Audit/LegalReference.cs
   - Models/Audit/RuleApplication.cs
   - Models/Audit/ProcessingStep.cs
   - Models/Audit/TechnicalDetails.cs

This refactoring follows SOLID principles, particularly the Single Responsibility Principle, making the codebase more maintainable and easier to understand.

## Logging Conventions

- We use Serilog for structured logging.
- Log levels are used as follows:
  - **Information**: General information about the application's operation.
  - **Debug**: Detailed information for debugging purposes.
  - **Warning**: Potential issues or non-critical errors.
  - **Error**: Errors that do not cause the application to crash.
  - **Fatal**: Errors that cause the application to crash.
- All log messages should be meaningful and provide enough context to understand the event being logged.
- Sensitive information should not be logged.

## Features

- AI-powered invoice analysis
- Fraud detection
- Bouwdepot eligibility validation
- Comprehensive audit reports
- Vendor profiling and verification
- Structured prompt template system for AI interactions

## Dynamic Schema Generation

The system now includes a powerful dynamic JSON schema generation feature that ensures prompt outputs always match the expected POCO classes:

### Custom Attributes

- **PromptSchemaAttribute**: Applied to classes to mark them for schema generation and provide a description.
- **PromptPropertyAttribute**: Applied to properties to include them in the schema with descriptions and requirements.
- **PromptIgnoreAttribute**: Applied to properties to exclude them from the schema.

### How It Works

1. POCO classes are annotated with the custom attributes to define their schema requirements.
2. The `JsonSchemaGenerator` uses reflection to analyze these classes and generate JSON schemas.
3. The `DynamicPromptService` combines these schemas with prompt templates to create comprehensive prompts.
4. When the LLM processes these prompts, it returns JSON that precisely matches the POCO structure.
5. This ensures successful deserialization and prevents errors during processing.

### Benefits

- **Consistency**: Ensures that LLM responses always match the expected data structure.
- **Maintainability**: When POCO classes are updated, the schemas automatically update too.
- **Reliability**: Reduces errors caused by mismatches between JSON output and POCO classes.
- **Documentation**: The schema serves as self-documenting code for the expected data structure.

## Prompt Structure and JSON Formatting

All AI prompts in the system follow a standardized structure to ensure consistent and reliable responses:

### Prompt Template Structure

Each prompt template consists of:

1. **Metadata**: Information about the prompt including name, version, description, last modified date, and author.
2. **Template**: The core prompt content with:
   - **Role**: Defines the AI's expertise and perspective
   - **Task**: Clearly states what the AI needs to accomplish
   - **Instructions**: Detailed steps for the AI to follow
   - **Output Format**: Specifies the expected JSON response structure
3. **Examples**: Sample inputs and expected outputs to guide the AI

### JSON Response Requirements

All prompts have been enhanced to ensure proper JSON formatting:

- Every prompt includes explicit instructions to return valid JSON
- Response fields are clearly defined with their expected data types
- Numeric values must be returned as numbers without quotes
- Boolean values must be returned as true/false without quotes
- String values must be properly quoted
- Arrays and objects must follow proper JSON syntax
- Null values are used for missing information rather than empty strings

### Available Prompt Templates

The system includes specialized prompts for different aspects of invoice validation:

- **Document Analysis**:
  - `document-type-verification.json`: Determines if a document is an invoice
  - `fraud-detection.json`: Detects visual signs of tampering or fraud
  - `line-item-analysis.json`: Analyzes line items for home improvement relevance
  - `multi-modal-home-improvement.json`: Validates eligibility for Bouwdepot financing
  - `language-detection.json`: Identifies the primary language of the document

- **Invoice Extraction**:
  - `invoice-header.json`: Extracts invoice number, dates, and amounts
  - `invoice-parties.json`: Extracts vendor and customer information
  - `invoice-line-items.json`: Extracts line items and payment details

- **Comprehensive Analysis**:
  - `ComprehensiveDocumentIntelligence.json`: Performs full document analysis including language, sentiment, topics, entities, and document type
