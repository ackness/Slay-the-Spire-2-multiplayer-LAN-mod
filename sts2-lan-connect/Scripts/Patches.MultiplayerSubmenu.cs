using System;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2LanConnect.Scripts;

internal static class MultiplayerSubmenuPatches
{
    private const int DuplicateWithoutSignals = 14;
    private const string HookedMetaKey = "sts2_lan_connect_multiplayer_hooks";
    private static readonly FieldInfo? LoadingOverlayField = typeof(NMultiplayerSubmenu).GetField("_loadingOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? StackField = typeof(NSubmenu).GetField("_stack", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static void ScheduleEnsureLanCreateButton(NMultiplayerSubmenu submenu, string source)
    {
        if (!GodotObject.IsInstanceValid(submenu))
        {
            return;
        }

        if (!submenu.HasMeta(HookedMetaKey))
        {
            submenu.SetMeta(HookedMetaKey, true);
            submenu.Connect(Node.SignalName.TreeEntered, Callable.From(() => QueueEnsureLanCreateButton(submenu, "tree_entered")));
            submenu.Connect(Node.SignalName.Ready, Callable.From(() => QueueEnsureLanCreateButton(submenu, "ready")));
            submenu.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(() => QueueEnsureLanCreateButton(submenu, "visibility_changed")));
        }

        Callable.From(() => TryEnsureLanCreateButton(submenu, source)).CallDeferred();
    }

    private static void QueueEnsureLanCreateButton(NMultiplayerSubmenu submenu, string source)
    {
        Callable.From(() => TryEnsureLanCreateButton(submenu, source)).CallDeferred();
    }

    private static void TryEnsureLanCreateButton(NMultiplayerSubmenu submenu, string source)
    {
        if (!GodotObject.IsInstanceValid(submenu) || !submenu.IsInsideTree() || !submenu.IsNodeReady())
        {
            return;
        }

        bool alreadyInstalled = FindLanCreateButton(submenu) != null;
        EnsureLanCreateButton(submenu);
        if (!alreadyInstalled && FindLanCreateButton(submenu) != null)
        {
            NSubmenuButton hostButton = submenu.GetNode<NSubmenuButton>("ButtonContainer/HostButton");
            Node? parent = hostButton.GetParent();
            Log.Info($"sts2_lan_connect injected LAN create button via {source}; hostButton={hostButton.GetPath()}, parentType={parent?.GetType().FullName ?? "<null>"}");
        }
    }

    internal static void EnsureLanCreateButton(NMultiplayerSubmenu submenu)
    {
        try
        {
            NSubmenuButton hostButton = submenu.GetNode<NSubmenuButton>("ButtonContainer/HostButton");
            NSubmenuButton? lanButton = FindLanCreateButton(submenu);
            if (lanButton == null)
            {
                Node parent = hostButton.GetParent();
                lanButton = hostButton.Duplicate(DuplicateWithoutSignals) as NSubmenuButton;
                if (lanButton == null)
                {
                    Log.Error("sts2_lan_connect failed to duplicate HostButton for LAN create button.");
                    return;
                }

                lanButton.Name = LanConnectConstants.MultiplayerLanCreateButtonName;
                ConfigureLanCreateButton(lanButton);
                lanButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnLanCreatePressed(submenu)));
                parent.AddChild(lanButton);
                parent.MoveChild(lanButton, hostButton.GetIndex() + 1);
            }

            // Mirror the official create card visibility when a multiplayer run save exists.
            lanButton.Visible = hostButton.Visible;
            lanButton.SetEnabled(hostButton.IsEnabled);
        }
        catch (Exception ex)
        {
            Log.Error($"sts2_lan_connect failed to inject LAN create button: {ex}");
        }
    }

    private static void OnLanCreatePressed(NMultiplayerSubmenu submenu)
    {
        Control? loadingOverlay = LoadingOverlayField?.GetValue(submenu) as Control;
        NSubmenuStack? stack = StackField?.GetValue(submenu) as NSubmenuStack;
        if (loadingOverlay == null || stack == null)
        {
            Log.Error($"sts2_lan_connect could not resolve top-level host flow dependencies. loadingOverlayNull={loadingOverlay == null} stackNull={stack == null}");
            LanConnectPopupUtil.ShowInfo("未能启动 LAN Host：页面上下文未就绪，请重新打开多人页面后再试。");
            return;
        }

        TaskHelper.RunSafely(LanConnectHostFlow.StartLanHostAsync(GameMode.Standard, loadingOverlay, stack));
    }

    private static void ConfigureLanCreateButton(NSubmenuButton button)
    {
        MegaLabel title = button.GetNode<MegaLabel>("%Title");
        MegaRichTextLabel description = button.GetNode<MegaRichTextLabel>("%Description");
        title.SetTextAutoSize("局域网创建");
        description.Text = "创建一个局域网房间，安装本 MOD 的玩家可通过内网 IP 直连。";
    }

    private static NSubmenuButton? FindLanCreateButton(NMultiplayerSubmenu submenu)
    {
        return submenu.FindChild(LanConnectConstants.MultiplayerLanCreateButtonName, recursive: true, owned: false) as NSubmenuButton;
    }
}
