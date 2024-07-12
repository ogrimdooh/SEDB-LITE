using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game;
using Sandbox.Engine;
using Sandbox.Engine.Multiplayer;
using HarmonyLib;
using SEDB_LITE;
using Sandbox.Game.Gui;
using VRage.Utils;
using static SEDB_LITE.PatchController;
using VRage.GameServices;
using System.Text.RegularExpressions;

namespace SEDB_LITE.Patches
{

    [PatchingClass]
    class PlayerLeftPatch
    {

        private static Plugin Plugin;

        public PlayerLeftPatch(Plugin plugin)
        {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyDedicatedServerBase), Method = "MyDedicatedServer_ClientLeft")]
        public static void PlayerDisconnected(ulong user, MyChatMemberStateChangeEnum arg2)
        {
            try
            {
                string playerName = Utilities.GetPlayerName(user);
                if (!(playerName.StartsWith("[") && playerName.EndsWith("]") && playerName.Contains("...")) &&
                    (Plugin.m_configuration.NameUnknownUserAsServer && playerName != Plugin.m_configuration.ServerUserName))
                {
                    var msgToUse = Plugin.m_configuration.DisconnectedMessage;
                    msgToUse = msgToUse.Replace("{a}", GetActionTitle(arg2));
                    Task.Run(async () => await Plugin.ProcessStatusMessage(playerName, user, msgToUse));
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(PlayerLeftPatch), e);
            }
        }

        private static string GetActionTitle(MyChatMemberStateChangeEnum arg2)
        {
            switch (arg2)
            {
                case MyChatMemberStateChangeEnum.Disconnected:
                    return Plugin.m_configuration.ServerDisconnectedAction;
                case MyChatMemberStateChangeEnum.Kicked:
                    return Plugin.m_configuration.ServerKickedAction;
                case MyChatMemberStateChangeEnum.Banned:
                    return Plugin.m_configuration.ServerBannedAction;
                case MyChatMemberStateChangeEnum.Entered:
                case MyChatMemberStateChangeEnum.Left:
                default:
                    return Plugin.m_configuration.ServerLeftAction;
            }
        }

    }

}
