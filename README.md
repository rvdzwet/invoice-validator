# Bouwdepot Invoice Validator

A comprehensive invoice validation system for Bouwdepot that uses AI to analyze and validate construction invoices.

## Recent Changes

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

## Features

- AI-powered invoice analysis
- Fraud detection
- Bouwdepot eligibility validation
- Comprehensive audit reports
- Vendor profiling and verification
