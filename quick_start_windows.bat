@echo off
REM ========================================
REM VoidWarp Windows - Quick Start
REM ========================================

cd /d "%~dp0"

echo.
echo ========================================
echo VoidWarp Windows - Quick Start
echo ========================================
echo.
echo This will build and run the Windows client.
echo.
echo Options:
echo   [1] Quick Build ^& Run (Debug mode, fast)
echo   [2] Full Build ^& Run (Release mode, optimized)
echo   [3] Just Run (use existing build)
echo   [4] Cancel
echo.
set /p choice="Select option [1-4]: "

if "%choice%"=="1" goto debug
if "%choice%"=="2" goto release
if "%choice%"=="3" goto run
if "%choice%"=="4" goto end

echo Invalid choice!
pause
exit /b 1

:debug
echo.
echo Building Debug version...
echo.
REM Build Rust core (debug)
cd core
cargo build
if errorlevel 1 (
    echo Rust build failed!
    cd ..
    pause
    exit /b 1
)
cd ..

REM Build C# (debug)
cd platforms\windows
dotnet build -c Debug
if errorlevel 1 (
    echo C# build failed!
    cd ..\..
    pause
    exit /b 1
)

REM Run
echo.
echo Launching application...
start "" "bin\Debug\net8.0-windows\VoidWarp.Windows.exe"
cd ..\..
echo.
echo Application started!
goto end

:release
echo.
echo Building Release version...
call build_windows.bat
if errorlevel 1 goto end
echo.
echo Launching application...
cd platforms\windows\bin\Release\net8.0-windows
start "" "VoidWarp.Windows.exe"
cd ..\..\..\..\..
goto end

:run
call run_windows.bat
goto end

:end
echo.
