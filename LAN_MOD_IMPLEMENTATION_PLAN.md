# STS2 LAN Connect V1 实施文档

## 目标

- 基于游戏内置 ENet/JoinFlow 流程实现 LAN 联机，而不是自建多人协议。
- 保留 Steam 原生好友加入流程。
- 在多人 Host 页面增加 `Host LAN` 入口。
- 在多人 Join 页面增加 `LAN IP` 输入框与 `Join via IP` 按钮。
- 首版范围只做手动 IPv4/IP:Port 直连，不做局域网自动发现。

## 当前实现基线

- 工程名与产物名已经统一为 `sts2_lan_connect`。
- 本地开发链路已经包含：
  - `.NET 9.0.311`
  - `Godot 4.5.1 .NET`
  - Mac 构建脚本 `./scripts/build-sts2-lan-connect.sh`
- 代码侧已经引入：
  - Host 页面 LAN 按钮注入
  - Join 页面 IP 输入框注入
  - ENet Host 自定义启动分支
  - `config.json` 持久化最近一次输入地址

## 关键设计

### Host 侧

- Host 页面新增 `Host LAN` 按钮。
- 点击后不改官方 UI 栈逻辑，而是通过 Harmony 拦截 `NMultiplayerHostSubmenu.StartHostAsync(...)`。
- 当且仅当本次点击来自 `Host LAN` 时，走 `NetHostGameService.StartENetHost(33771, 4)`。
- Host 成功后继续沿用官方 `CharacterSelect/CustomRun/DailyRun` Push 流程。
- Host 成功后弹出原生风格说明弹窗，显示检测到的主 IPv4 地址和端口。

### Join 侧

- 在 `NJoinFriendScreen` 的好友列表容器下方增加一个 `LAN 直连` 区块。
- 区块包含：
  - `NMegaLineEdit`
  - `Join via IP` 按钮
- 支持输入：
  - `192.168.1.20`
  - `192.168.1.20:33771`
  - `localhost`
- 省略端口时默认使用 `33771`。
- 每次成功触发 Join 时，把原始输入保存到 `config.json` 的 `LastEndpoint`。

### 连接链路

- Join 逻辑直接复用 `NJoinFriendScreen.JoinGameAsync(IClientConnectionInitializer)`。
- 通过 `ENetClientConnectionInitializer(随机netId, ip, port)` 接入现有 `JoinFlow`。
- 版本校验、Mod 校验、Lobby 握手、错误处理全部沿用游戏原生流程。

## 后续开发与验证

### 仍需人工验证

- Windows 真机加载与构建。
- Win -> Win、Mac -> Mac、Win -> Mac、Mac -> Win 四组真实联机。
- Host LAN 按钮在实际 UI 中的布局是否需要微调。

### 明确不做

- 自动扫描局域网房间
- IPv6
- NAT 穿透 / 公网联机
- 与其他多人 MOD 的兼容承诺
