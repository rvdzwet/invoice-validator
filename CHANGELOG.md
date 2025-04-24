# Change Log

All notable changes to this project will be documented in this file.

## [1.3.0] - 2025-04-21 - Construction Fund Withdrawal Proof Validator

### Major Architectural Changes

- **Refactored application to validate withdrawal proofs instead of just invoices:**
  - Renamed key interfaces and services for better domain alignment
  - Added support for multiple document types (invoices, receipts, quotations)
  - Created comprehensive validation for construction fund withdrawal proofs
  - Added new models for construction activities and withdrawal requests

- **Enhanced construction fund validation:**
  - Created structured activity classification by construction category
  - Added support for identifying construction activities in documents
  - Improved eligibility determination with detailed activity summaries
  - Enhanced audit report generation with comprehensive validation details

- **Added new interfaces and implementations:**
  - Created `IWithdrawalProofValidationService` interface
  - Implemented `WithdrawalProofValidationService` service
  - Created `WithdrawalProofController` for API access
  - Added `ConstructionActivity` model for activity classification
  - Implemented `WithdrawalRequest` model for contextual requests

- **Enhanced pipeline steps:**
  - Updated `DocumentTypeVerificationStep` to accept multiple document types
  - Improved `BouwdepotEligibilityStep` with detailed construction activity analysis
  - Enhanced `AuditReportGenerationStep` with comprehensive construction activity reporting
  - Added helper classes to improve code organization and maintainability

- **Enhanced audit reporting:**
  - Generated detailed construction activity summaries by category
  - Improved validation summaries with activity-based eligibility reasoning
  - Enhanced audit trails with comprehensive construction context

## [1.2.0] - 2025-04-21 - Prompt Service Integration

- Fixed prompt service integration with pipeline steps:
  - Updated all pipeline steps to use the PromptService instead of loading prompt files directly
  - Changed steps to use prompt metadata names instead of file paths
  - Added proper dependency injection of PromptService in all steps
  - Ensured consistent naming between prompt metadata and step references
  - Eliminated errors caused by mismatched prompt paths and names

## [1.1.0] - 2025-04-21 - Conversation Context Preservation

- Added conversation context preservation across LLM calls:
  - Created ConversationContext class to maintain conversation history
  - Updated ILLMProvider interface to support conversation history
  - Modified GeminiClient to maintain and pass conversation history
  - Updated ValidationPipeline to initialize and pass conversation context
  - Enhanced API response with detailed validation information
  - Improved LLM accuracy by providing context from previous steps

## [1.0.0] - 2025-04-21 - Dynamic Schema Generation

- Added dynamic JSON schema generation from POCO classes:
  - Created custom attributes (PromptSchemaAttribute, PromptPropertyAttribute, PromptIgnoreAttribute) for annotating POCO classes
  - Implemented JsonSchemaGenerator to generate JSON schemas from annotated classes
  - Created DynamicPromptService to build prompts with dynamically generated schemas
  - Updated DocumentStructureExtractionStep to use dynamic schemas for line items extraction
  - Added VatRate property to LineItemResponse to match InvoiceLineItem model
  - Ensures that prompt JSON output always matches POCO classes, preventing deserialization errors

## [0.9.0] - 2025-04-21 - Single Prompt Pipeline Architecture

- Modified validation pipeline to ensure each step can only do one prompt:
  - Updated IValidationPipelineStep interface to include prompt-related properties and methods
  - Modified ValidationPipeline to handle prompt execution
  - Updated ILLMProvider interface to support Type-based prompt execution
  - Refactored all pipeline steps to comply with the new interface
  - Improved adherence to SOLID principles with better separation of concerns

## [0.8.0] - 2025-04-21 - Pipeline Architecture and Receipt Support

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
