# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2025-04-06

### Fixed
- Fixed build errors in InvoiceValidationHelpers.cs
- Updated MapToValidationResult method to match ValidationResult class properties
- Fixed property names: Id -> ValidationId, ProcessingInfo -> Processing, InvoiceData -> Invoice
- Fixed FraudAnalysis mapping to match ValidationResult class structure

## [1.0.0] - 2025-04-06

### Added
- Implemented comprehensive `InvoiceValidationService` using the `ILLMProvider` for construction fund invoice validation
- Created `ValidationContext` class to maintain state throughout the validation process
- Added `InvoiceValidationResponses` class for structured LLM responses
- Created `InvoiceValidationHelpers` class for utility methods
- Implemented a structured workflow with the following steps:
  - Language detection
  - Document type verification
  - Invoice structure extraction (header, parties, line items)
  - Fraud detection
  - Bouwdepot eligibility validation
  - Audit report generation

### Changed
- Updated all prompt templates to ensure they work effectively with the implementation
- Added payment reference field to invoice-line-items.json
- Updated all prompts with the current date
- Ensured prompts match the expected response formats

### Fixed
- Improved context preservation across different prompts in the validation workflow

## [0.9.0] - 2025-04-05

### Added
- Updated the README.md file to include a section on logging conventions.

## [0.8.0] - 2025-04-01

### Added
- Implemented Domain-Driven Design (DDD) architecture to better separate concerns
- Created a clean domain layer with well-defined interfaces and models
- Simplified the invoice analysis process with a streamlined pipeline approach
- Improved error handling and logging throughout the application
- Added comprehensive documentation to all interfaces and models

### Changed
- Simplified the invoice analysis process by introducing a step-by-step pipeline
- Created domain interfaces for AI services
- Simplified API response models

### Fixed
- Fixed build errors by updating model classes to match service implementation
- Added missing properties to TechnicalDetails class
- Updated ProcessingStep property names to align with service usage

## [0.7.0] - 2025-03-15

### Changed
- Refactored codebase to improve maintainability by splitting large model files into smaller, more focused files
