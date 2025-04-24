# Script to build the client, copy assets, and publish the backend .NET application

# --- Build Client ---
Write-Host "Building frontend client..."
Push-Location .\BouwdepotInvoiceValidator.Client
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to navigate to client directory."
    exit 1
}

Write-Host "Installing client dependencies..."
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Error "npm install failed!"
    Pop-Location
    exit 1
}

Write-Host "Building client application for production..."
npm run build:prod
if ($LASTEXITCODE -ne 0) {
    Write-Error "Client build failed!"
    Pop-Location
    exit 1
}

Write-Host "Returning to root directory..."
Pop-Location

# --- Copy Client Assets ---
$sourceDir = ".\BouwdepotInvoiceValidator.Client\dist"
$destDir = ".\BouwdepotInvoiceValidator\wwwroot"
$publishDir = ".\publish-output"

Write-Host "Checking client build output directory: $sourceDir"
if (-not (Test-Path $sourceDir)) {
    Write-Error "Source directory '$sourceDir' not found. Client build might have failed or output to a different location."
    exit 1
}

Write-Host "Preparing backend wwwroot directory: $destDir"
# Ensure the destination directory exists
if (-not (Test-Path $destDir)) {
    Write-Host "Destination directory not found. Creating..."
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
} else {
    Write-Host "Clearing destination directory..."
    Remove-Item -Path "$destDir\*" -Recurse -Force
}

Write-Host "Copying built client assets from '$sourceDir' to '$destDir'..."
Copy-Item -Path "$sourceDir\*" -Destination $destDir -Recurse -Force
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy client assets."
    exit 1
}
Write-Host "Client assets copied successfully."

# --- Publish Backend ---
Write-Host "Publishing .NET application..."
# Clean publish directory first
if (Test-Path $publishDir) {
    Write-Host "Removing existing publish directory: $publishDir"
    Remove-Item -Path $publishDir -Recurse -Force
}

dotnet publish .\BouwdepotInvoiceValidator\BouwdepotInvoiceValidator.csproj -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Error ".NET publish failed!"
    exit 1
}

Write-Host "Application published successfully to '$publishDir'."
exit 0
