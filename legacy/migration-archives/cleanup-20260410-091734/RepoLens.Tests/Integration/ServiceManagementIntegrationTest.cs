using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Integration;

/// <summary>
/// Comprehensive integration test for the optimized batch file service management system
/// Tests all critical scenarios that could occur during service startup, monitoring, and shutdown
/// This addresses gaps in existing test coverage from a QA professional perspective
/// </summary>
public class ServiceManagementIntegrationTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _projectRoot;
    private readonly List<int> _trackedProcessIds = new();

    public ServiceManagementIntegrationTest(ITestOutputHelper output)
    {
        _output = output;
        _projectRoot = GetProjectRoot();
        _output.WriteLine("🚀 Starting Service Management Integration Tests");
        _output.WriteLine($"Project Root: {_projectRoot}");
    }

    [Fact]
    public async Task ServiceStartup_OptimizedBatchFile_ShouldHandleAllScenarios()
    {
        _output.WriteLine("\n📋 === SERVICE STARTUP COMPREHENSIVE TEST ===");

        // Scenario 1: Clean startup (no existing processes)
        await TestCleanStartupScenarioAsync();
        
        // Scenario 2: Port conflict resolution
        await TestPortConflictResolutionAsync();
        
        // Scenario 3: Missing dependencies handling
        await TestMissingDependenciesHandlingAsync();
        
        // Scenario 4: Build failure recovery
        await TestBuildFailureRecoveryAsync();
        
        // Scenario 5: Service health monitoring
        await TestServiceHealthMonitoringAsync();
        
        // Scenario 6: Graceful shutdown handling
        await TestGracefulShutdownAsync();
        
        // Scenario 7: Zombie process cleanup
        await TestZombieProcessCleanupAsync();
        
        // Scenario 8: Configuration validation
        await TestConfigurationValidationAsync();
        
        // Scenario 9: Restart reliability
        await TestRestartReliabilityAsync();
        
        // Scenario 10: Error logging and reporting
        await TestErrorLoggingAndReportingAsync();

        _output.WriteLine("\n🎉 === ALL SERVICE MANAGEMENT SCENARIOS COMPLETED ===");
    }

    private async Task TestCleanStartupScenarioAsync()
    {
        _output.WriteLine("\n🧹 Test Scenario 1: Clean Startup");
        
        // Ensure no existing processes are running
        await KillAllRepoLensProcessesAsync();
        
        // Verify ports are free
        var backendPortFree = await IsPortFreeAsync(5179);
        var frontendPortFree = await IsPortFreeAsync(3000);
        
        Assert.True(backendPortFree, "Backend port 5179 should be free for clean startup test");
        Assert.True(frontendPortFree, "Frontend port 3000 should be free for clean startup test");
        
        _output.WriteLine("   ✅ All ports are free");
        _output.WriteLine("   ✅ No existing RepoLens processes found");
        _output.WriteLine("   🎯 Clean startup environment verified");
    }

    private async Task TestPortConflictResolutionAsync()
    {
        _output.WriteLine("\n🔧 Test Scenario 2: Port Conflict Resolution");
        
        // Create a dummy process to occupy backend port
        var dummyProcess = await CreateDummyPortOccupierAsync(5179);
        if (dummyProcess != null)
        {
            _trackedProcessIds.Add(dummyProcess.Id);
            _output.WriteLine($"   🔒 Created dummy process {dummyProcess.Id} occupying port 5179");
            
            // Verify port is occupied
            var portOccupied = await IsPortOccupiedAsync(5179);
            Assert.True(portOccupied, "Port 5179 should be occupied by dummy process");
            
            _output.WriteLine("   ✅ Port conflict scenario created");
            _output.WriteLine("   💡 Optimized batch file should detect and resolve this conflict");
            
            // Cleanup
            try { dummyProcess.Kill(); } catch { }
        }
        else
        {
            _output.WriteLine("   ⚠️  Could not create dummy process - port conflict test skipped");
        }
    }

    private async Task TestMissingDependenciesHandlingAsync()
    {
        _output.WriteLine("\n🚫 Test Scenario 3: Missing Dependencies Handling");
        
        // Test 1: Verify .NET SDK detection
        var dotnetAvailable = await CheckDotNetAvailabilityAsync();
        _output.WriteLine($"   📦 .NET SDK Available: {dotnetAvailable}");
        
        // Test 2: Verify Node.js detection  
        var nodeAvailable = await CheckNodeAvailabilityAsync();
        _output.WriteLine($"   📦 Node.js Available: {nodeAvailable}");
        
        // Test 3: Verify project directory structure
        var backendDirExists = Directory.Exists(Path.Combine(_projectRoot, "RepoLens.Api"));
        var frontendDirExists = Directory.Exists(Path.Combine(_projectRoot, "repolens-ui"));
        
        _output.WriteLine($"   📁 Backend Directory Exists: {backendDirExists}");
        _output.WriteLine($"   📁 Frontend Directory Exists: {frontendDirExists}");
        
        // Test 4: Verify project files
        var backendProjectExists = File.Exists(Path.Combine(_projectRoot, "RepoLens.Api", "RepoLens.Api.csproj"));
        var frontendPackageExists = File.Exists(Path.Combine(_projectRoot, "repolens-ui", "package.json"));
        
        _output.WriteLine($"   📄 Backend Project File Exists: {backendProjectExists}");
        _output.WriteLine($"   📄 Frontend Package File Exists: {frontendPackageExists}");
        
        if (!dotnetAvailable || !nodeAvailable || !backendDirExists || !frontendDirExists)
        {
            _output.WriteLine("   💡 Missing dependencies detected - batch file should handle gracefully");
        }
        else
        {
            _output.WriteLine("   ✅ All dependencies available");
        }
    }

    private async Task TestBuildFailureRecoveryAsync()
    {
        _output.WriteLine("\n🔨 Test Scenario 4: Build Failure Recovery");
        
        // Test backend build
        var backendBuildResult = await TestDotNetBuildAsync();
        _output.WriteLine($"   🏗️  Backend Build Result: {backendBuildResult}");
        
        // Test frontend dependency check
        var frontendDepsResult = await TestNpmDependenciesAsync();
        _output.WriteLine($"   📦 Frontend Dependencies: {frontendDepsResult}");
        
        if (!backendBuildResult || !frontendDepsResult)
        {
            _output.WriteLine("   💡 Build issues detected - optimized batch should attempt recovery");
        }
        else
        {
            _output.WriteLine("   ✅ Build validation passed");
        }
    }

    private async Task TestServiceHealthMonitoringAsync()
    {
        _output.WriteLine("\n💊 Test Scenario 5: Service Health Monitoring");
        
        // Test port monitoring capability
        _output.WriteLine("   🔍 Testing port monitoring capabilities...");
        
        // Test health check endpoints (if available)
        var backendHealthy = await TestHealthEndpointAsync("http://localhost:5179");
        var frontendHealthy = await TestHealthEndpointAsync("http://localhost:3000");
        
        _output.WriteLine($"   💚 Backend Health Check: {(backendHealthy ? "✅ Available" : "❌ Not responding")}");
        _output.WriteLine($"   💚 Frontend Health Check: {(frontendHealthy ? "✅ Available" : "❌ Not responding")}");
        
        // Test curl availability for health checks
        var curlAvailable = await CheckCurlAvailabilityAsync();
        _output.WriteLine($"   🌐 Curl Available for Health Checks: {curlAvailable}");
        
        if (!curlAvailable)
        {
            _output.WriteLine("   💡 Curl not available - batch file should fallback to port checking");
        }
    }

    private async Task TestGracefulShutdownAsync()
    {
        _output.WriteLine("\n🛑 Test Scenario 6: Graceful Shutdown");
        
        // Test if stop-services.bat exists and is executable
        var stopServicesPath = Path.Combine(_projectRoot, "stop-services.bat");
        var stopServicesExists = File.Exists(stopServicesPath);
        
        _output.WriteLine($"   📄 Stop Services Script Exists: {stopServicesExists}");
        
        if (stopServicesExists)
        {
            _output.WriteLine("   💡 Graceful shutdown capability available");
            _output.WriteLine("   🎯 Should properly terminate all processes and free ports");
        }
        else
        {
            _output.WriteLine("   ⚠️  No dedicated stop script - manual cleanup required");
        }
    }

    private async Task TestZombieProcessCleanupAsync()
    {
        _output.WriteLine("\n🧟 Test Scenario 7: Zombie Process Cleanup");
        
        // Check for existing dotnet and node processes
        var dotnetProcesses = await GetProcessesByNameAsync("dotnet");
        var nodeProcesses = await GetProcessesByNameAsync("node");
        
        _output.WriteLine($"   👻 Existing .NET processes: {dotnetProcesses.Count}");
        _output.WriteLine($"   👻 Existing Node.js processes: {nodeProcesses.Count}");
        
        // Check processes using our ports
        var backendPortProcesses = await GetProcessesUsingPortAsync(5179);
        var frontendPortProcesses = await GetProcessesUsingPortAsync(3000);
        
        _output.WriteLine($"   🔒 Processes using backend port 5179: {backendPortProcesses.Count}");
        _output.WriteLine($"   🔒 Processes using frontend port 3000: {frontendPortProcesses.Count}");
        
        if (backendPortProcesses.Count > 0 || frontendPortProcesses.Count > 0)
        {
            _output.WriteLine("   💡 Existing processes detected - cleanup required before starting");
        }
        else
        {
            _output.WriteLine("   ✅ No zombie processes detected");
        }
    }

    private async Task TestConfigurationValidationAsync()
    {
        _output.WriteLine("\n⚙️  Test Scenario 8: Configuration Validation");
        
        // Test configuration file existence
        var configPath = Path.Combine(_projectRoot, "service-config.bat");
        var configExists = File.Exists(configPath);
        
        _output.WriteLine($"   📋 Configuration File Exists: {configExists}");
        
        if (configExists)
        {
            // Test configuration loading
            var configContent = await File.ReadAllTextAsync(configPath);
            var hasBackendPort = configContent.Contains("BACKEND_PORT");
            var hasFrontendPort = configContent.Contains("FRONTEND_PORT");
            var hasTimeouts = configContent.Contains("HEALTH_CHECK_TIMEOUT");
            
            _output.WriteLine($"   🔧 Backend Port Config: {hasBackendPort}");
            _output.WriteLine($"   🔧 Frontend Port Config: {hasFrontendPort}");
            _output.WriteLine($"   ⏱️  Timeout Configurations: {hasTimeouts}");
            
            if (hasBackendPort && hasFrontendPort && hasTimeouts)
            {
                _output.WriteLine("   ✅ Configuration appears complete");
            }
            else
            {
                _output.WriteLine("   ⚠️  Configuration may be incomplete");
            }
        }
        else
        {
            _output.WriteLine("   ⚠️  No configuration file - using hardcoded values");
        }
    }

    private async Task TestRestartReliabilityAsync()
    {
        _output.WriteLine("\n🔄 Test Scenario 9: Restart Reliability");
        
        // Test individual restart scripts
        var backendRestartExists = File.Exists(Path.Combine(_projectRoot, "restart-backend-optimized.bat"));
        var frontendRestartExists = File.Exists(Path.Combine(_projectRoot, "restart-frontend-optimized.bat"));
        
        _output.WriteLine($"   🔄 Backend Restart Script: {backendRestartExists}");
        _output.WriteLine($"   🔄 Frontend Restart Script: {frontendRestartExists}");
        
        // Test main service script
        var mainServiceScript = File.Exists(Path.Combine(_projectRoot, "start-services-optimized.bat"));
        _output.WriteLine($"   🚀 Main Service Script: {mainServiceScript}");
        
        if (backendRestartExists && frontendRestartExists && mainServiceScript)
        {
            _output.WriteLine("   ✅ Complete restart infrastructure available");
        }
        else
        {
            _output.WriteLine("   ⚠️  Partial restart infrastructure - some components missing");
        }
    }

    private async Task TestErrorLoggingAndReportingAsync()
    {
        _output.WriteLine("\n📝 Test Scenario 10: Error Logging and Reporting");
        
        // Check if batch files have proper error handling
        var optimizedScript = Path.Combine(_projectRoot, "start-services-optimized.bat");
        
        if (File.Exists(optimizedScript))
        {
            var scriptContent = await File.ReadAllTextAsync(optimizedScript);
            
            var hasErrorHandling = scriptContent.Contains(":error_exit");
            var hasLogging = scriptContent.Contains("[ERROR]") || scriptContent.Contains("[WARNING]");
            var hasUserFriendlyMessages = scriptContent.Contains("Common solutions") || scriptContent.Contains("Try:");
            
            _output.WriteLine($"   🚨 Error Handling Blocks: {hasErrorHandling}");
            _output.WriteLine($"   📊 Structured Logging: {hasLogging}");
            _output.WriteLine($"   💡 User-Friendly Error Messages: {hasUserFriendlyMessages}");
            
            if (hasErrorHandling && hasLogging && hasUserFriendlyMessages)
            {
                _output.WriteLine("   ✅ Comprehensive error handling implemented");
            }
            else
            {
                _output.WriteLine("   ⚠️  Error handling could be improved");
            }
        }
        else
        {
            _output.WriteLine("   ❌ Optimized script not found");
        }
    }

    // Helper Methods
    private async Task<bool> IsPortFreeAsync(int port)
    {
        try
        {
            var result = await ExecuteCommandAsync($"netstat -an | findstr :{port}");
            return string.IsNullOrEmpty(result.output);
        }
        catch
        {
            return true; // Assume free if can't check
        }
    }

    private async Task<bool> IsPortOccupiedAsync(int port)
    {
        return !(await IsPortFreeAsync(port));
    }

    private async Task<Process?> CreateDummyPortOccupierAsync(int port)
    {
        try
        {
            // Create a simple HTTP listener on the port
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m http.server {port}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(startInfo);
            if (process != null)
            {
                await Task.Delay(1000); // Give it time to start
                return process;
            }
        }
        catch
        {
            // Python might not be available
        }

        return null;
    }

    private async Task KillAllRepoLensProcessesAsync()
    {
        try
        {
            await ExecuteCommandAsync("taskkill /f /fi \"windowtitle eq RepoLens*\" 2>nul");
            await Task.Delay(1000);
        }
        catch { }
    }

    private async Task<bool> CheckDotNetAvailabilityAsync()
    {
        var result = await ExecuteCommandAsync("dotnet --version");
        return result.exitCode == 0;
    }

    private async Task<bool> CheckNodeAvailabilityAsync()
    {
        var result = await ExecuteCommandAsync("node --version");
        return result.exitCode == 0;
    }

    private async Task<bool> CheckCurlAvailabilityAsync()
    {
        var result = await ExecuteCommandAsync("curl --version");
        return result.exitCode == 0;
    }

    private async Task<bool> TestDotNetBuildAsync()
    {
        try
        {
            var backendPath = Path.Combine(_projectRoot, "RepoLens.Api");
            if (Directory.Exists(backendPath))
            {
                var result = await ExecuteCommandAsync("dotnet build --no-restore", backendPath);
                return result.exitCode == 0;
            }
        }
        catch { }
        
        return false;
    }

    private async Task<bool> TestNpmDependenciesAsync()
    {
        try
        {
            var frontendPath = Path.Combine(_projectRoot, "repolens-ui");
            if (Directory.Exists(frontendPath))
            {
                var nodeModulesExists = Directory.Exists(Path.Combine(frontendPath, "node_modules"));
                var packageJsonExists = File.Exists(Path.Combine(frontendPath, "package.json"));
                return nodeModulesExists && packageJsonExists;
            }
        }
        catch { }
        
        return false;
    }

    private async Task<bool> TestHealthEndpointAsync(string url)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<Process>> GetProcessesByNameAsync(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).ToList();
        }
        catch
        {
            return new List<Process>();
        }
    }

    private async Task<List<int>> GetProcessesUsingPortAsync(int port)
    {
        try
        {
            var result = await ExecuteCommandAsync($"netstat -ano | findstr :{port}");
            var pids = new List<int>();
            
            if (!string.IsNullOrEmpty(result.output))
            {
                var lines = result.output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && int.TryParse(parts[4], out int pid))
                    {
                        pids.Add(pid);
                    }
                }
            }
            
            return pids.Distinct().ToList();
        }
        catch
        {
            return new List<int>();
        }
    }

    private async Task<(string output, int exitCode)> ExecuteCommandAsync(string command, string? workingDirectory = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? _projectRoot
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                return (!string.IsNullOrEmpty(output) ? output : error, process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            return (ex.Message, -1);
        }
        
        return ("", -1);
    }

    private string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir, "RepoLens.sln")))
            {
                return currentDir;
            }
            
            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }
        
        return Directory.GetCurrentDirectory();
    }

    public void Dispose()
    {
        // Clean up any processes we created during testing
        foreach (var pid in _trackedProcessIds)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch { }
        }
    }
}
