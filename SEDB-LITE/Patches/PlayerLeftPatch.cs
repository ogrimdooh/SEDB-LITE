using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace SEDB_LITE.Patches {
    class PlayerLeftPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public PlayerLeftPatch(Plugin plugin) {
            Plugin = plugin;
        }


    }
}
