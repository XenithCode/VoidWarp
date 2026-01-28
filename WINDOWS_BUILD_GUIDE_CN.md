# VoidWarp Windows 客户端 - 编译指南

## ✅ 编译成功确认

当前编译状态：**✓ 编译完成**

生成的文件位于：`platforms/windows/bin/Release/net8.0-windows/`

### 输出文件清单

| 文件名 | 大小 | 说明 |
|-------|------|------|
| `VoidWarp.Windows.exe` | 150 KB | ✓ Windows 客户端主程序 |
| `VoidWarp.Windows.dll` | 90 KB | ✓ C# 程序集 |
| `voidwarp_core.dll` | 1.4 MB | ✓ Rust 核心库（已自动复制） |
| `VoidWarp.Windows.runtimeconfig.json` | 0.5 KB | ✓ 运行时配置 |
| `VoidWarp.Windows.deps.json` | 0.4 KB | ✓ 依赖配置 |

---

## 🚀 三种快速使用方式

### 方式 1️⃣：一键启动（最简单）

```bash
# 在项目根目录双击运行
quick_start_windows.bat
```

### 方式 2️⃣：直接运行

```bash
# 在项目根目录执行
run_windows.bat
```

### 方式 3️⃣：手动运行

```bash
cd platforms\windows\bin\Release\net8.0-windows
VoidWarp.Windows.exe
```

---

## 📦 发布打包

如需将应用分发给其他用户：

```bash
# 在项目根目录执行
publish_windows.bat
```

打包后的完整应用位于：`publish/VoidWarp-Windows/`

包含内容：
- ✅ 可执行文件
- ✅ 所有必需的 DLL
- ✅ 防火墙配置脚本
- ✅ README 说明文档

---

## 🔄 日常开发流程

### 修改代码后重新编译

#### 仅修改了 C# 代码：
```bash
cd platforms\windows
dotnet build -c Release
```

#### 修改了 Rust 核心：
```bash
# 1. 编译 Rust
cd core
cargo build --release

# 2. 编译 C#
cd ..\platforms\windows
dotnet build -c Release
```

#### 使用快速脚本：
```bash
# 完整构建（Rust + C#）
build_windows.bat

# 仅 Debug 快速编译
build_windows_debug.bat
```

---

## 🎯 构建脚本速查表

| 脚本 | 用途 | 速度 |
|-----|------|-----|
| `quick_start_windows.bat` | 🚀 一键菜单（推荐） | - |
| `build_windows.bat` | 📦 完整 Release 构建 | ⭐⭐ |
| `build_windows_debug.bat` | 🔧 Debug 快速构建 | ⭐⭐⭐ |
| `run_windows.bat` | ▶️ 运行现有构建 | ⭐⭐⭐⭐⭐ |
| `publish_windows.bat` | 📤 打包发布版本 | ⭐ |

---

## 🛠️ 编译命令详解

### 完整手动编译流程

```bash
# 步骤 1：编译 Rust 核心库（Release 模式）
cd core
cargo build --release
# 输出：target/release/voidwarp_core.dll

# 步骤 2：编译 C# 客户端（Release 模式）
cd ..\platforms\windows
dotnet build -c Release
# 输出：bin/Release/net8.0-windows/VoidWarp.Windows.exe

# 步骤 3：运行
cd bin\Release\net8.0-windows
VoidWarp.Windows.exe
```

### Debug 版本（开发调试用）

```bash
# 步骤 1：编译 Rust（Debug 更快）
cd core
cargo build
# 输出：target/debug/voidwarp_core.dll

# 步骤 2：编译 C#（Debug 模式）
cd ..\platforms\windows
dotnet build -c Debug
# 输出：bin/Debug/net8.0-windows/VoidWarp.Windows.exe

# 步骤 3：运行
cd bin\Debug\net8.0-windows
VoidWarp.Windows.exe
```

---

## 🔍 验证编译结果

### 检查 DLL 是否正确复制

```powershell
# 检查 Release 版本
dir platforms\windows\bin\Release\net8.0-windows\voidwarp_core.dll

# 检查 Debug 版本
dir platforms\windows\bin\Debug\net8.0-windows\voidwarp_core.dll
```

### 测试应用启动

```bash
# 方式 1：使用脚本
run_windows.bat

# 方式 2：直接运行
cd platforms\windows\bin\Release\net8.0-windows
VoidWarp.Windows.exe
```

### 验证功能清单

启动应用后，检查以下功能：

- [ ] ✅ 应用正常启动（无错误提示）
- [ ] ✅ 设备 ID 显示正常
- [ ] ✅ 点击"开始发现设备"能扫描到设备
- [ ] ✅ 右侧日志面板显示消息
- [ ] ✅ "接收模式"可以切换
- [ ] ✅ "浏览..."按钮可以选择文件
- [ ] ✅ 拖拽文件到中间区域有响应

---

## ❗ 常见问题速查

### 问题 1：缺少 voidwarp_core.dll

**症状**：启动时提示"无法加载 voidwarp_core.dll"

**原因**：Rust DLL 未编译或未复制

**解决**：
```bash
# 重新编译 Rust
cd core
cargo build --release

# 手动复制 DLL
copy target\release\voidwarp_core.dll platforms\windows\bin\Release\net8.0-windows\
```

---

### 问题 2：无法发现设备

**症状**：点击"开始发现设备"后，设备列表一直为空

**原因**：防火墙阻止 UDP 5353 端口

**解决**：
```bash
# 以管理员身份运行
platforms\windows\setup_firewall.bat
```

或在应用内点击右下角"🔧 诊断"按钮。

---

### 问题 3：编译时提示"Access denied"

**症状**：`dotnet build` 时提示"Access to path denied"

**原因**：文件被占用或权限不足

**解决**：
```bash
# 方式 1：关闭所有打开的 VoidWarp 进程
taskkill /F /IM VoidWarp.Windows.exe

# 方式 2：清理并重新构建
dotnet clean
dotnet build -c Release
```

---

### 问题 4：Rust 编译失败

**症状**：`cargo build` 报错

**解决**：
```bash
cd core
cargo clean      # 清理缓存
cargo update     # 更新依赖
cargo build --release
```

---

## 📊 性能对比

| 编译模式 | Rust 编译 | C# 编译 | DLL 大小 | 启动速度 | 运行速度 |
|---------|----------|---------|---------|---------|---------|
| Debug | ~20 秒 | ~3 秒 | 2.5 MB | 慢 | 慢 |
| Release | ~120 秒 | ~3 秒 | 1.4 MB | 快 | 快 |

**推荐**：
- 日常开发：使用 Debug 模式（编译快）
- 测试/发布：使用 Release 模式（性能好）

---

## 🎓 进阶技巧

### 并行编译（加速）

如果你的 CPU 有多核心：

```bash
# Rust 并行编译（使用所有核心）
cd core
cargo build --release -j 8  # 8 是线程数

# C# 并行编译（默认已启用）
cd ..\platforms\windows
dotnet build -c Release -m
```

### 条件编译

```bash
# 仅编译但不链接（检查语法）
cargo check

# 仅编译 Rust，不编译 C#
cd core
cargo build --release

# 仅编译 C#（假设 Rust DLL 已存在）
cd platforms\windows
dotnet build -c Release
```

### 清理构建缓存

```bash
# 清理 Rust 缓存（释放空间）
cd core
cargo clean

# 清理 C# 缓存
cd ..\platforms\windows
dotnet clean
```

---

## 📞 获取帮助

如遇到问题：

1. **查看日志**：启动应用后，右侧面板会显示详细日志
2. **查看文档**：`platforms/windows/BUILD.md`（详细构建指南）
3. **查看重构说明**：`platforms/windows/REFACTOR_SUMMARY.md`（架构说明）

---

## ✅ 快速检查清单

编译前确认：
- [ ] 已安装 Rust (`cargo --version`)
- [ ] 已安装 .NET 8.0 SDK (`dotnet --version`)
- [ ] 在项目根目录 (`g:\project\VoidWarp`)

编译后确认：
- [ ] `target\release\voidwarp_core.dll` 存在（1.4 MB）
- [ ] `platforms\windows\bin\Release\net8.0-windows\VoidWarp.Windows.exe` 存在
- [ ] `platforms\windows\bin\Release\net8.0-windows\voidwarp_core.dll` 存在

运行时确认：
- [ ] 应用正常启动
- [ ] 日志面板显示"VoidWarp 已启动"
- [ ] 设备 ID 正常显示
- [ ] 接收模式端口号显示

---

## 🎉 下一步

编译成功后，你可以：

1. **测试功能**：
   - 在局域网内运行两个设备
   - 测试文件发送和接收

2. **分发应用**：
   ```bash
   publish_windows.bat
   ```
   将 `publish/VoidWarp-Windows/` 文件夹压缩为 ZIP 分享

3. **继续开发**：
   - 修改 `platforms/windows/ViewModels/MainViewModel.cs`（业务逻辑）
   - 修改 `platforms/windows/MainWindow.xaml`（界面）
   - 修改 `core/src/`（核心功能）

---

**祝使用愉快！** 🚀
