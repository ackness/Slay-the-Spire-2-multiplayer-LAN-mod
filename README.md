# STS2 LAN Connect

`STS2 LAN Connect` 是一个《杀戮尖塔 2》多人联机 MOD。

它在不移除原有 Steam 多人入口的前提下，为多人模式补充了一条基于手动 `IPv4 / IP:Port` 的局域网直连路径，方便：

- 同一局域网内联机
- 借助虚拟局域网 / Mesh VPN 做远程“局域网”联机
- 在 Steam 好友入口之外，直接通过 IP 加入房主

当前版本：

- MOD 版本：`0.1.1`
- GitHub Release tag：`v0.1.1`

## 当前功能

- 多人首页新增 `局域网创建`
- 多人首页新增 `局域网继续`
- Join 页面新增 `LAN IP` 输入框和 `Join via IP`
- 复用游戏内置 `ENet` 与 `JoinFlow`
- 默认端口：`33771`
- 支持最近一次连接地址持久化
- 房主地址弹窗支持一键复制 `IP:Port`
- 提供 macOS / Windows 一键安装脚本
- 提供无 MOD 存档到 modded 存档的一键迁移 / 单向同步

## 仓库结构

- `sts2-lan-connect/`
  MOD 工程目录
- `scripts/`
  构建、打包、安装脚本
- `sts2-lan-connect/release/sts2_lan_connect/`
  当前可分发的发布目录
- `LAN_MOD_RESEARCH.md`
  反编译与链路确认记录
- `LAN_MOD_IMPLEMENTATION_PLAN.md`
  实施思路与范围说明
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
  面向玩家的使用说明
- `STS2_LAN_CONNECT_INSTALL_USE_DEBUG_GUIDE_ZH.md`
  安装、使用与调试说明

## 快速开始

### 1. 构建

macOS：

```bash
./scripts/build-sts2-lan-connect.sh
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1
```

如果自动探测不到游戏目录或 Godot，可通过环境变量或命令行参数传入：

- `STS2_ROOT`
- `GODOT_BIN`
- `DOTNET_BIN`

Windows 建议优先使用标准版 Godot 可执行文件来打 `pck`。如果使用 mono/.NET 版 Godot，需要确保同目录下存在完整的 `GodotSharp/Api/Debug`。

### 2. 打包

macOS：

```bash
./scripts/package-sts2-lan-connect.sh
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-sts2-lan-connect-windows.ps1
```

### 3. 一键安装

macOS：

```bash
chmod +x ./scripts/install-sts2-lan-connect-macos.sh
./scripts/install-sts2-lan-connect-macos.sh --package-dir ./sts2-lan-connect/release/sts2_lan_connect
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-sts2-lan-connect-windows.ps1 -PackageDir .\sts2-lan-connect\release\sts2_lan_connect
```

## GitHub Actions

这个项目编译时需要引用游戏安装目录里的：

- `sts2.dll`
- `0Harmony.dll`
- `Steamworks.NET.dll`

因此默认不能依赖 GitHub 托管 runner，仓库里的 workflow 按 `self-hosted` 设计。

建议准备两个 runner：

- `self-hosted`, `windows`, `sts2`
- `self-hosted`, `macOS`, `sts2`

可选的仓库变量：

- `STS2_ROOT_WINDOWS`
- `GODOT_BIN_WINDOWS`
- `DOTNET_BIN_WINDOWS`
- `STS2_ROOT_MACOS`
- `GODOT_BIN_MACOS`
- `DOTNET_BIN_MACOS`

如果这些变量为空，脚本会优先尝试自动探测标准 Steam 安装路径与 PATH 中的 `godot` / `dotnet`。

工作流行为：

- 推送分支或手动触发：构建并上传 `macOS / Windows` zip artifact
- 推送 `v*` tag：构建并自动发布到 GitHub Releases

当前计划发布版本：

- MOD 版本：`0.1.1`
- 对应 Release tag：`v0.1.1`

## 典型联机流程

1. 房主启动游戏并加载同版本 MOD
2. 在多人首页点击 `局域网创建`
3. 把本机局域网地址或虚拟局域网地址发给队友
4. 队友在 `加入` 页面输入 `IP` 或 `IP:33771`
5. 点击 `Join via IP`

同机测试时，成员端可直接填：

```text
127.0.0.1
```

## Release 内容

当前发布目录：

- `sts2-lan-connect/release/sts2_lan_connect/`

当前打包文件：

- `sts2-lan-connect/release/sts2_lan_connect-v0.1.1-macos.zip`
- `sts2-lan-connect/release/sts2_lan_connect-v0.1.1-windows.zip`

发布包内包含：

- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`
- `mod_manifest.json`
- `README.md`
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
- `install-sts2-lan-connect-macos.sh`
- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`

## 注意事项

- 所有联机玩家必须加载相同版本的 MOD，否则会触发官方 `ModMismatch`
- 当前 V1 只支持手动 IPv4 / `IP:Port`，不做自动发现
- 远程“局域网”联机建议搭配 Tailscale / Headscale / ZeroTier / 蒲公英 等虚拟局域网方案

## 版权与说明

- 本项目仅用于学习、研究和 MOD 开发测试
- 《Slay the Spire 2》及相关版权归 Mega Crit 所有
- 本项目与 Mega Crit 无官方关联
