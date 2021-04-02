using Sandbox.Game.Entities.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Utils;

namespace SEDB_LITE.Patches {
    public class PlayerJoinedPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public PlayerJoinedPatch(Plugin plugin) {
            Plugin = plugin;
        }

        public static void PlayerConnected(ulong changedUser, string userName) {
            try {
                Task.Run(async () => Plugin.ProcessStatusMessage(userName, changedUser));
            } catch(Exception e) {
                Log.WriteLineAndConsole(e.ToString());
            }
        }
    }
}
