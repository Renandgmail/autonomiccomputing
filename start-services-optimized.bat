@echo off
setlocal enabledelayedexpansion
title RepoLens Service Monitor - Optimized

:: Configuration
set "BACKEND_PORT=5179"
set "FRONTEND_PORT=3000"
set "CHECK_INTERVAL=10"
set "RESTART_DELAY=5"
set "MAX_RESTART_ATTEMPTS=3"
set "HEALTH_CHECK_TIMEOUT=30"

:: Counters for restart attempts
set "BACKEND_RESTARTS=0"
set "FRONTEND_RESTARTS=0"

echo =========================================
echo    RepoLens Service Monitor - Optimized
echo =========================================
echo Backend: .NET API (Port %BACKEND_PORT%)
echo Frontend: React App (Port %FRONTEND_PORT%)
echo Check Interval: %CHECK_INTERVAL%s
echo =========================================
echo.

:check_existing_services
echo [%TIME%] Checking for existing services...
:: Just check what's running, don't kill everything automatically
netstat -an | findstr ":%BACKEND_PORT% " | findstr "LISTENING" >nul
if not errorlevel 1 (
    echo [INFO] Backend service appears to be running on port %BACKEND_PORT%
)

netstat -an | findstr ":%FRONTEND_PORT% " | findstr "LISTENING" >nul
if not errorlevel 1 (
    echo [INFO] Frontend service appears to be running on port %FRONTEND_PORT%
)

:check_dependencies
echo [%TIME%] Verifying dependencies...

:: Check if .NET is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found. Please install .NET SDK.
    goto :error_exit
)

:: Check if Node.js is available
node --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Node.js not found. Please install Node.js.
    goto :error_exit
)

:: Check if project directories exist
if not exist "RepoLens.Api" (
    echo [ERROR] Backend directory 'RepoLens.Api' not found.
    goto :error_exit
)

if not exist "repolens-ui" (
    echo [ERROR] Frontend directory 'repolens-ui' not found.
    goto :error_exit
)

echo [%TIME%] Dependencies verified successfully.
echo.

:check_ports
echo [%TIME%] Checking port status...
:: Just informational - don't kill processes yet
netstat -an | findstr :%BACKEND_PORT% >nul
if not errorlevel 1 (
    echo [INFO] Port %BACKEND_PORT% is currently in use - will check if service is healthy
)

netstat -an | findstr :%FRONTEND_PORT% >nul
if not errorlevel 1 (
    echo [INFO] Port %FRONTEND_PORT% is currently in use - will check if service is healthy
)

:start_services
echo [%TIME%] Starting services...

:start_backend
:: Check if backend is already running and healthy
call :check_service_health %BACKEND_PORT% "Backend"
if not errorlevel 1 (
    echo [%TIME%] Backend API is already running and healthy on port %BACKEND_PORT%
    goto :start_frontend
)

echo [%TIME%] Starting Backend API...
cd /d "%~dp0RepoLens.Api"
start "RepoLens-Backend" /min cmd /c "dotnet run"
cd /d "%~dp0"

:: Wait for backend to start and verify
call :wait_for_service %BACKEND_PORT% "Backend API" %HEALTH_CHECK_TIMEOUT%
if errorlevel 1 (
    set /a BACKEND_RESTARTS+=1
    if !BACKEND_RESTARTS! leq %MAX_RESTART_ATTEMPTS% (
        echo [WARNING] Backend failed to start. Attempt !BACKEND_RESTARTS!/%MAX_RESTART_ATTEMPTS%
        timeout /t %RESTART_DELAY% /nobreak >nul
        goto :start_backend
    ) else (
        echo [ERROR] Backend failed to start after %MAX_RESTART_ATTEMPTS% attempts.
        goto :error_exit
    )
)
echo [%TIME%] Backend API is ready on http://localhost:%BACKEND_PORT%

:start_frontend
:: Check if frontend is already running and healthy
call :check_service_health %FRONTEND_PORT% "Frontend"
if not errorlevel 1 (
    echo [%TIME%] Frontend is already running and healthy on port %FRONTEND_PORT%
    goto :services_ready
)

echo [%TIME%] Starting Frontend React App...
cd /d "%~dp0repolens-ui"
start "RepoLens-Frontend" /min cmd /c "npm start"
cd /d "%~dp0"

:: Wait for frontend to start and verify
call :wait_for_service %FRONTEND_PORT% "Frontend" %HEALTH_CHECK_TIMEOUT%
if errorlevel 1 (
    set /a FRONTEND_RESTARTS+=1
    if !FRONTEND_RESTARTS! leq %MAX_RESTART_ATTEMPTS% (
        echo [WARNING] Frontend failed to start. Attempt !FRONTEND_RESTARTS!/%MAX_RESTART_ATTEMPTS%
        timeout /t %RESTART_DELAY% /nobreak >nul
        goto :start_frontend
    ) else (
        echo [ERROR] Frontend failed to start after %MAX_RESTART_ATTEMPTS% attempts.
        goto :error_exit
    )
)
echo [%TIME%] Frontend is ready on http://localhost:%FRONTEND_PORT%

:services_ready

echo.
echo =========================================
echo ✅ SERVICES RUNNING SUCCESSFULLY
echo =========================================
echo - Backend API: http://localhost:%BACKEND_PORT%
echo - Frontend: http://localhost:%FRONTEND_PORT%
echo =========================================
echo.
echo [%TIME%] Starting health monitoring...
echo Press Ctrl+C to stop all services
echo.

:monitor_loop
timeout /t %CHECK_INTERVAL% /nobreak >nul

:: Check backend health
call :check_service_health %BACKEND_PORT% "Backend"
if errorlevel 1 (
    echo [%TIME%] Backend health check failed! Restarting...
    call :restart_service "backend"
)

:: Check frontend health  
call :check_service_health %FRONTEND_PORT% "Frontend"
if errorlevel 1 (
    echo [%TIME%] Frontend health check failed! Restarting...
    call :restart_service "frontend"
)

goto :monitor_loop

:: ============================================
:: UTILITY FUNCTIONS
:: ============================================

:wait_for_service
:: Parameters: %1=port, %2=service_name, %3=timeout_seconds
set "port=%1"
set "service_name=%2"
set "timeout_sec=%3"
set "wait_time=0"

:wait_loop
if %wait_time% geq %timeout_sec% (
    echo [ERROR] %service_name% did not start within %timeout_sec% seconds
    exit /b 1
)

:: Check if port is listening
netstat -an | findstr ":%port% " | findstr "LISTENING" >nul
if errorlevel 1 (
    echo [INFO] Waiting for %service_name% on port %port%... (%wait_time%/%timeout_sec%s)
    timeout /t 2 /nobreak >nul
    set /a wait_time+=2
    goto :wait_loop
)

echo [SUCCESS] %service_name% is listening on port %port%
exit /b 0

:check_service_health
:: Parameters: %1=port, %2=service_name
set "port=%1"
set "service_name=%2"

:: Check if port is still listening
netstat -an | findstr ":%port% " | findstr "LISTENING" >nul
if errorlevel 1 (
    echo [ERROR] %service_name% port %port% is not listening
    exit /b 1
)

:: Try a simple HTTP connection test
curl -s --connect-timeout 3 http://localhost:%port% >nul 2>&1
if errorlevel 1 (
    echo [WARNING] %service_name% port %port% not responding to HTTP requests
    exit /b 1
)

exit /b 0

:restart_service
:: Parameters: %1=service_type (backend/frontend)
set "service_type=%1"

if /i "%service_type%"=="backend" (
    echo [%TIME%] Killing backend processes...
    for /f "tokens=5" %%i in ('netstat -ano ^| findstr :%BACKEND_PORT%') do (
        taskkill /pid %%i /f >nul 2>&1
    )
    taskkill /fi "windowtitle eq RepoLens-Backend*" /f >nul 2>&1
    timeout /t %RESTART_DELAY% /nobreak >nul
    
    echo [%TIME%] Restarting Backend API...
    cd /d "%~dp0RepoLens.Api"
    start "RepoLens-Backend" /min cmd /c "dotnet run"
    cd /d "%~dp0"
    
    call :wait_for_service %BACKEND_PORT% "Backend API" %HEALTH_CHECK_TIMEOUT%
    if not errorlevel 1 (
        echo [SUCCESS] Backend API restarted successfully
    )
)

if /i "%service_type%"=="frontend" (
    echo [%TIME%] Killing frontend processes...
    for /f "tokens=5" %%i in ('netstat -ano ^| findstr :%FRONTEND_PORT%') do (
        taskkill /pid %%i /f >nul 2>&1
    )
    taskkill /fi "windowtitle eq RepoLens-Frontend*" /f >nul 2>&1
    timeout /t %RESTART_DELAY% /nobreak >nul
    
    echo [%TIME%] Restarting Frontend...
    cd /d "%~dp0repolens-ui"
    start "RepoLens-Frontend" /min cmd /c "npm start"
    cd /d "%~dp0"
    
    call :wait_for_service %FRONTEND_PORT% "Frontend" %HEALTH_CHECK_TIMEOUT%
    if not errorlevel 1 (
        echo [SUCCESS] Frontend restarted successfully
    )
)

exit /b 0

:error_exit
echo.
echo =========================================
echo ❌ SERVICE MONITOR FAILED
echo =========================================
echo Please check the error messages above.
echo =========================================
pause
exit /b 1

:cleanup_exit
echo.
echo [%TIME%] Shutting down all services...
:: Kill backend processes
for /f "tokens=5" %%i in ('netstat -ano ^| findstr :%BACKEND_PORT%') do (
    taskkill /pid %%i /f >nul 2>&1
)
:: Kill frontend processes  
for /f "tokens=5" %%i in ('netstat -ano ^| findstr :%FRONTEND_PORT%') do (
    taskkill /pid %%i /f >nul 2>&1
)
taskkill /fi "windowtitle eq RepoLens-Backend*" /f >nul 2>&1
taskkill /fi "windowtitle eq RepoLens-Frontend*" /f >nul 2>&1

echo [%TIME%] All services stopped.
exit /b 0
