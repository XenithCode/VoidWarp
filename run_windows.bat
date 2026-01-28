@echo off
REM ========================================
REM Quick Run - VoidWarp Windows Client
REM ========================================

cd /d "%~dp0"

REM Check if release build exists
if exist "platforms\windows\bin\Release\net8.0-windows\VoidWarp.Windows.exe" (
    echo Running Release build...
    cd platforms\windows\bin\Release\net8.0-windows
    start "" "VoidWarp.Windows.exe"
    cd ..\..\..\..\..
    exit /b 0
)

REM Check if debug build exists
if exist "platforms\windows\bin\Debug\net8.0-windows\VoidWarp.Windows.exe" (
    echo Running Debug build...
    cd platforms\windows\bin\Debug\net8.0-windows
    start "" "VoidWarp.Windows.exe"
    cd ..\..\..\..\..
    exit /b 0
)

REM No build found
echo ERROR: No build found!
echo.
echo Please run one of the following first:
echo   - build_windows.bat          (Release build)
echo   - build_windows_debug.bat    (Debug build)
echo.
pause
exit /b 1
