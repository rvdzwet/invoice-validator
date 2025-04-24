# Contributing to Bouwdepot Invoice Validator

First off, thank you for considering contributing to the Bouwdepot Invoice Validator! It's people like you that make open source such a great community.

We welcome contributions of all kinds, from reporting bugs and suggesting enhancements to submitting code changes.

## Table Of Contents

*   [Code of Conduct](#code-of-conduct)
*   [How Can I Contribute?](#how-can-i-contribute)
    *   [Reporting Bugs](#reporting-bugs)
    *   [Suggesting Enhancements](#suggesting-enhancements)
    *   [Your First Code Contribution](#your-first-code-contribution)
    *   [Pull Requests](#pull-requests)
*   [Styleguides](#styleguides)
    *   [Git Commit Messages](#git-commit-messages)
    *   [C# Styleguide](#c-styleguide)
    *   [TypeScript/React Styleguide](#typescriptreact-styleguide)
*   [Development Setup](#development-setup)

## Code of Conduct

This project and everyone participating in it is governed by the [Bouwdepot Invoice Validator Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior.

## How Can I Contribute?

### Reporting Bugs

This section guides you through submitting a bug report. Following these guidelines helps maintainers and the community understand your report, reproduce the behavior, and find related reports.

*   **Use the GitHub Issues search** — check if the bug has already been reported.
*   If you're unable to find an open issue addressing the problem, **open a new one**. Be sure to include a **title and clear description**, as much relevant information as possible, and a **code sample or an executable test case** demonstrating the expected behavior that is not occurring.

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion, including completely new features and minor improvements to existing functionality.

*   **Use the GitHub Issues search** — check if the enhancement has already been suggested.
*   If you're unable to find an open issue addressing the enhancement, **open a new one**. Be sure to include a **title and clear description** of the enhancement, why it's needed, and any potential implementation ideas.

### Your First Code Contribution

Unsure where to begin contributing? You can start by looking through `good first issue` and `help wanted` issues:

*   [Good first issues](https://github.com/your-username/bouwdepot-factuur-thing/labels/good%20first%20issue) - issues which should only require a few lines of code, and a test or two.
*   [Help wanted issues](https://github.com/your-username/bouwdepot-factuur-thing/labels/help%20wanted) - issues which should be a bit more involved than `good first issue` issues.

*(Note: Replace `your-username/bouwdepot-factuur-thing` with the actual repository path once created)*

### Pull Requests

The process described here has several goals:

*   Maintain code quality
*   Fix problems that are important to users
*   Engage the community in working toward the best possible project
*   Enable a sustainable system for maintainers to review contributions

Please follow these steps to have your contribution considered by the maintainers:

1.  Fork the repository and create your branch from `main`.
2.  If you've added code that should be tested, add tests.
3.  If you've changed APIs, update the documentation.
4.  Ensure the test suite passes (`dotnet test` for backend, `npm test` or `yarn test` for frontend).
5.  Make sure your code lints (`dotnet format` for backend, `eslint` for frontend).
6.  Issue that pull request!

## Styleguides

### Git Commit Messages

*   Use the present tense ("Add feature" not "Added feature").
*   Use the imperative mood ("Move cursor to..." not "Moves cursor to...").
*   Limit the first line to 72 characters or less.
*   Reference issues and pull requests liberally after the first line.

### C# Styleguide

*   Follow the [.NET Runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md).
*   Use `dotnet format` to automatically format your code.

### TypeScript/React Styleguide

*   Follow the established code style in the `BouwdepotInvoiceValidator.Client` project.
*   Use ESLint and Prettier (if configured) to check and format your code. Run `npm run lint` or `yarn lint`.

## Development Setup

To get the project running locally:

**Prerequisites:**

*   [.NET SDK](https://dotnet.microsoft.com/download) (Check `global.json` or `.csproj` files for the specific version, likely .NET 8.0)
*   [Node.js and npm](https://nodejs.org/) (LTS version recommended)
*   (Optional) An IDE like Visual Studio or VS Code.

**Backend (.NET API):**

1.  Navigate to the `BouwdepotInvoiceValidator` directory.
2.  Restore dependencies: `dotnet restore ../ROMARS_CONSTRUCTION-FUND-VALIDATOR.sln` (or build the specific project)
3.  Configure `appsettings.Development.json` if needed (e.g., for external service keys - consider using User Secrets).
4.  Run the API: `dotnet run --project BouwdepotInvoiceValidator.csproj` (or run from your IDE). The API typically runs on `https://localhost:7XXX` or `http://localhost:5XXX`.

**Frontend (React Client):**

1.  Navigate to the `BouwdepotInvoiceValidator.Client` directory.
2.  Install dependencies: `npm install` (or `yarn install`)
3.  Run the development server: `npm run dev` (or `yarn dev`). The client typically runs on `http://localhost:5173`.

Ensure the backend API is running before starting the frontend. The frontend is usually configured in `vite.config.ts` or similar to proxy API requests to the backend server.
