# BouwdepotInvoiceValidator

A modern application for validating invoices related to home improvement expenses.

## Project Overview

This application validates PDF invoices to determine if they're related to home improvement expenses. It uses Google's Gemini AI for intelligent validation and provides detailed results.

### Features

- PDF invoice upload and text extraction
- AI-powered validation for home improvement relevance
- Tampering detection for invoice security
- Detailed validation results with extracted invoice data

## Technical Architecture

The application is built with a decoupled architecture:

- **Backend**: ASP.NET Core Web API
- **Frontend**: React with Material-UI

This separation allows for independent development and scaling of each component.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (LTS version recommended)
- PowerShell

### Running the Application

The easiest way to run the application is using the provided PowerShell script:

1. Open PowerShell
2. Navigate to the project root directory
3. Run the script:

```powershell
.\run-app.ps1
```

This script will:
- Start the backend API (.NET Core)
- Check if frontend dependencies are installed, and install them if necessary
- Start the frontend React application
- Open separate windows for each application

### Manual Setup

If you prefer to run the applications manually:

#### Backend (API)

1. Navigate to the BouwdepotInvoiceValidator directory
2. Run the API with:
   ```
   dotnet run
   ```
3. The API will be available at https://localhost:7051
4. Swagger documentation is accessible at https://localhost:7051/swagger

#### Frontend (React)

1. Navigate to the BouwdepotInvoiceValidator.Client directory
2. Install dependencies:
   ```
   npm install
   ```
3. Start the development server:
   ```
   npm run dev
   ```
4. The frontend will be accessible at http://localhost:3000

## Development

- **API Development**: Modify controllers and services in the BouwdepotInvoiceValidator directory
- **Frontend Development**: Work on React components in the BouwdepotInvoiceValidator.Client/src directory
