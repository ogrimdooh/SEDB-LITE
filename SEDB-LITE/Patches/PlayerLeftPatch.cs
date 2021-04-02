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

namespace SEDB_LITE.Patches {
    [PatchingClass]
    class PlayerLeftPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public PlayerLeftPatch(Plugin plugin) {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerBase), Method = "RaiseClientLeft")]
        public static void PlayerDisconnected(ulong changedUser, MyChatMemberStateChangeEnum stateChange) {
            try {
                string playerName = Utilities.GetPlayerName(changedUser);
                Task.Run(async () => Plugin.ProcessStatusMessage(playerName, changedUser, Plugin.m_configuration.DisconnectedMessage));
            }
            catch (Exception e) {
                Log.WriteLineAndConsole(e.ToString());
            }
        }
    }
}
