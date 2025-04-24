# Script to run the backend ASP.NET Core application
# Assumes the frontend has been built and copied to wwwroot

Write-Host "Starting the backend application (which serves the frontend)..."
Write-Host "Access at: https://localhost:8080 (or configured port)"
Write-Host "Press Ctrl+C in the terminal to stop the application."

# Run the main web project
dotnet run --project .\BouwdepotInvoiceValidator\BouwdepotInvoiceValidator.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to run the application."
    exit 1
}

Write-Host "Application stopped."
exit 0
