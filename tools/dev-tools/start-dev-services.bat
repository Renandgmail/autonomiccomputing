@echo off
title RepoLens Development Services

echo ========================================
echo    RepoLens Development Services
echo ========================================
echo Starting in hot deployment mode...
echo Time: %date% %time%
echo.

:: Kill any existing processes first
echo [INFO] Cleaning up existing processes...
taskkill /f /im dotnet.exe >nul 2>&1
taskkill /f /im node.exe >nul 2>&1
echo [INFO] Process cleanup completed
echo.

:: Start Backend API with hot reload
echo [INFO] Starting Backend API with hot reload...
cd /d "%~dp0RepoLens.Api"
echo [INFO] Current directory: %CD%
echo [INFO] Starting dotnet watch for hot reload on port 5000...
start "RepoLens-Backend-HotReload" cmd /k "echo [BACKEND] Starting with hot reload && dotnet watch run --urls=http://localhost:5000"

:: Wait a moment for backend to initialize
echo [INFO] Waiting 3 seconds for backend to initialize...
timeout /t 3 /nobreak >nul

:: Go back to root and start Frontend with hot reload
cd /d "%~dp0"
echo [INFO] Starting Frontend with hot reload...
cd /d "%~dp0repolens-ui"
echo [INFO] Current directory: %CD%
echo [INFO] Installing npm dependencies if needed...
if not exist "node_modules" (
    echo [INFO] Installing npm packages...
    npm install
)
echo [INFO] Starting React development server with hot reload on port 3000...
start "RepoLens-Frontend-HotReload" cmd /k "echo [FRONTEND] Starting with hot reload && npm start"

:: Return to root directory
cd /d "%~dp0"
echo.
echo ========================================
echo    Services Started Successfully
echo ========================================
echo.
echo [INFO] Both services are running with hot reload:
echo   Backend API:  http://localhost:5000
echo   Frontend:     http://localhost:3000
echo   API Health:   http://localhost:5000/health
echo   API Docs:     http://localhost:5000/swagger
echo.
echo [INFO] Hot deployment features:
echo   - Backend: dotnet watch automatically rebuilds on code changes
echo   - Frontend: React dev server reloads on file changes
echo.
echo [INFO] Service windows are now running in background
echo [INFO] Close the service windows to stop the services
echo [INFO] Press any key to exit this startup window
echo.
pause
exit
