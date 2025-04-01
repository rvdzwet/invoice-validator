# Run-App.ps1
# Script to run both backend API and frontend React applications

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

function Write-Header {
    param (
        [Parameter(Mandatory = $true)]
        [string]$text
    )
    
    Write-Host ""
    Write-Host "====================================================" -ForegroundColor Cyan
    Write-Host $text -ForegroundColor Cyan
    Write-Host "====================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Start-Backend {
    Write-Header "Starting Backend API (.NET Core)"
    
    $backendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator"
    
    # Check if backend directory exists
    if (-not (Test-Path $backendPath)) {
        Write-Host "Backend directory not found at: $backendPath" -ForegroundColor Red
        exit 1
    }
    
    # Start the backend process
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$backendPath'; dotnet run" -WindowStyle Normal
    
    Write-Host "Backend API started. Swagger available at: https://localhost:7051/swagger" -ForegroundColor Green
}

function Start-Frontend {
    Write-Header "Starting Frontend (React)"
    
    $frontendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator.Client"
    
    # Check if frontend directory exists
    if (-not (Test-Path $frontendPath)) {
        Write-Host "Frontend directory not found at: $frontendPath" -ForegroundColor Red
        exit 1
    }
    
    # Check if node_modules exists, if not, install dependencies
    $nodeModulesPath = Join-Path $frontendPath "node_modules"
    if (-not (Test-Path $nodeModulesPath)) {
        Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$frontendPath'; npm install; npm run dev" -WindowStyle Normal
    } else {
        # Start the frontend development server
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$frontendPath'; npm run dev" -WindowStyle Normal
    }
    
    Write-Host "Frontend started. Available at: http://localhost:3000" -ForegroundColor Green
}

function Main {
    Write-Header "Bouwdepot Invoice Validator"
    
    Write-Host "Starting both backend and frontend applications..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C in the terminal windows to stop each application" -ForegroundColor Yellow
    
    # Start backend and frontend
    Start-Backend
    Start-Frontend
    
    Write-Host ""
    Write-Host "Both applications are now running." -ForegroundColor Green
    Write-Host "- Backend API: https://localhost:7051" -ForegroundColor Yellow
    Write-Host "- Frontend:    http://localhost:3000" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To stop the applications, close the opened terminal windows." -ForegroundColor Yellow
}

# Run the main function
Main
