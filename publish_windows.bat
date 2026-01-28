@echo off
REM ========================================
REM VoidWarp Windows Client - Publish
REM ========================================
echo.
echo ========================================
echo VoidWarp Windows Client - Publish
echo ========================================
echo.

cd /d "%~dp0"

REM Step 1: Build Release version
echo [Step 1/3] Building Release version...
call build_windows.bat
if errorlevel 1 (
    echo ERROR: Build failed!
    exit /b 1
)
echo.

REM Step 2: Create publish directory
echo [Step 2/3] Creating publish package...
set PUBLISH_DIR=publish\VoidWarp-Windows
if exist "%PUBLISH_DIR%" (
    echo   ^> Cleaning old publish directory...
    rmdir /s /q "%PUBLISH_DIR%"
)
mkdir "%PUBLISH_DIR%"
echo   ^> Created: %PUBLISH_DIR%
echo.

REM Step 3: Copy files
echo [Step 3/3] Copying files...

REM Copy executable and DLL
echo   ^> Copying executable...
copy /Y "platforms\windows\bin\Release\net8.0-windows\VoidWarp.Windows.exe" "%PUBLISH_DIR%\" >nul
copy /Y "platforms\windows\bin\Release\net8.0-windows\VoidWarp.Windows.dll" "%PUBLISH_DIR%\" >nul
copy /Y "platforms\windows\bin\Release\net8.0-windows\voidwarp_core.dll" "%PUBLISH_DIR%\" >nul

REM Copy runtime config
copy /Y "platforms\windows\bin\Release\net8.0-windows\*.runtimeconfig.json" "%PUBLISH_DIR%\" >nul 2>&1
copy /Y "platforms\windows\bin\Release\net8.0-windows\*.deps.json" "%PUBLISH_DIR%\" >nul 2>&1

REM Copy firewall script
echo   ^> Copying firewall script...
copy /Y "platforms\windows\setup_firewall.bat" "%PUBLISH_DIR%\firewall_rules.bat" >nul

REM Create README
echo   ^> Creating README...
(
echo VoidWarp Windows Client
echo =======================
echo.
echo 使用方法：
echo   1. 双击运行 VoidWarp.Windows.exe
echo   2. 如果无法发现设备，请以管理员身份运行 firewall_rules.bat
echo.
echo 功能：
echo   - 设备发现（mDNS）
echo   - 文件发送（拖拽或浏览）
echo   - 文件接收（自动监听）
echo   - 实时进度显示
echo   - 日志记录
echo.
echo 系统要求：
echo   - Windows 10/11
echo   - .NET 8.0 Runtime（如未安装，首次运行会提示）
echo.
echo 问题排查：
echo   - 无法发现设备 -^> 运行 firewall_rules.bat
echo   - 缺少 voidwarp_core.dll -^> 确保 DLL 在同一目录
echo   - 端口被占用 -^> 检查防火墙设置
echo.
) > "%PUBLISH_DIR%\README.txt"

REM Copy LICENSE
if exist "LICENSE" (
    copy /Y "LICENSE" "%PUBLISH_DIR%\" >nul
)

echo   ^> Files copied
echo.

REM Show summary
echo ========================================
echo PUBLISH COMPLETED!
echo ========================================
echo.
echo Package location: %PUBLISH_DIR%
echo.
echo Contents:
dir /b "%PUBLISH_DIR%"
echo.
echo To distribute:
echo   1. Compress the folder: %PUBLISH_DIR%
echo   2. Share the ZIP file
echo.
echo To test:
echo   cd %PUBLISH_DIR%
echo   VoidWarp.Windows.exe
echo.
pause
