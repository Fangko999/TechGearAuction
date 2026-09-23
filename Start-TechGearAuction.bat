@echo off
echo ===================================================
echo     TechGearAuction - System Startup Script
echo ===================================================

echo [1/3] Starting Docker Services (SQL Server, MinIO)...
docker-compose up -d sqlserver minio
if %errorlevel% neq 0 (
    echo [ERROR] Failed to start Docker services. Make sure Docker Desktop is running.
    pause
    exit /b %errorlevel%
)
echo Docker services started successfully.
echo.

echo [2/3] Starting Backend API...
start "TechGearAuction Backend" cmd /c "title Backend API && cd TechGearAuction.API && dotnet run"
echo Backend is starting in a new window...
echo.

echo [3/3] Starting Frontend Next.js...
start "TechGearAuction Frontend" cmd /c "title Frontend UI && cd frontend && npm install && npm run dev"
echo Frontend is starting in a new window...
echo.

echo ===================================================
echo All services are starting!
echo - Backend API will be at: http://localhost:8888
echo - Frontend UI will be at: http://localhost:3000
echo - MinIO Console will be at: http://localhost:9001 (minioadmin / minioadmin)
echo.
echo Please wait about 15-30 seconds for the backend to run migrations and seed data.
echo You can then open your browser and navigate to http://localhost:3000
echo ===================================================
pause

