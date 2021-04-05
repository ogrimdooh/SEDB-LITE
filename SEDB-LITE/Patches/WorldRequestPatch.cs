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
using Sandbox;
using VRage.Network;
using System.Reflection;

namespace SEDB_LITE.Patches {
    [PatchingClass]
    public class WorldRequestPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();
        public static Bridge bridge;

        public WorldRequestPatch(Plugin plugin) {
            Plugin = plugin;
        }


        //[PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerServerBase), Method = "OnWorldRequest")]
        public static bool PatchGetWorld(EndpointId sender) {
            bridge = new Bridge(Plugin);

            Log.Info($"Patched World request received: {MyMultiplayer.Static.GetMemberName(sender.Value)}");

            if (!bridge.Ready) {
                var _raiseClientLeft = typeof(MyMultiplayerServerBase).GetMethod("RaiseClientLeft", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                _raiseClientLeft.Invoke(null, new object[] { MyMultiplayer.Static, sender.Value, MyChatMemberStateChangeEnum.Disconnected });
                return false;
            }
            return true;
        }
    }
}
