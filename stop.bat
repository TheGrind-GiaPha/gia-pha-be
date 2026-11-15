@echo off
setlocal
chcp 65001 >nul
title Dung he thong Gia Pha

echo ====================================================================
echo        DUNG HE THONG QUAN LY GIA PHA (PORT 5000 & 5173)
echo ====================================================================

echo [1/2] Dang giai phong port 5000 (Backend)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5000 ^| findstr LISTENING') do (
    echo Dang tat PID: %%a
    taskkill /f /pid %%a >nul 2>&1
)

echo [2/2] Dang giai phong port 5173 (Frontend)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5173 ^| findstr LISTENING') do (
    echo Dang tat PID: %%a
    taskkill /f /pid %%a >nul 2>&1
)

echo --------------------------------------------------------------------
echo Da dung toan bo dich vu Backend va Frontend!
pause
endlocal
exit /b 0
