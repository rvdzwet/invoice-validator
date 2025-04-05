# Bouwdepot Invoice Validator

A comprehensive invoice validation system for Bouwdepot that uses AI to analyze and validate construction invoices.

## Recent Changes

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
