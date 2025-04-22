# Construction Fund Withdrawal Proof Validator (Bouwdepot Factuur Thing)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive document validation system for construction fund (bouwdepot) withdrawals in the Netherlands. It uses AI (configurable via Gemini or Ollama) to analyze and validate invoices, receipts, and quotations submitted as proof for withdrawal requests.

## Features

*   AI-powered analysis of invoices, receipts, and quotations.
*   Validation against Bouwdepot eligibility rules.
*   Fraud detection capabilities.
*   Structured data extraction from documents.
*   Configurable AI provider (Google Gemini or local Ollama).
*   Comprehensive audit trails and reporting.
*   React-based frontend for document upload and results visualization.

## Getting Started

Follow these instructions to get the project running locally for development or testing.

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 or later recommended - check `global.json` or `.csproj` files)
*   [Node.js and npm](https://nodejs.org/) (LTS version recommended)
*   (Optional) An IDE like Visual Studio or VS Code.
*   Access to an AI provider:
    *   **Gemini:** A Google Cloud Project ID and API Key.
    *   **Ollama:** A running Ollama instance (`http://localhost:11434` by default) with required models downloaded (e.g., `llama3`, `llava`).

### Installation

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd bouwdepot-factuur-thing
    ```
    *(Replace `<repository-url>` with the actual URL)*

2.  **Backend (.NET API):**
    *   Navigate to the root directory (`bouwdepot-factuur-thing`).
    *   Restore dependencies:
        ```bash
        dotnet restore ROMARS_CONSTRUCTION-FUND-VALIDATOR.sln
        ```
    *   Configure AI Provider: Edit `BouwdepotInvoiceValidator/appsettings.Development.json` (or use User Secrets / Environment Variables) with your chosen provider's details (see [Configuration](#configuration) below). **Do not commit secrets!**

3.  **Frontend (React Client):**
    *   Navigate to the client directory:
        ```bash
        cd BouwdepotInvoiceValidator.Client
        ```
    *   Install dependencies:
        ```bash
        npm install
        ```
        *(Or `yarn install` if you prefer yarn)*

## Running the Application

1.  **Start the Backend:**
    *   From the `BouwdepotInvoiceValidator` directory:
        ```bash
        dotnet run --project BouwdepotInvoiceValidator.csproj
        ```
    *   Alternatively, run/debug the `BouwdepotInvoiceValidator` project from your IDE (Visual Studio / Rider / VS Code with C# Dev Kit).
    *   The API will typically start on `https://localhost:7XXX` or `http://localhost:5XXX`. Note the exact URL from the console output.

2.  **Start the Frontend:**
    *   From the `BouwdepotInvoiceValidator.Client` directory:
        ```bash
        npm run dev
        ```
        *(Or `yarn dev`)*
    *   The frontend development server usually starts on `http://localhost:5173`.
    *   Open your browser to `http://localhost:5173`. The frontend is configured (via `vite.config.ts`) to proxy API requests to the running backend.

## Configuration

The application supports multiple Large Language Model (LLM) providers. Configure which provider to use and its settings in `BouwdepotInvoiceValidator/appsettings.json` (and override locally in `BouwdepotInvoiceValidator/appsettings.Development.json` or via environment variables).

### Selecting the Provider

Set the `LlmProvider` property to either `"Gemini"` or `"Ollama"`:

```json
{
  "Logging": { /* ... */ },
  "AllowedHosts": "*",
  "LlmProvider": "Gemini" // Or "Ollama"
  // ... Provider-specific sections below
}
```

### Gemini Configuration

If `LlmProvider` is `"Gemini"`, configure the `Gemini` section:

```json
{
  // ... other settings
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY", // Use User Secrets or Environment Variables
    "ProjectId": "YOUR_GOOGLE_CLOUD_PROJECT_ID",
    "Location": "europe-west1",
    "DefaultTextModel": "gemini-1.5-flash-latest",
    "DefaultMultimodalModel": "gemini-1.5-flash-latest",
    "TimeoutSeconds": 60,
    "Temperature": 0.5,
    "TopP": 0.95,
    "TopK": 40,
    "CandidateCount": 1
  }
}
```
**Important:** Use .NET User Secrets (`dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY"`) or environment variables for `ApiKey` in development. Do not commit your API key.

### Ollama Configuration

If `LlmProvider` is `"Ollama"`, configure the `Ollama` section:

```json
{
  // ... other settings
  "Ollama": {
    "ApiBaseUrl": "http://localhost:11434", // Default Ollama URL
    "DefaultTextModel": "llama3",
    "DefaultMultimodalModel": "llava", // Ensure this model is pulled in Ollama
    "TimeoutSeconds": 120,
    "KeepAlive": "5m",
    "Temperature": null, // Optional: 0.7
    "TopP": null,      // Optional: 0.9
    "TopK": null       // Optional: 50
  }
}
```
Ensure your Ollama instance is running and the specified models are available (`ollama pull llama3`, `ollama pull llava`).

## Logging Conventions

*   Serilog is used for structured logging.
*   Standard log levels (Information, Debug, Warning, Error, Fatal) are used.
*   Log messages should be meaningful and provide context.
*   Sensitive information must not be logged.

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details on how to submit bug reports, feature requests, and pull requests.

*(Remember to update the links in `CONTRIBUTING.md` to point to the actual repository location once created on GitHub)*

## Code of Conduct

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
