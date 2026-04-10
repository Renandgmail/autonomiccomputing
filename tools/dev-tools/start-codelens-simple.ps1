# CodeLens Platform - Simple Deployment Script
# Works without Docker - just .NET, Node.js, and PostgreSQL required

param(
    [switch]$OnlyApi = $false,
    [switch]$SkipFrontend = $false,
    [switch]$ShowStatus = $false,
    [switch]$StopServices = $false,
    [switch]$DryRun = $false
)

Write-Host "🚀 CodeLens Platform - Simple Deployment" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date)" -ForegroundColor Gray

# Check if required tools are available
function Test-Prerequisites {
    Write-Host "`n📋 Checking Prerequisites..." -ForegroundColor Yellow
    
    $tools = @("dotnet", "node", "npm")
    $missing = @()
    
    foreach ($tool in $tools) {
        try {
            $null = & $tool --version 2>$null
            Write-Host "✅ $tool is available" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ $tool is missing" -ForegroundColor Red
            $missing += $tool
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "❌ Missing: $($missing -join ', ')" -ForegroundColor Red
        Write-Host "Please install missing tools and try again." -ForegroundColor Red
        return $false
    }
    
    return $true
}

# Show service status
function Show-Status {
    Write-Host "`n📊 Service Status Check:" -ForegroundColor Cyan
    
    $ports = @(
        @{Name="Database"; Port=5432},
        @{Name="API"; Port=5000},
        @{Name="Frontend"; Port=3000}
    )
    
    foreach ($service in $ports) {
        try {
            $tcpClient = New-Object System.Net.Sockets.TcpClient
            $tcpClient.ConnectAsync("localhost", $service.Port).Wait(1000)
            if ($tcpClient.Connected) {
                Write-Host "✅ $($service.Name) (Port $($service.Port)) is running" -ForegroundColor Green
                $tcpClient.Close()
            } else {
                Write-Host "❌ $($service.Name) (Port $($service.Port)) is not running" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "❌ $($service.Name) (Port $($service.Port)) is not running" -ForegroundColor Red
        }
    }
    
    Write-Host "`n🔗 Expected URLs:" -ForegroundColor Cyan
    Write-Host "  • Frontend: http://localhost:3000" -ForegroundColor White
    Write-Host "  • API: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  • Health: http://localhost:5000/health" -ForegroundColor Gray
}

# Main execution
if ($ShowStatus) {
    Show-Status
    exit
}

if ($StopServices) {
    Write-Host "🛑 To stop services, close the PowerShell windows or press Ctrl+C in each" -ForegroundColor Yellow
    exit
}

# Check prerequisites
if (-not (Test-Prerequisites)) {
    exit 1
}

# Database reminder
Write-Host "`n🗄️ Database Check:" -ForegroundColor Blue
Write-Host "Make sure PostgreSQL is running on localhost:5432" -ForegroundColor Gray
Write-Host "Default database: codelens_platform" -ForegroundColor Gray

if ($DryRun) {
    Write-Host "`n🔍 DRY RUN - Would start these services:" -ForegroundColor Magenta
    Write-Host "  • API: dotnet run --project CodeLens.Api" -ForegroundColor Gray
    if (-not $OnlyApi -and -not $SkipFrontend) {
        Write-Host "  • Frontend: npm start (in repolens-ui folder)" -ForegroundColor Gray
    }
    Write-Host "`nDry run completed successfully!" -ForegroundColor Green
    exit
}

# Start services
Write-Host "`n🚀 Starting CodeLens Services..." -ForegroundColor Cyan

# Start API
Write-Host "Starting API..." -ForegroundColor Blue
Start-Process powershell -ArgumentList "-Command", "cd '$PWD'; dotnet run --project CodeLens.Api --urls=http://localhost:5000"

Start-Sleep -Seconds 3

# Start Frontend (if not skipped)
if (-not $OnlyApi -and -not $SkipFrontend) {
    Write-Host "Starting Frontend..." -ForegroundColor Blue
    Start-Process powershell -ArgumentList "-Command", "cd '$PWD\repolens-ui'; npm start"
}

Write-Host "`n🎉 CodeLens Platform Started!" -ForegroundColor Green
Write-Host "`n🔗 Access URLs:" -ForegroundColor Cyan
Write-Host "  • API: http://localhost:5000/swagger" -ForegroundColor White
if (-not $OnlyApi -and -not $SkipFrontend) {
    Write-Host "  • Frontend: http://localhost:3000" -ForegroundColor White
}
Write-Host "  • Health: http://localhost:5000/health" -ForegroundColor Gray

Write-Host "`n📚 Management Commands:" -ForegroundColor Yellow
Write-Host "  • Check status: .\start-codelens-simple.ps1 -ShowStatus" -ForegroundColor White
Write-Host "  • API only: .\start-codelens-simple.ps1 -OnlyApi" -ForegroundColor White
Write-Host "  • Skip frontend: .\start-codelens-simple.ps1 -SkipFrontend" -ForegroundColor White

Write-Host "`n💡 Services are running in separate windows" -ForegroundColor Gray
Write-Host "💡 Close the windows or press Ctrl+C to stop services" -ForegroundColor Gray
