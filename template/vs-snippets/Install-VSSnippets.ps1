# NetCorePal Template - Visual Studio Code Snippets Installer
# Auto install Visual Studio code snippets

param(
    [string]$VisualStudioVersion = "2022",
    [switch]$ShowPathOnly
)

$ErrorActionPreference = "Stop"

Write-Host "NetCorePal Template - Visual Studio Code Snippets Installer" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Get current script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SnippetFile = Join-Path $ScriptDir "NetCorePalTemplates.snippet"

# Check if snippet file exists
if (-not (Test-Path $SnippetFile)) {
    Write-Error "Snippet file not found: $SnippetFile"
    exit 1
}

# Build Visual Studio snippets directory path
$VSSnippetsPath = "$env:USERPROFILE\Documents\Visual Studio $VisualStudioVersion\Code Snippets\Visual C#\My Code Snippets"

Write-Host "Target directory: $VSSnippetsPath" -ForegroundColor Yellow

# If only showing path, don't execute installation
if ($ShowPathOnly) {
    Write-Host ""
    Write-Host "Manual installation steps:" -ForegroundColor Cyan
    Write-Host "1. Ensure target directory exists: $VSSnippetsPath" -ForegroundColor White
    Write-Host "2. Copy file: $SnippetFile" -ForegroundColor White
    Write-Host "3. To target directory: $VSSnippetsPath" -ForegroundColor White
    Write-Host "4. Restart Visual Studio" -ForegroundColor White
    Write-Host ""
    Write-Host "Or use Tools > Code Snippets Manager > Import in Visual Studio" -ForegroundColor Yellow
    return
}

# Create directory if it doesn't exist
if (-not (Test-Path $VSSnippetsPath)) {
    Write-Host "Creating snippets directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $VSSnippetsPath -Force | Out-Null
}

# Copy snippet file
$DestinationFile = Join-Path $VSSnippetsPath "NetCorePalTemplates.snippet"

try {
    Copy-Item -Path $SnippetFile -Destination $DestinationFile -Force
    Write-Host "Code snippets installed successfully!" -ForegroundColor Green
    Write-Host "  Source file: $SnippetFile" -ForegroundColor Gray
    Write-Host "  Target file: $DestinationFile" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "Available snippet shortcuts:" -ForegroundColor Cyan
    Write-Host "  postproc    - PostProcessor class" -ForegroundColor White
    Write-Host "  tstclass    - Test class" -ForegroundColor White
    Write-Host "  ncpcmd      - NetCorePal command" -ForegroundColor White
    Write-Host "  ncpcmdres   - Command response" -ForegroundColor White
    Write-Host "  evnt        - Domain event" -ForegroundColor White
    Write-Host "  ncprepo     - Repository interface" -ForegroundColor White
    Write-Host "  epp         - FastEndpoint" -ForegroundColor White
    
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Cyan
    Write-Host "1. Open C# file in Visual Studio" -ForegroundColor White
    Write-Host "2. Type shortcut (like 'postproc')" -ForegroundColor White
    Write-Host "3. Press Tab key twice" -ForegroundColor White
    Write-Host "4. Fill parameters and press Tab to switch to next parameter" -ForegroundColor White
    
    Write-Host ""
    Write-Host "Note: If Visual Studio is running, restart it to load new snippets." -ForegroundColor Yellow
}
catch {
    Write-Error "Installation failed: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "Installation completed!" -ForegroundColor Green
