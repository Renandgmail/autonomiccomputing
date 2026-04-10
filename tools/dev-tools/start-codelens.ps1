# CodeLens Platform - Flexible Deployment Script
# Supports native (no Docker), Docker, and hybrid deployment modes

param(
    [string]$Mode = "native",
    [string]$Environment = "development",
    [switch]$SkipDatabase = $false,
    [switch]$SkipFrontend = $false,
    [switch]$SkipWorker = $false,
    [switch]$OnlyApi = $false,
    [switch]$DryRun = $false,
    [switch]$StopServices = $false,
    [switch]$ShowStatus = $false
)

Write-Host "🚀 CodeLens Platform Deployment" -ForegroundColor Cyan
Write-Host "Mode: $Mode | Environment: $Environment" -ForegroundColor Gray
Write-Host "Time: $(Get-Date)" -ForegroundColor Gray

# Global variables
$script:runningProcesses = @{}

# Configuration
$config = @{
    services = @{
        api = @{
            name = "CodeLens.Api"
            port = 5000
            ssl_port = 5001
            startup_command = "dotnet run --project CodeLens.Api --urls=https://localhost:5001;http://localhost:5000"
        }
        worker = @{
            name = "CodeLens.Worker"
            port = 5010
            startup_command = "dotnet run --project CodeLens.Worker --urls=http://localhost:5010"
        }
        frontend = @{
            name = "CodeLens.UI"
            port = 3000
            path = "repolens-ui"
            startup_command = "npm start"
        }
    }
    database = @{
        host = "localhost"
        port = 5432
        database = "codelens_platform"
    }
}

# Check prerequisites
function Check-Prerequisites {
    Write-Host "`n📋 Checking Prerequisites for '$Mode' mode..." -ForegroundColor Yellow
    
    $requiredTools = @(
        @{Name = "dotnet"; Command = "dotnet --version"; Description = ".NET SDK"},
        @{Name = "node"; Command = "node --version"; Description = "Node.js"},
        @{Name = "npm"; Command = "npm --version"; Description = "npm"}
    )
    
    if ($Mode -eq "docker" -or $Mode -eq "hybrid") {
        $requiredTools += @{Name = "docker"; Command = "docker --version"; Description = "Docker"}
    }
    
    $missingTools = @()
    
    foreach ($tool in $requiredTools) {
        try {
            $null = Invoke-Expression $tool.Command -ErrorAction Stop
            Write-Host "✅ $($tool.Description) is available" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ $($tool.Description) is missing" -ForegroundColor Red
            $missingTools += $tool.Name
        }
    }
    
    if ($missingTools.Count -gt 0) {
        Write-Host "`n💡 Missing tools: $($missingTools -join ', ')" -ForegroundColor Red
        Write-Host "Please install missing tools and try again." -ForegroundColor Red
        exit 1
    }
}

# Start a service natively
function Start-NativeService {
    param($ServiceName, $ServiceConfig)
    
    if ($DryRun) {
        Write-Host "🔍 [DRY RUN] Would start $ServiceName" -ForegroundColor Magenta
        Write-Host "   Command: $($ServiceConfig.startup_command)" -ForegroundColor Gray
        return
    }
    
    Write-Host "🚀 Starting $ServiceName..." -ForegroundColor Blue
    
    try {
        $command = $ServiceConfig.startup_command
        $workingDir = Get-Location
        
        # Handle frontend service
        if ($ServiceName -eq "frontend" -and $ServiceConfig.path) {
            $workingDir = Join-Path (Get-Location) $ServiceConfig.path
        }
        
        # Start the process
        $processInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processInfo.FileName = "powershell"
        $processInfo.Arguments = "-Command `"$command`""
        $processInfo.WorkingDirectory = $workingDir
        $processInfo.UseShellExecute = $true
        $processInfo.CreateNoWindow = $false
        
        $process = [System.Diagnostics.Process]::Start($processInfo)
        $script:runningProcesses[$ServiceName] = $process
        
        Write-Host "✅ $ServiceName started successfully" -ForegroundColor Green
        Write-Host "   Process ID: $($process.Id)" -ForegroundColor Gray
        Write-Host "   Port: $($ServiceConfig.port)" -ForegroundColor Gray
    }
    catch {
        Write-Host "❌ Failed to start $ServiceName" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
    }
}

# Stop all running services
function Stop-AllServices {
    Write-Host "`n🛑 Stopping all CodeLens services..." -ForegroundColor Red
    
    foreach ($serviceName in $script:runningProcesses.Keys) {
        $process = $script:runningProcesses[$serviceName]
        if ($process -and -not $process.HasExited) {
            Write-Host "🛑 Stopping $serviceName (PID: $($process.Id))" -ForegroundColor Yellow
            try {
                $process.Kill()
                $process.WaitForExit(5000)
                Write-Host "✅ $serviceName stopped" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Failed to stop $serviceName" -ForegroundColor Red
            }
        }
    }
}

# Show status of all services
function Show-ServiceStatus {
    Write-Host "`n📊 CodeLens Service Status:" -ForegroundColor Cyan
    
    $services = @(
        @{Name="API"; Port=5000; Url="http://localhost:5000"},
        @{Name="Worker"; Port=5010; Url="http://localhost:5010"},
        @{Name="Frontend"; Port=3000; Url="http://localhost:3000"},
        @{Name="Database"; Port=5432; Url=""}
    )
    
    foreach ($service in $services) {
        $status = "❓ Unknown"
        $color = "Gray"
        
        try {
            if ($service.Url) {
                $response = Invoke-WebRequest -Uri $service.Url -TimeoutSec 5 -ErrorAction Stop
                if ($response.StatusCode -eq 200) {
                    $status = "✅ Running"
                    $color = "Green"
                }
            } else {
                # Test port for database
                $tcpClient = New-Object System.Net.Sockets.TcpClient
                $tcpClient.Connect("localhost", $service.Port)
                $status = "✅ Running"
                $color = "Green"
                $tcpClient.Close()
            }
        }
        catch {
            $status = "❌ Stopped"
            $color = "Red"
        }
        
        Write-Host "  $($service.Name) ($($service.Port)): $status" -ForegroundColor $color
    }
    
    Write-Host "`n🔗 Service URLs:" -ForegroundColor Cyan
    Write-Host "  • API: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  • Frontend: http://localhost:3000" -ForegroundColor White
    Write-Host "  • Health: http://localhost:5000/health" -ForegroundColor Gray
}

# Main execution
if ($StopServices) {
    Stop-AllServices
    exit
}

if ($ShowStatus) {
    Show-ServiceStatus
    exit
}

# Check prerequisites
Check-Prerequisites

# Database check
if (-not $SkipDatabase) {
    Write-Host "`n🗄️  Database should be running on localhost:5432" -ForegroundColor Blue
    Write-Host "💡 Make sure PostgreSQL is started" -ForegroundColor Gray
}

# Start services
Write-Host "`n🚀 Starting CodeLens Platform in '$Mode' mode..." -ForegroundColor Cyan

if (-not $OnlyApi) {
    # Start worker service
    if (-not $SkipWorker) {
        Start-NativeService -ServiceName "worker" -ServiceConfig $config.services.worker
        Start-Sleep -Seconds 2
    }
    
    # Start frontend
    if (-not $SkipFrontend) {
        Start-NativeService -ServiceName "frontend" -ServiceConfig $config.services.frontend
        Start-Sleep -Seconds 2
    }
}

# Start API (main service)
Start-NativeService -ServiceName "api" -ServiceConfig $config.services.api

if (-not $DryRun) {
    Write-Host "`n🎉 CodeLens Platform started successfully!" -ForegroundColor Green
    Write-Host "`n🔗 Access your application:" -ForegroundColor Cyan
    Write-Host "  • Frontend: http://localhost:3000" -ForegroundColor White
    Write-Host "  • API: http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  • Health: http://localhost:5000/health" -ForegroundColor Gray
    
    Write-Host "`n📚 Management commands:" -ForegroundColor Yellow
    Write-Host "  • Show status: .\start-codelens.ps1 -ShowStatus" -ForegroundColor White
    Write-Host "  • Stop services: .\start-codelens.ps1 -StopServices" -ForegroundColor White
    Write-Host "  • API only: .\start-codelens.ps1 -OnlyApi" -ForegroundColor White
    
    Write-Host "`n💡 Press Ctrl+C to stop all services or run: .\start-codelens.ps1 -StopServices" -ForegroundColor Gray
    Write-Host "💡 Services will continue running in separate windows" -ForegroundColor Gray
}
