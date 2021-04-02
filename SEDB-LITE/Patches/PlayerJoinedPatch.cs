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

namespace SEDB_LITE.Patches {
    [PatchingClass]
    public class PlayerJoinedPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public PlayerJoinedPatch(Plugin plugin) {
            Plugin = plugin;
        }


        [PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerBase), Method = "RaiseClientJoined")]
        public static void PlayerConnected(ulong changedUser, string userName) {
            try {
                Task.Run(async () => Plugin.ProcessStatusMessage(userName, changedUser, Plugin.m_configuration.ConnectedMessage));
            } catch(Exception e) {
                Log.WriteLineAndConsole(e.ToString());
            }
        }
    }
}
