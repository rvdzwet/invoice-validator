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

function Build-ProductionRelease {
    Write-Header "Building Production Release"
    
    $frontendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator.Client"
    $backendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator"
    
    # Check if directories exist
    if (-not (Test-Path $frontendPath)) {
        Write-Host "Frontend directory not found at: $frontendPath" -ForegroundColor Red
        exit 1
    }
    
    if (-not (Test-Path $backendPath)) {
        Write-Host "Backend directory not found at: $backendPath" -ForegroundColor Red
        exit 1
    }
    
    # Build frontend production bundle
    Write-Host "Building frontend production bundle..." -ForegroundColor Yellow
    Set-Location $frontendPath
    
    # Install dependencies if needed
    $nodeModulesPath = Join-Path $frontendPath "node_modules"
    if (-not (Test-Path $nodeModulesPath)) {
        Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
        npm install
    }
    
    # Run production build
    npm run build:prod
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Frontend build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    
    # Check if wwwroot directory was created in the backend project
    $wwwrootPath = Join-Path $backendPath "wwwroot"
    if (-not (Test-Path $wwwrootPath)) {
        Write-Host "Frontend build failed - wwwroot directory not created in $backendPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Frontend production build completed successfully into $wwwrootPath" -ForegroundColor Green
    
    # Build backend
    Write-Host "Building backend for production..." -ForegroundColor Yellow
    Set-Location $backendPath
    
    # Build in Release configuration
    dotnet build -c Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Backend build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Backend built successfully in Release configuration" -ForegroundColor Green
    
    # Return to project root
    Set-Location $projectRoot
    
    Write-Host ""
    Write-Host "Production release build completed!" -ForegroundColor Green
    Write-Host "Frontend bundle location: $wwwrootPath" -ForegroundColor Yellow
    Write-Host "Backend release build complete" -ForegroundColor Yellow
}

function Main {
    param (
        [Parameter()]
        [switch]$BuildProduction
    )
    
    Write-Header "Bouwdepot Invoice Validator"
    
    if ($BuildProduction) {
        # Build production release
        Build-ProductionRelease
    } else {
        # Run development servers
        Write-Host "Starting both backend and frontend applications..." -ForegroundColor Yellow
        Write-Host "Starting the backend application which now serves the frontend..." -ForegroundColor Yellow
        Write-Host "Press Ctrl+C in the terminal window to stop the application" -ForegroundColor Yellow
        
        # Start backend (which now serves frontend)
        Start-Backend
        # Start-Frontend # No longer needed as backend serves the frontend
        
        Write-Host ""
        Write-Host "Application is now running." -ForegroundColor Green
        Write-Host "- Access at: https://localhost:7051" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To stop the application, close the opened terminal window." -ForegroundColor Yellow
    }
}

# Main execution logic
# Check if -BuildProduction parameter was passed
if ($args -contains "-BuildProduction") {
    # Run in production build mode
    Main -BuildProduction
} else {
    # Run in development mode
    Main
}
