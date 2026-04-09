@echo off
setlocal enabledelayedexpansion

title RepoLens Service Startup

echo ================================================
echo              RepoLens Service Startup
echo ================================================
echo Time: %date% %time%
echo.

:: Configuration
set "API_PORT=5000"
set "FRONTEND_PORT=3000"
set "STARTUP_TIMEOUT=45"

echo [INFO] Starting RepoLens Platform Services
echo [INFO] API Port: %API_PORT%
echo [INFO] Frontend Port: %FRONTEND_PORT%
echo.

:: Step 1: Check Prerequisites
echo [STEP 1/5] Checking Prerequisites
echo [INFO] Verifying .NET SDK availability
dotnet --version
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK not found. Please install .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo [INFO] .NET SDK found

echo [INFO] Verifying Node.js availability
node --version
if %errorlevel% neq 0 (
    echo [ERROR] Node.js not found. Please install Node.js from https://nodejs.org
    pause
    exit /b 1
)
echo [INFO] Node.js found

echo [INFO] Verifying npm availability
npm --version
if %errorlevel% neq 0 (
    echo [ERROR] npm not found. Install Node.js from https://nodejs.org
    pause
    exit /b 1
)
echo [INFO] npm found
echo [INFO] Prerequisites check completed successfully
echo.

:: Step 2: Check Project Structure
echo [STEP 2/5] Validating Project Structure
if not exist "RepoLens.Api" (
    echo [ERROR] RepoLens.Api directory not found
    pause
    exit /b 1
)
echo [INFO] RepoLens.Api directory found

if not exist "repolens-ui" (
    echo [ERROR] repolens-ui directory not found
    pause
    exit /b 1
)
echo [INFO] repolens-ui directory found

if not exist "RepoLens.sln" (
    echo [ERROR] RepoLens.sln solution file not found
    pause
    exit /b 1
)
echo [INFO] RepoLens.sln solution file found
echo [INFO] Project structure validation completed
echo.

:: Step 3: Clean Previous Processes
echo [STEP 3/5] Cleaning Previous Processes
echo [LOG] Killing any existing dotnet and node processes
taskkill /f /im dotnet.exe 2>nul
taskkill /f /im node.exe 2>nul
echo [LOG] Process cleanup completed
echo.

:: Step 4: Build and Start Backend
echo [STEP 4/5] Building and Starting Backend API
echo [LOG] Current directory: %CD%
echo [LOG] Building RepoLens solution with Release configuration
echo [LOG] Build command: dotnet build RepoLens.sln --configuration Release
dotnet build RepoLens.sln --configuration Release
set "build_result=%errorlevel%"
echo [LOG] Build exit code: %build_result%
if %build_result% neq 0 (
    echo [ERROR] Backend build failed with exit code %build_result%
    echo [ERROR] Check the build output above for details
    pause
    exit /b 1
)
echo [INFO] Backend build completed successfully

echo [LOG] Changing to RepoLens.Api directory
cd /d "%~dp0RepoLens.Api"
echo [LOG] Current directory after cd: %CD%
echo [LOG] Starting RepoLens API on port %API_PORT%
echo [LOG] API startup command: dotnet run --configuration Release --urls=http://localhost:%API_PORT%
start "RepoLens-API" cmd /k "dotnet run --configuration Release --urls=http://localhost:%API_PORT%"
echo [LOG] API process started in new window
cd /d "%~dp0"
echo [LOG] Returned to root directory: %CD%

echo [LOG] Starting API health check loop with %STARTUP_TIMEOUT% second timeout
set "wait_time=0"
:wait_api
echo [LOG] API health check attempt at %wait_time% seconds
if %wait_time% geq %STARTUP_TIMEOUT% (
    echo [WARNING] API startup took longer than expected (%wait_time% seconds)
    echo [LOG] Final port check before continuing...
    netstat -an | findstr ":%API_PORT%" 
    echo [INFO] Check the RepoLens-API window for detailed status
    echo [INFO] Continuing with frontend startup...
    goto start_frontend
)

echo [LOG] Checking if port %API_PORT% is listening...
netstat -an | findstr ":%API_PORT% " | findstr "LISTENING" >nul
set "port_check_result=%errorlevel%"
echo [LOG] Port check result: %port_check_result% (0=listening, 1=not listening)
if %port_check_result% equ 0 (
    echo [SUCCESS] API is running on http://localhost:%API_PORT%
    goto start_frontend
)

echo [LOG] API not ready yet, waiting 3 seconds... (%wait_time%/%STARTUP_TIMEOUT%s)
timeout /t 3 /nobreak >nul
set /a wait_time+=3
goto wait_api

:start_frontend
echo.
:: Step 5: Start Frontend
echo [STEP 5/5] Starting Frontend Application
echo [LOG] Changing to repolens-ui directory
cd /d "%~dp0repolens-ui"
echo [LOG] Current directory for frontend: %CD%

echo [LOG] Installing frontend dependencies with npm install
npm install
set "npm_result=%errorlevel%"
echo [LOG] npm install exit code: %npm_result%
if %npm_result% neq 0 (
    echo [WARNING] npm install encountered issues (exit code %npm_result%), continuing...
) else (
    echo [LOG] npm install completed successfully
)

echo [LOG] Starting React development server on port %FRONTEND_PORT%
echo [LOG] Frontend startup command: npm start
start "RepoLens-Frontend" cmd /k "npm start"
echo [LOG] Frontend process started in new window
cd /d "%~dp0"
echo [LOG] Returned to root directory: %CD%

echo [LOG] Starting frontend health check loop with %STARTUP_TIMEOUT% second timeout
set "wait_time=0"
:wait_frontend
echo [LOG] Frontend health check attempt at %wait_time% seconds
if %wait_time% geq %STARTUP_TIMEOUT% (
    echo [WARNING] Frontend startup took longer than expected (%wait_time% seconds)
    echo [LOG] Final port check before continuing...
    netstat -an | findstr ":%FRONTEND_PORT%"
    echo [INFO] Check the RepoLens-Frontend window for detailed status
    echo [INFO] You can manually verify by opening http://localhost:%FRONTEND_PORT%
    goto services_summary
)

echo [LOG] Checking if port %FRONTEND_PORT% is listening...
netstat -an | findstr ":%FRONTEND_PORT% " | findstr "LISTENING" >nul
set "frontend_port_check=%errorlevel%"
echo [LOG] Frontend port check result: %frontend_port_check% (0=listening, 1=not listening)
if %frontend_port_check% equ 0 (
    echo [SUCCESS] Frontend is running on http://localhost:%FRONTEND_PORT%
    goto services_summary
)

echo [LOG] Frontend not ready yet, waiting 3 seconds... (%wait_time%/%STARTUP_TIMEOUT%s)
timeout /t 3 /nobreak >nul
set /a wait_time+=3
goto wait_frontend

:services_summary
echo.
echo ================================================
echo            Services Started Successfully
echo ================================================
echo.
echo Access Points:
echo   API: http://localhost:%API_PORT%
echo   Frontend: http://localhost:%FRONTEND_PORT%
echo   Health Check: http://localhost:%API_PORT%/health
echo   API Documentation: http://localhost:%API_PORT%/swagger
echo.
echo Service Windows:
echo   - RepoLens-API (Backend API Server)
echo   - RepoLens-Frontend (React Development Server)
echo.
echo [INFO] All services are running in separate windows
echo [INFO] Close the service windows to stop the services
echo [INFO] Press any key to exit this startup window
echo.
pause
exit /b 0
