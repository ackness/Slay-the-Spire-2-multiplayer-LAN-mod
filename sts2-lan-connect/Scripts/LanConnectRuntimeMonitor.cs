using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Sts2LanConnect.Scripts;

internal sealed partial class LanConnectRuntimeMonitor : Node
{
    private const string MonitorName = "Sts2LanConnectRuntimeMonitor";
    private const double ScanIntervalSeconds = 0.25d;

    private double _timeUntilScan;

    internal static void Install()
    {
        Callable.From(InstallDeferred).CallDeferred();
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _timeUntilScan = 0d;
        Log.Info("sts2_lan_connect runtime monitor ready.");
    }

    public override void _Process(double delta)
    {
        _timeUntilScan -= delta;
        if (_timeUntilScan > 0d)
        {
            return;
        }

        _timeUntilScan = ScanIntervalSeconds;
        ScanTree();
    }

    private static void InstallDeferred()
    {
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            Callable.From(InstallDeferred).CallDeferred();
            return;
        }

        if (tree.Root.GetNodeOrNull<Node>(MonitorName) != null)
        {
            return;
        }

        LanConnectRuntimeMonitor monitor = new()
        {
            Name = MonitorName
        };
        tree.Root.AddChild(monitor);
        Log.Info("sts2_lan_connect runtime monitor installed.");
    }

    private void ScanTree()
    {
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return;
        }

        ScanNode(tree.Root);
    }

    private void ScanNode(Node node)
    {
        if (node is NJoinFriendScreen joinScreen)
        {
            JoinFriendScreenPatches.ScheduleEnsureLanJoinControls(joinScreen, "runtime_monitor");
        }
        else if (node is NMultiplayerSubmenu multiplayerSubmenu)
        {
            MultiplayerSubmenuPatches.ScheduleEnsureLanCreateButton(multiplayerSubmenu, "runtime_monitor");
        }
        else if (node is NMultiplayerHostSubmenu hostSubmenu)
        {
            HostSubmenuPatches.ScheduleEnsureLanHostButton(hostSubmenu, "runtime_monitor");
        }

        foreach (Node child in node.GetChildren())
        {
            ScanNode(child);
        }
    }
}
