# RepoLens Service Monitor PowerShell Script
param(
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Continue"
$OriginalLocation = Get-Location

# Configuration
$Config = @{
    Backend = @{
        Name = "RepoLens Backend API"
        Path = "RepoLens.Api"
        Command = "dotnet"
        Arguments = "run"
        Port = 5179
        ProcessName = "dotnet"
    }
    Frontend = @{
        Name = "RepoLens Frontend"
        Path = "repolens-ui" 
        Command = "npm"
        Arguments = "start"
        Port = 3000
        ProcessName = "node"
    }
    CheckInterval = 10  # seconds
    RestartDelay = 5    # seconds
}

# Service tracking
$Services = @{
    Backend = $null
    Frontend = $null
}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $colorMap = @{
        "INFO" = "White"
        "SUCCESS" = "Green"
        "WARNING" = "Yellow"
        "ERROR" = "Red"
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $colorMap[$Level]
}

function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Start-Service {
    param([string]$ServiceType)
    
    $serviceConfig = $Config[$ServiceType]
    Write-Log "Starting $($serviceConfig.Name)..." "INFO"
    
    try {
        Set-Location $serviceConfig.Path
        
        # Start the process
        $process = Start-Process -FilePath $serviceConfig.Command -ArgumentList $serviceConfig.Arguments -PassThru -NoNewWindow
        $Services[$ServiceType] = $process
        
        Write-Log "$($serviceConfig.Name) started with PID: $($process.Id)" "SUCCESS"
        
        # Wait for the service to be ready
        $maxWaitTime = 60  # seconds
        $waitTime = 0
        
        while ($waitTime -lt $maxWaitTime) {
            if (Test-Port $serviceConfig.Port) {
                Write-Log "$($serviceConfig.Name) is ready on port $($serviceConfig.Port)" "SUCCESS"
                break
            }
            Start-Sleep -Seconds 2
            $waitTime += 2
            Write-Log "Waiting for $($serviceConfig.Name) to start... ($waitTime/$maxWaitTime seconds)" "INFO"
        }
        
        if ($waitTime -ge $maxWaitTime) {
            Write-Log "$($serviceConfig.Name) took too long to start" "WARNING"
        }
        
        Set-Location $OriginalLocation
        return $true
    }
    catch {
        Write-Log "Failed to start $($serviceConfig.Name): $($_.Exception.Message)" "ERROR"
        Set-Location $OriginalLocation
        return $false
    }
}

function Stop-Services {
    Write-Log "Stopping all services..." "INFO"
    
    foreach ($serviceType in $Services.Keys) {
        $process = $Services[$serviceType]
        if ($process -and !$process.HasExited) {
            try {
                $process.Kill()
                $process.WaitForExit(5000)
                Write-Log "$($Config[$serviceType].Name) stopped" "SUCCESS"
            }
            catch {
                Write-Log "Failed to stop $($Config[$serviceType].Name): $($_.Exception.Message)" "ERROR"
            }
        }
    }
}

function Test-ServiceHealth {
    param([string]$ServiceType)
    
    $serviceConfig = $Config[$ServiceType]
    $process = $Services[$ServiceType]
    
    # Check if process is still running
    if ($process -and !$process.HasExited) {
        # Check if port is still responsive
        if (Test-Port $serviceConfig.Port) {
            return $true
        }
        else {
            Write-Log "$($serviceConfig.Name) process running but port $($serviceConfig.Port) not responsive" "WARNING"
            return $false
        }
    }
    else {
        Write-Log "$($serviceConfig.Name) process has stopped" "WARNING"
        return $false
    }
}

# Main execution
try {
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "    RepoLens Service Monitor (PowerShell)" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Backend: .NET API (RepoLens.Api)" -ForegroundColor Gray
    Write-Host "Frontend: React App (repolens-ui)" -ForegroundColor Gray
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""

    # Register cleanup handler
    Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Stop-Services }
    
    # Handle Ctrl+C gracefully
    [Console]::TreatControlCAsInput = $false
    [Console]::CancelKeyPress += {
        param($sender, $e)
        $e.Cancel = $true
        Write-Log "Shutdown signal received. Stopping services..." "WARNING"
        Stop-Services
        exit
    }

    # Initial service startup
    Write-Log "Starting initial services..." "INFO"
    
    if (!(Start-Service "Backend")) {
        Write-Log "Failed to start backend service. Exiting." "ERROR"
        exit 1
    }
    
    Start-Sleep -Seconds $Config.RestartDelay
    
    if (!(Start-Service "Frontend")) {
        Write-Log "Failed to start frontend service. Exiting." "ERROR"
        exit 1
    }

    Write-Log "Both services started successfully!" "SUCCESS"
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "Services are now running:" -ForegroundColor Green
    Write-Host "- Backend API: http://localhost:$($Config.Backend.Port)" -ForegroundColor Green
    Write-Host "- Frontend: http://localhost:$($Config.Frontend.Port)" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host ""
    Write-Log "Monitoring started. Press Ctrl+C to stop." "INFO"

    # Main monitoring loop
    while ($true) {
        Start-Sleep -Seconds $Config.CheckInterval
        
        foreach ($serviceType in @("Backend", "Frontend")) {
            if (!(Test-ServiceHealth $serviceType)) {
                Write-Log "$($Config[$serviceType].Name) health check failed. Restarting..." "WARNING"
                
                # Kill the old process if it exists
                $oldProcess = $Services[$serviceType]
                if ($oldProcess -and !$oldProcess.HasExited) {
                    try { $oldProcess.Kill() } catch { }
                }
                
                # Wait before restart
                Start-Sleep -Seconds $Config.RestartDelay
                
                # Restart the service
                if (Start-Service $serviceType) {
                    Write-Log "$($Config[$serviceType].Name) restarted successfully" "SUCCESS"
                }
                else {
                    Write-Log "Failed to restart $($Config[$serviceType].Name)" "ERROR"
                }
            }
            elseif ($Verbose) {
                Write-Log "$($Config[$serviceType].Name) is healthy" "INFO"
            }
        }
    }
}
catch {
    Write-Log "Critical error in service monitor: $($_.Exception.Message)" "ERROR"
    Stop-Services
    exit 1
}
finally {
    Set-Location $OriginalLocation
}
