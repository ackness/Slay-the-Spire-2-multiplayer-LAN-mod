using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2LanConnect.Scripts;

internal static class LanConnectHostFlow
{
    private static bool _useLanHostOnce;

    public static void QueueLanHost()
    {
        _useLanHostOnce = true;
    }

    public static bool ConsumeQueuedLanHost()
    {
        if (!_useLanHostOnce)
        {
            return false;
        }

        _useLanHostOnce = false;
        return true;
    }

    public static async Task StartLanHostAsync(GameMode gameMode, Control loadingOverlay, NSubmenuStack stack)
    {
        loadingOverlay.Visible = true;

        try
        {
            NetHostGameService netService = new();
            NetErrorInfo? error = netService.StartENetHost(LanConnectConstants.DefaultPort, LanConnectConstants.DefaultMaxPlayers);
            if (error.HasValue)
            {
                NErrorPopup? popup = NErrorPopup.Create(error.Value);
                if (popup != null)
                {
                    NModalContainer.Instance?.Add(popup);
                }

                return;
            }

            switch (gameMode)
            {
                case GameMode.Standard:
                {
                    NCharacterSelectScreen submenu = stack.GetSubmenuType<NCharacterSelectScreen>();
                    submenu.InitializeMultiplayerAsHost(netService, LanConnectConstants.DefaultMaxPlayers);
                    stack.Push(submenu);
                    break;
                }
                case GameMode.Daily:
                {
                    NDailyRunScreen submenu = stack.GetSubmenuType<NDailyRunScreen>();
                    submenu.InitializeMultiplayerAsHost(netService);
                    stack.Push(submenu);
                    break;
                }
                default:
                {
                    NCustomRunScreen submenu = stack.GetSubmenuType<NCustomRunScreen>();
                    submenu.InitializeMultiplayerAsHost(netService, LanConnectConstants.DefaultMaxPlayers);
                    stack.Push(submenu);
                    break;
                }
            }

            await Task.Yield();
            string ip = LanConnectNetUtil.GetPrimaryLanAddress();
            LanConnectPopupUtil.ShowInfo($"LAN 主机已启动。\n把这个地址发给好友：{ip}:{LanConnectConstants.DefaultPort}");
        }
        catch
        {
            NErrorPopup? popup = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
            if (popup != null)
            {
                NModalContainer.Instance?.Add(popup);
            }

            throw;
        }
        finally
        {
            loadingOverlay.Visible = false;
        }
    }
}
