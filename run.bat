@echo off
setlocal
chcp 65001 >nul
title Khoi dong he thong Gia Pha (Fullstack)

echo ====================================================================
echo        HE THONG QUAN LY GIA PHA - KHOI DONG FULLSTACK
echo ====================================================================

set "CURRENT_DIR=%~dp0"
if exist "%CURRENT_DIR%gia-pha-be" (
    set "BE_DIR=%CURRENT_DIR%gia-pha-be"
    set "FE_DIR=%CURRENT_DIR%gia-pha-fe"
) else if exist "%CURRENT_DIR%APIs\GiaPha.APIs.csproj" (
    set "BE_DIR=%CURRENT_DIR%"
    set "FE_DIR=%CURRENT_DIR%..\gia-pha-fe"
) else (
    set "BE_DIR=D:\Download\gia-pha-be"
    set "FE_DIR=D:\Download\gia-pha-fe"
)

echo [BE Path]: %BE_DIR%
echo [FE Path]: %FE_DIR%
echo --------------------------------------------------------------------

if not exist "%FE_DIR%\node_modules" (
    echo [Frontend] Chua tim thay node_modules, dang cai dat dependencies...
    cd /d "%FE_DIR%"
    call npm install
)

echo [1/2] Dang khoi dong Backend (.NET 8 Web API - Port 5000)...
start "GiaPha Backend (Port 5000)" cmd /k "cd /d "%BE_DIR%" && dotnet run --project APIs/GiaPha.APIs.csproj --launch-profile http"

timeout /t 3 /nobreak >nul

echo [2/2] Dang khoi dong Frontend (React 19 + Vite - Port 5173)...
start "GiaPha Frontend (Port 5173)" cmd /k "cd /d "%FE_DIR%" && npm run dev"

timeout /t 2 /nobreak >nul

echo ====================================================================
echo                 KHOI DONG THANH CONG!
echo ====================================================================
echo  - Frontend Web UI:    http://localhost:5173
echo  - Backend Swagger:    http://localhost:5000/swagger
echo  - Hangfire Dashboard: http://localhost:5000/hangfire
echo ====================================================================
echo Dang mo trinh duyet...

start http://localhost:5173

endlocal
exit /b 0
