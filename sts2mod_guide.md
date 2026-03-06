# Slay the Spire 2 Mod制作指南

  

本指南将帮助你为《杀戮尖塔2》制作Mod。本游戏使用C# + Godot 4.5开发，Mod系统基于Harmony补丁和Hook扩展点。

  

---

  

## 1. Mod基础架构

  

### 1.1 Mod加载机制

  

```

mods/                          # 本地Mod目录

├── my_mod.pck                 # 打包的Mod文件

│   ├── mod_manifest.json      # Mod清单(必需)

│   ├── my_mod.dll            # C#代码(可选)

│   └── localization/          # 本地化文件(可选)

│       └── eng/

│           └── cards.json

```

  

### 1.2 支持的Mod来源

  

| 来源 | 位置 | 说明 |

|-----|------|------|

| 本地Mod | `exe所在目录/mods/` | .pck文件 |

| Steam创意工坊 | Steam Workshop | 自动下载 |

  

### 1.3 Mod清单 (mod_manifest.json)

  

每个Mod必须包含 `mod_manifest.json`:

  

```json

{

    "pck_name": "my_awesome_mod",

    "name": "My Awesome Mod",

    "author": "YourName",

    "description": "Adds cool new features!",

    "version": "1.0.0"

}

```

  

---

  

## 2. 创建你的第一个Mod

  

### 2.1 项目结构

  

创建一个C#类库项目，引用以下程序集：

- `MegaCrit.Sts2.Core.dll` (游戏核心)

- `GodotSharp.dll` (Godot引擎)

- `0Harmony.dll` (Harmony补丁库)

  

### 2.2 基础Mod类

  

```csharp

using System;

using MegaCrit.Sts2.Core.Modding;

using HarmonyLib;

  

[ModInitializer("Initialize")]

public class MyMod

{

    public static void Initialize()

    {

        // 在这里初始化你的Mod

        Harmony harmony = new Harmony("com.yourname.awesomemod");

        harmony.PatchAll();

    }

}

```

  

### 2.3 使用ModInitializerAttribute

  

```csharp

[ModInitializer("OnModLoad")]

public static class ModEntry

{

    public static void OnModLoad()

    {

        // 初始化代码

    }

}

```

  

---

  

## 3. Hook扩展系统

  

游戏提供100+个Hook点，允许你在特定时机插入代码。

  

### 3.1 Hook分类

  

| 分类 | 数量 | 示例 |

|------|------|------|

| 战斗Hook | 40+ | BeforeAttack, AfterCardPlayed |

| 卡牌Hook | 30+ | AfterCardDrawn, BeforeCardPlayed |

| 遗物Hook | 20+ | AfterRelicObtained, OnBattleEnd |

| 地图Hook | 10+ | AfterActEntered, BeforeMapNodeChosen |

  

### 3.2 使用Hook

  

在AbstractModel中重写hook方法：

  

```csharp

using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Combat;

using MegaCrit.Sts2.Core.Commands.Builders;

using MegaCrit.Sts2.Core.Entities.Cards;

using System.Threading.Tasks;

  

public class MyPower : PowerModel

{

    public override bool ShouldReceiveCombatHooks => true;

  

    // 在攻击前触发

    public override Task BeforeAttack(AttackCommand command)

    {

        // 增加50%伤害

        command.Damage *= 1.5m;

        return Task.CompletedTask;

    }

  

    // 在卡牌打出后触发

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)

    {

        // 你的逻辑

        return Task.CompletedTask;

    }

}

```

  

### 3.3 完整Hook列表

  

#### 战斗相关

```csharp

// 攻击

Task BeforeAttack(AttackCommand command)

Task AfterAttack(AttackCommand command)

  

// 护甲

Task BeforeBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)

Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)

Task AfterBlockBroken(CombatState combatState, Creature creature)

Task AfterBlockCleared(CombatState combatState, Creature creature)

  

// 死亡

Task BeforeDeath(IRunState runState, CombatState? combatState, Creature creature)

Task AfterDeath(IRunState runState, CombatState? combatState, Creature creature, bool wasRemovalPrevented, float deathAnimLength)

```

  

#### 卡牌相关

```csharp

// 打牌

Task BeforeCardPlayed(CardPlay cardPlay)

Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)

Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)

  

// 抽牌

Task AfterCardDrawnEarly(PlayerChoiceContext context, CardModel card, bool fromHandDraw)

Task AfterCardDrawn(PlayerChoiceContext context, CardModel card, bool fromHandDraw)

  

// 弃牌

Task AfterCardDiscarded(PlayerChoiceContext context, CardModel card)

  

// 消耗

Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)

  

// 堆叠变动

Task AfterCardChangedPiles(IRunState runState, CombatState? combatState, CardModel card, PileType oldPile, AbstractModel? source)

```

  

#### 遗物相关

```csharp

Task AfterRelicObtained(IRunState runState, RelicModel relic)

Task AfterRelicLost(IRunState runState, RelicModel relic)

Task OnBattleEnd(IRunState runState, CombatState combatState)

Task OnBattleStart(CombatState combatState)

Task OnScry(IRunState runState, int amount)

```

  

#### 回合相关

```csharp

Task OnPlayerTurnStart(CombatState combatState)

Task OnPlayerTurnEnd(CombatState combatState)

Task OnEnemyTurnStart(CombatState combatState)

Task OnEnemyTurnEnd(CombatState combatState)

```

  

---

  

## 4. 添加自定义内容

  

### 4.1 添加卡牌到卡池

  

```csharp

using MegaCrit.Sts2.Core.Modding;

using MegaCrit.Sts2.Core.Models;

  

public class MyMod

{

    public static void Initialize()

    {

        // 添加自定义卡牌到卡池

        ModHelper.AddModelToPool<CardPoolModel, MyCustomCardModel>();

    }

}

```

  

### 4.2 添加遗物到遗物池

  

```csharp

ModHelper.AddModelToPool<RelicPoolModel, MyCustomRelicModel>();

```

  

### 4.3 添加怪物到遭遇池

  

```csharp

ModHelper.AddModelToPool<EncounterModel, MyCustomEncounterModel>();

```

  

---

  

## 5. 本地化支持

  

### 5.1 添加Mod本地化文件

  

在.pck中包含以下结构：

  

```

localization/

└── eng/

    ├── cards.json

    ├── relics.json

    └── powers.json

```

  

### 5.2 本地化格式

  

```json

{

    "my_custom_card_NAME": "My Custom Card",

    "my_custom_card_DESCRIPTION": "Deal {0} damage. ",

    "my_custom_card_EXTENDED_DESCRIPTION": "Deal {0} damage. Extra info."

}

```

  

### 5.3 使用本地化

  

```csharp

// 在模型中使用LocString

public class MyCard : CardModel

{

    public LocString Name = new LocString("my_custom_card", "MY_CUSTOM_CARD");

    public LocString Description = new LocString("my_custom_card", "MY_CUSTOM_CARD_DESCRIPTION");

}

```

  

---

  

## 6. Harmony补丁

  

对于Hook无法覆盖的场景，可以使用Harmony直接修改游戏代码。

  

### 6.1 前置/后置补丁

  

```csharp

using HarmonyLib;

using MegaCrit.Sts2.Core.Combat;

  

[HarmonyPatch]

public class MyPatches

{

    [HarmonyPatch(typeof(CombatState), nameof(CombatState.StartCombat))]

    [HarmonyPrefix]

    public static void StartCombat_Prefix(CombatState __instance)

    {

        // 在战斗开始前执行的代码

    }

  

    [HarmonyPatch(typeof(CombatState), nameof(CombatState.StartCombat))]

    [HarmonyPostfix]

    public static void StartCombat_Postfix(CombatState __instance)

    {

        // 在战斗开始后执行的代码

    }

}

```

  

### 6.2 修改方法返回值

  

```csharp

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDamage))]

[HarmonyPrefix]

public static bool ModifyDamage(CardModel __instance, ref int __result)

{

    // 将伤害翻倍

    __result *= 2;

    return false; // 跳过原方法

}

```

  

### 6.3 注入代码

  

```csharp

[HarmonyPatch(typeof(SomeClass), "SomeMethod")]

[HarmonyTranspiler]

public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)

{

    // 修改IL代码

}

```

  

---

  

## 7. 完整示例：创建一张新卡牌

  

### 7.1 定义卡牌模型

  

```csharp

using System;

using System.Collections.Generic;

using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Entities.Cards;

using MegaCrit.Sts2.Core.Localization;

using MegaCrit.Sts2.Core.ValueProps;

  

[Serializable]

public class DoubleStrikeCard : CardModel

{

    public DoubleStrikeCard()

    {

        Id = ModelId.FromString("double_strike");

        Type = CardType.Attack;

        Damage = 6;

        BaseDamage = 6;

        Cost = 1;

        UpgradedCost = 0;

        Rarity = CardRarity.Common;

        DamageType = DamageType.Normal;

        Name = new LocString("double_strike", "DOUBLE_STRIKE");

        Description = new LocString("double_strike", "DOUBLE_STRIKE_DESCRIPTION");

        ExtendedDescription = new LocString("double_strike", "DOUBLE_STRIKE_EXTENDED_DESCRIPTION");

    }

  

    public override bool ShouldReceiveCombatHooks => true;

  

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)

    {

        // 造成两次伤害

        var target = cardPlay.Target;

        if (target != null)

        {

            for (int i = 0; i < 2; i++)

            {

                AttackCommand command = new AttackCommand

                {

                    Target = target,

                    Damage = Damage,

                    Source = cardPlay.Card,

                    DamageType = DamageType

                };

                // 执行攻击...

            }

        }

        return Task.CompletedTask;

    }

}

```

  

### 7.2 注册卡牌

  

```csharp

using MegaCrit.Sts2.Core.Modding;

  

[ModInitializer("Initialize")]

public class DoubleStrikeMod

{

    public static void Initialize()

    {

        // 添加到普通卡池

        ModHelper.AddModelToPool<CardPoolModel, DoubleStrikeCard>();

    }

}

```

  

### 7.3 本地化文件 (localization/eng/cards.json)

  

```json

{

    "DOUBLE_STRIKE": "Double Strike",

    "DOUBLE_STRIKE_DESCRIPTION": "Deal 6 damage twice.",

    "DOUBLE_STRIKE_EXTENDED_DESCRIPTION": "Deal 6 damage twice."

}

```

  

---

  

## 8. 调试与测试

  

### 8.1 调试日志

  

```csharp

using MegaCrit.Sts2.Core.Logging;

  

Log.Info("My mod is loaded!");

Log.Warn("Something unexpected happened");

Log.Error("Something went wrong");

```

  

### 8.2 启用Mod加载

  

首次运行游戏后，在设置中同意Mod加载协议。

  

### 8.3 常见错误

  

| 错误 | 解决方案 |

|------|----------|

| `Mod manifest not found` | 确保mod_manifest.json在.pck根目录 |

| `Assembly load failed` | 检查C#程序集引用是否正确 |

| `Hook not firing` | 确保模型注册到了正确的池 |

  

---

  

## 9. 打包发布

  

### 9.1 打包步骤

  

1. 编译你的C#项目为DLL

2. 创建一个.pck包，包含：

   - `mod_manifest.json`

   - 你的DLL

   - 本地化文件（如果有）

3. 将.pck文件放入 `mods/` 目录

  

### 9.2 Steam创意工坊

  

游戏支持Steam创意工坊分发：

- 打包.pck文件

- 上传到Steam创意工坊

- 玩家订阅后自动下载

  

---

  

## 10. API参考

  

### 10.1 核心类

  

| 类 | 用途 |

|---|------|

| `AbstractModel` | 所有游戏实体基类 |

| `CardModel` | 卡牌数据 |

| `RelicModel` | 遗物数据 |

| `PowerModel` | 能力/状态数据 |

| `MonsterModel` | 怪物数据 |

| `CombatState` | 战斗状态 |

| `IRunState` | Run状态接口 |

  

### 10.2 关键接口

  

| 接口 | 用途 |

|------|------|

| `IPoolModel` | 可加入卡池的模型 |

| `ITemporaryPower` | 临时能力 |

  

### 10.3 工具类

  

| 类 | 用途 |

|---|------|

| `ModHelper` | Mod辅助API |

| `ModelDb` | 模型数据库 |

| `Hook` | 静态钩子触发器 |

| `LocManager` | 本地化管理 |

| `Rng` | 随机数生成 |

  

---

  

## 11. 进阶话题

  

### 11.1 多人游戏支持

  

使用 `Net*` 开头的类处理网络同步：

- `NetPlayCardAction`

- `NetEndPlayerTurnAction`

  

### 11.2 自动游戏(Auto-Slay)支持

  

实现 `IAutoPlayable` 接口支持自动战斗。

  

### 11.3 自定义事件

  

使用 `EventModel` 创建自定义事件。

  

---

  

## 12. 常见问题

  

**Q: 如何添加新角色?**

A: 继承相关模型类并注册到游戏系统。

  

**Q: 如何修改现有卡牌?**

A: 使用Harmony的Postfix修改属性值。

  

**Q: Mod会破坏存档吗?**

A: 建议使用Mod时创建新存档，游戏不会阻止但可能产生兼容性问题。

  

**Q: 如何卸载Mod?**

A: 从mods目录删除.pck文件，或在游戏设置中禁用。

  

---

  

## 附录：Hook完整列表

  

完整Hook列表请参考 `src/Core/Hooks/Hook.cs`，该文件包含100+个静态方法，每个对应一个游戏时机。

  

---

  

## 相关文档

  

- [framework.md](./framework.md) - 项目架构详解

- [Hook.cs源文件](./src/Core/Hooks/Hook.cs) - Hook API完整定义

- [ModManager.cs源文件](./src/Core/Modding/ModManager.cs) - Mod加载器实现