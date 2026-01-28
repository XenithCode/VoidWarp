# VoidWarp Windows Client - 构建指南

## 📋 前置要求

### 必需工具
1. **Rust 工具链** (1.70+)
   - 安装：https://rustup.rs/
   - 验证：`cargo --version`

2. **.NET 8.0 SDK**
   - 安装：https://dotnet.microsoft.com/download/dotnet/8.0
   - 验证：`dotnet --version`

3. **Visual Studio 2022** 或 **VS Build Tools** (可选，但推荐)
   - 工作负载：".NET 桌面开发"

### 系统要求
- Windows 10/11 (x64)
- 至少 2GB 可用磁盘空间

---

## 🚀 快速开始

### 方法 1：一键构建运行 (推荐新手)

```bash
# 在项目根目录运行
quick_start_windows.bat
```

选择选项：
- `[1]` Debug 模式（快速编译，适合开发）
- `[2]` Release 模式（优化编译，适合发布）
- `[3]` 仅运行（使用现有构建）

---

### 方法 2：手动构建

#### Step 1: 编译 Rust 核心库

```bash
cd core
cargo build --release
```

**输出**：`target/release/voidwarp_core.dll`

#### Step 2: 编译 C# 客户端

```bash
cd platforms/windows
dotnet build -c Release
```

**输出**：`platforms/windows/bin/Release/net8.0-windows/VoidWarp.Windows.exe`

#### Step 3: 运行应用

```bash
cd platforms/windows/bin/Release/net8.0-windows
VoidWarp.Windows.exe
```

---

### 方法 3：使用构建脚本

#### Release 版本（推荐）
```bash
# 项目根目录
build_windows.bat
```

#### Debug 版本（开发调试）
```bash
# 项目根目录
build_windows_debug.bat
```

#### 发布打包
```bash
# 项目根目录
publish_windows.bat
```

生成的发布包位于：`publish/VoidWarp-Windows/`

---

## 📁 项目结构

```
VoidWarp/
├── core/                          # Rust 核心库
│   ├── src/
│   │   ├── lib.rs                # FFI 接口
│   │   ├── transport.rs          # TCP 传输
│   │   └── ...
│   └── Cargo.toml
│
├── target/
│   ├── debug/
│   │   └── voidwarp_core.dll    # Debug DLL
│   └── release/
│       └── voidwarp_core.dll    # Release DLL
│
├── platforms/windows/             # C# WPF 客户端
│   ├── Core/
│   │   ├── VoidWarpEngine.cs    # 引擎封装
│   │   ├── VoidWarpClient.cs    # 高层 API
│   │   ├── TransferManager.cs   # 发送管理
│   │   └── ReceiveManager.cs    # 接收管理
│   ├── Native/
│   │   └── NativeBindings.cs    # P/Invoke 绑定
│   ├── ViewModels/
│   │   └── MainViewModel.cs     # UI 逻辑
│   ├── MainWindow.xaml          # UI 界面
│   ├── bin/
│   │   ├── Debug/
│   │   └── Release/
│   └── VoidWarp.Windows.csproj
│
├── build_windows.bat              # Release 构建脚本
├── build_windows_debug.bat        # Debug 构建脚本
├── run_windows.bat                # 快速运行脚本
├── publish_windows.bat            # 发布打包脚本
└── quick_start_windows.bat        # 一键启动脚本
```

---

## 🔧 故障排除

### 问题 1: "cargo: command not found"
**原因**：未安装 Rust 工具链

**解决**：
1. 访问 https://rustup.rs/
2. 下载并运行安装器
3. 重启命令行
4. 验证：`cargo --version`

---

### 问题 2: "dotnet: command not found"
**原因**：未安装 .NET SDK

**解决**：
1. 访问 https://dotnet.microsoft.com/download/dotnet/8.0
2. 下载并安装 ".NET 8.0 SDK"
3. 重启命令行
4. 验证：`dotnet --version`

---

### 问题 3: "无法加载 voidwarp_core.dll"
**原因**：DLL 未复制到输出目录

**解决方案 A**：手动复制
```bash
copy target\release\voidwarp_core.dll platforms\windows\bin\Release\net8.0-windows\
```

**解决方案 B**：检查 csproj 配置
确保 `VoidWarp.Windows.csproj` 包含：
```xml
<ItemGroup>
  <None Include="..\..\target\release\voidwarp_core.dll" 
        CopyToOutputDirectory="PreserveNewest" 
        Link="voidwarp_core.dll" />
</ItemGroup>
```

**解决方案 C**：完全重新构建
```bash
# 清理
dotnet clean
cd core
cargo clean
cd ..

# 重新构建
build_windows.bat
```

---

### 问题 4: "无法发现设备"
**原因**：防火墙阻止了 UDP mDNS (端口 5353)

**解决**：
```bash
# 以管理员身份运行
platforms\windows\setup_firewall.bat
```

或者手动添加防火墙规则：
```powershell
# 允许 UDP 5353 (mDNS)
New-NetFirewallRule -DisplayName "VoidWarp mDNS" -Direction Inbound -Protocol UDP -LocalPort 5353 -Action Allow

# 允许应用程序
New-NetFirewallRule -DisplayName "VoidWarp App" -Program "完整\路径\VoidWarp.Windows.exe" -Action Allow
```

---

### 问题 5: "Rust 编译错误"
**常见原因**：依赖问题

**解决**：
```bash
cd core
cargo clean
cargo update
cargo build --release
```

---

### 问题 6: "C# 编译错误"
**常见原因**：目标框架不匹配

**检查**：
1. 确认已安装 .NET 8.0 SDK：`dotnet --list-sdks`
2. 如果只有 .NET 6/7，升级到 .NET 8

**临时解决**：修改 `VoidWarp.Windows.csproj`
```xml
<!-- 从 net8.0-windows 改为你已安装的版本 -->
<TargetFramework>net8.0-windows</TargetFramework>
```

---

### 问题 7: "应用启动后立即崩溃"
**排查步骤**：

1. **检查 DLL 是否存在**
```bash
dir platforms\windows\bin\Release\net8.0-windows\voidwarp_core.dll
```

2. **查看详细错误**
```bash
cd platforms\windows\bin\Release\net8.0-windows
VoidWarp.Windows.exe
# 查看控制台输出
```

3. **检查事件查看器**
   - 打开 Windows 事件查看器
   - Windows 日志 > 应用程序
   - 查找 VoidWarp 相关错误

4. **运行 Debug 版本**
```bash
build_windows_debug.bat
# Debug 版本会输出更多信息
```

---

## 🎯 开发工作流

### 日常开发流程

```bash
# 1. 修改 Rust 代码
cd core
cargo build              # 快速编译 (Debug)
cd ..

# 2. 修改 C# 代码
cd platforms/windows
dotnet build -c Debug
cd ..\..

# 3. 运行测试
run_windows.bat
```

### 准备发布

```bash
# 1. 完整构建
build_windows.bat

# 2. 测试
run_windows.bat

# 3. 打包
publish_windows.bat

# 4. 测试打包版本
cd publish\VoidWarp-Windows
VoidWarp.Windows.exe
```

---

## 📦 发布检查清单

构建发布版本前，确认：

- [ ] Rust 核心已用 `--release` 编译
- [ ] C# 项目已用 `-c Release` 编译
- [ ] `voidwarp_core.dll` 已复制到输出目录
- [ ] 应用能正常启动
- [ ] 设备发现功能正常
- [ ] 文件发送功能正常
- [ ] 文件接收功能正常
- [ ] 防火墙脚本包含在发布包中
- [ ] README.txt 已创建

---

## 🐛 调试技巧

### 启用 Rust 日志
```bash
set RUST_LOG=debug
VoidWarp.Windows.exe
```

### 启用 C# 详细日志
在 `MainViewModel.cs` 中，所有操作都会记录到日志面板。

### 检查 DLL 加载
在 `NativeBindings.cs` 中，已添加详细的 Debug 输出：
```csharp
System.Diagnostics.Debug.WriteLine($"[NativeBindings] Trying: {candidatePath}");
```

使用 DebugView (https://docs.microsoft.com/en-us/sysinternals/downloads/debugview) 查看这些消息。

---

## 📝 附录

### 构建脚本说明

| 脚本名称 | 用途 | 编译模式 |
|---------|------|---------|
| `build_windows.bat` | 标准 Release 构建 | Release |
| `build_windows_debug.bat` | 开发调试构建 | Debug |
| `run_windows.bat` | 快速运行现有构建 | - |
| `publish_windows.bat` | 打包发布版本 | Release |
| `quick_start_windows.bat` | 一键启动菜单 | 可选 |

### 性能对比

| 编译模式 | 编译时间 | 运行性能 | 文件大小 | 适用场景 |
|---------|---------|---------|---------|---------|
| Debug | ~30 秒 | 较慢 | 较大 | 日常开发 |
| Release | ~2 分钟 | 最优 | 最小 | 正式发布 |

---

## 📞 获取帮助

如果遇到本文档未涵盖的问题：

1. 检查日志面板（应用右侧）
2. 查看 Debug 输出（DebugView）
3. 检查 Windows 事件查看器
4. 提交 Issue 到项目仓库

---

**祝编译顺利！** 🎉
