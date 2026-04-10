@echo off
setlocal enabledelayedexpansion

title RepoLens Service Startup (Fixed Paths)

echo ================================================
echo         RepoLens Service Startup (Fixed Paths)
echo ================================================
echo Time: %date% %time%
echo.

:: Configuration
set "API_PORT=5000"
set "FRONTEND_PORT=3000"
set "STARTUP_TIMEOUT=45"
set "ROOT_DIR=%~dp0..\.."

echo [INFO] Starting RepoLens Platform Services
echo [INFO] Root directory: %ROOT_DIR%
echo [INFO] API Port: %API_PORT%
echo [INFO] Frontend Port: %FRONTEND_PORT%
echo.

:: Step 1: Check Prerequisites
echo [STEP 1/6] Checking Prerequisites
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
echo [STEP 2/6] Validating Project Structure
if not exist "%ROOT_DIR%\src\backend\RepoLens.Api" (
    echo [ERROR] RepoLens.Api directory not found at %ROOT_DIR%\src\backend\RepoLens.Api
    pause
    exit /b 1
)
echo [INFO] RepoLens.Api directory found

if not exist "%ROOT_DIR%\src\frontend\repolens-ui" (
    echo [ERROR] repolens-ui directory not found at %ROOT_DIR%\src\frontend\repolens-ui
    pause
    exit /b 1
)
echo [INFO] repolens-ui directory found

if not exist "%ROOT_DIR%\src\backend\RepoLens.sln" (
    echo [ERROR] RepoLens.sln solution file not found at %ROOT_DIR%\src\backend\
    pause
    exit /b 1
)
echo [INFO] RepoLens.sln solution file found
echo [INFO] Project structure validation completed
echo.

:: Step 3: Clean Previous Processes
echo [STEP 3/6] Cleaning Previous Processes
echo [LOG] Killing any existing dotnet and node processes
taskkill /f /im dotnet.exe 2>nul
taskkill /f /im node.exe 2>nul
echo [LOG] Process cleanup completed
echo.

:: Step 4: Compile All Backend Projects
echo [STEP 4/6] Compiling All Backend Projects
cd /d "%ROOT_DIR%\src\backend"
echo [LOG] Current directory: %CD%

echo [LOG] Building Core project...
dotnet build RepoLens.Core/RepoLens.Core.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] Core build failed
    pause
    exit /b 1
)
echo [INFO] Core project compiled successfully

echo [LOG] Building Infrastructure project...
dotnet build RepoLens.Infrastructure/RepoLens.Infrastructure.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] Infrastructure build failed
    pause
    exit /b 1
)
echo [INFO] Infrastructure project compiled successfully

echo [LOG] Building API project...
dotnet build RepoLens.Api/RepoLens.Api.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] API build failed
    pause
    exit /b 1
)
echo [INFO] API project compiled successfully

echo [LOG] Building Worker project...
dotnet build RepoLens.Worker/RepoLens.Worker.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] Worker build failed
    pause
    exit /b 1
)
echo [INFO] Worker project compiled successfully

echo [LOG] Building DatabaseQuery project...
dotnet build DatabaseQuery/DatabaseQuery.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] DatabaseQuery build failed
    pause
    exit /b 1
)
echo [INFO] DatabaseQuery project compiled successfully

echo [LOG] Building SearchApiDemo project...
dotnet build SearchApiDemo/SearchApiDemo.csproj --configuration Release
if %errorlevel% neq 0 (
    echo [ERROR] SearchApiDemo build failed
    pause
    exit /b 1
)
echo [INFO] SearchApiDemo project compiled successfully

echo [INFO] All backend projects compiled successfully
echo.

:: Step 5: Start Backend API
echo [STEP 5/6] Starting Backend API
cd /d "%ROOT_DIR%\src\backend\RepoLens.Api"
echo [LOG] Starting RepoLens API on port %API_PORT%
start "RepoLens-API" cmd /k "echo [BACKEND API] Starting... && dotnet run --configuration Release --urls=http://localhost:%API_PORT%"
echo [LOG] API process started in new window
cd /d "%ROOT_DIR%"

echo [LOG] Starting API health check loop with %STARTUP_TIMEOUT% second timeout
set "wait_time=0"
:wait_api
if %wait_time% geq %STARTUP_TIMEOUT% (
    echo [WARNING] API startup took longer than expected (%wait_time% seconds)
    echo [LOG] Final port check before continuing...
    netstat -an | findstr ":%API_PORT%" 
    echo [INFO] Check the RepoLens-API window for detailed status
    echo [INFO] Continuing with frontend startup...
    goto start_frontend
)

netstat -an | findstr ":%API_PORT% " | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
    echo [SUCCESS] API is running on http://localhost:%API_PORT%
    goto start_frontend
)

echo [LOG] API not ready yet, waiting 3 seconds... (%wait_time%/%STARTUP_TIMEOUT%s)
timeout /t 3 /nobreak >nul
set /a wait_time+=3
goto wait_api

:start_frontend
echo.
:: Step 6: Compile and Start Frontend
echo [STEP 6/6] Compiling and Starting Frontend Application
cd /d "%ROOT_DIR%\src\frontend\repolens-ui"
echo [LOG] Current directory for frontend: %CD%

echo [LOG] Installing frontend dependencies with npm install
npm install
set "npm_result=%errorlevel%"
if %npm_result% neq 0 (
    echo [WARNING] npm install encountered issues (exit code %npm_result%), continuing...
) else (
    echo [LOG] npm install completed successfully
)

echo [LOG] Building frontend for production...
npm run build
set "build_result=%errorlevel%"
if %build_result% neq 0 (
    echo [WARNING] npm build encountered issues (exit code %build_result%), starting dev server anyway...
) else (
    echo [LOG] Frontend build completed successfully
)

echo [LOG] Starting React development server on port %FRONTEND_PORT%
start "RepoLens-Frontend" cmd /k "echo [FRONTEND] Starting React dev server... && npm start"
echo [LOG] Frontend process started in new window
cd /d "%ROOT_DIR%"

echo [LOG] Starting frontend health check loop with %STARTUP_TIMEOUT% second timeout
set "wait_time=0"
:wait_frontend
if %wait_time% geq %STARTUP_TIMEOUT% (
    echo [WARNING] Frontend startup took longer than expected (%wait_time% seconds)
    echo [LOG] Final port check before continuing...
    netstat -an | findstr ":%FRONTEND_PORT%"
    echo [INFO] Check the RepoLens-Frontend window for detailed status
    goto services_summary
)

netstat -an | findstr ":%FRONTEND_PORT% " | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
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
echo         All Services Compiled and Started
echo ================================================
echo.
echo Compiled Projects:
echo   Backend:
echo     - RepoLens.Core
echo     - RepoLens.Infrastructure  
echo     - RepoLens.Api
echo     - RepoLens.Worker
echo     - DatabaseQuery
echo     - SearchApiDemo
echo   Frontend:
echo     - repolens-ui
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
