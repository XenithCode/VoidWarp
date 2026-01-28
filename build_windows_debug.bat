@echo off
REM ========================================
REM VoidWarp Windows Client - Debug Build
REM ========================================
echo.
echo ========================================
echo VoidWarp Windows Client - Debug Build
echo ========================================
echo.

cd /d "%~dp0"

REM Step 1: Build Rust core library (Debug mode)
echo [Step 1/3] Building Rust core library (Debug)...
cd core
echo   ^> Running: cargo build
cargo build
if errorlevel 1 (
    echo ERROR: Rust build failed!
    echo.
    cd ..
    pause
    exit /b 1
)
cd ..
echo   ^> Rust core built: target\debug\voidwarp_core.dll
echo.

REM Step 2: Copy DLL to Windows debug output (if needed)
echo [Step 2/3] Copying DLL to debug output...
if not exist "platforms\windows\bin\Debug\net8.0-windows" (
    mkdir "platforms\windows\bin\Debug\net8.0-windows"
)
copy /Y "target\debug\voidwarp_core.dll" "platforms\windows\bin\Debug\net8.0-windows\voidwarp_core.dll" >nul
echo   ^> DLL copied
echo.

REM Step 3: Build C# Windows client (Debug)
echo [Step 3/3] Building C# Windows client (Debug)...
cd platforms\windows
echo   ^> Running: dotnet build -c Debug
dotnet build -c Debug
if errorlevel 1 (
    echo ERROR: C# build failed!
    echo.
    cd ..\..
    pause
    exit /b 1
)
cd ..\..
echo   ^> C# client built
echo.

echo ========================================
echo DEBUG BUILD COMPLETED!
echo ========================================
echo.
echo Output: platforms\windows\bin\Debug\net8.0-windows\VoidWarp.Windows.exe
echo.
echo Press any key to run the application...
pause >nul
cd platforms\windows\bin\Debug\net8.0-windows
start "" "VoidWarp.Windows.exe"
cd ..\..\..\..\..
