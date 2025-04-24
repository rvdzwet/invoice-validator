# Publish-App.ps1
# Script to build the frontend and publish the backend application for deployment

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$publishDir = Join-Path $projectRoot "publish"

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

# --- Build Frontend ---
Write-Header "Building Frontend (React)"

$frontendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator.Client"
$backendPath = Join-Path $projectRoot "BouwdepotInvoiceValidator"

# Check if frontend directory exists
if (-not (Test-Path $frontendPath)) {
    Write-Host "Frontend directory not found at: $frontendPath" -ForegroundColor Red
    exit 1
}

Set-Location $frontendPath

# Install dependencies if needed
$nodeModulesPath = Join-Path $frontendPath "node_modules"
if (-not (Test-Path $nodeModulesPath)) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Frontend dependency installation failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}

# Run production build (outputs to ../BouwdepotInvoiceValidator/wwwroot)
Write-Host "Running frontend production build..." -ForegroundColor Yellow
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

# --- Publish Backend ---
Write-Header "Publishing Backend (.NET Core)"

# Check if backend directory exists
if (-not (Test-Path $backendPath)) {
    Write-Host "Backend directory not found at: $backendPath" -ForegroundColor Red
    exit 1
}

Set-Location $backendPath

# Publish the application
Write-Host "Running dotnet publish..." -ForegroundColor Yellow
dotnet publish -c Release -o $publishDir --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend publish failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Write-Host "Backend published successfully to: $publishDir" -ForegroundColor Green

# Return to project root
Set-Location $projectRoot

Write-Host ""
Write-Host "Application successfully published!" -ForegroundColor Green
Write-Host "Publish location: $publishDir" -ForegroundColor Yellow
