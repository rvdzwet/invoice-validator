# Bouwdepot Invoice Validator

A C# Blazor Server application for validating invoices for home improvement expenses using PDF text extraction and Gemini Flash 2.0 AI integration.

## Features

- Upload and process PDF invoice files
- Extract invoice data including vendor, date, amounts, and line items
- Detect signs of PDF tampering or manipulation
- Validate if expenses are related to home improvement using Gemini Flash 2.0 AI
- Check for potential fraud indicators in the invoice
- User-friendly interface with real-time validation feedback

## Architecture

The application follows SOLID principles and is organized into several key components:

### Models
- `Invoice`: Represents extracted invoice data
- `InvoiceLineItem`: Represents individual line items from an invoice
- `ValidationResult`: Holds validation results with status and issues
- `ValidationIssue`: Represents specific validation problems

### Services
- `IPdfExtractionService`: Interface for PDF text extraction and tampering detection
- `PdfExtractionService`: Implementation using iText7 library
- `IGeminiService`: Interface for AI-based validation with Gemini
- `GeminiService`: Implementation using Google Cloud AI Platform
- `IInvoiceValidationService`: Orchestrates the validation workflow
- `InvoiceValidationService`: Combines PDF extraction and AI validation

### Components
- `InvoiceUpload`: Main UI component for uploading invoices
- `InvoiceDetails`: Displays extracted invoice information

## Setup and Configuration

1. Clone the repository
2. Set up your Google Cloud credentials and Gemini Flash 2.0 API key:
   - Add your API key to `appsettings.json` in the "Gemini" section
   - Configure the "ProjectId", "Location", and "ModelId" if needed

3. Build and run the application:
```bash
cd BouwdepotInvoiceValidator
dotnet build
dotnet run
```

4. Access the application at `http://localhost:5105`

## Technologies Used

- .NET 9.0
- Blazor Server
- iText7 for PDF processing
- Google Cloud AIPlatform SDK for Gemini integration
- Bootstrap for UI styling

## Note About PDF Processing

The PDF extraction uses regular expressions to identify common invoice patterns. For production use, the extraction logic may need to be customized based on the specific invoice formats used by your vendors.

## Security Considerations

- The application validates PDF files for tampering, but additional security measures should be implemented in a production environment
- API keys should be properly secured through environment variables or a secrets manager in production
