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

namespace SEDB_LITE.Patches {
    public class ChatPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public ChatPatch(Plugin plugin) {
            Plugin = plugin;
        }

        public static void ProcessChat(ulong steamUserID, string messageText, ChatChannel channel, long targetId, string customAuthorName = null) {
            string playerName = Utilities.GetPlayerName(steamUserID);
            ChatMsg msg = new ChatMsg() {Author = steamUserID, AuthorName = playerName, Text = messageText, Channel = channel, Target = targetId, CustomAuthor = customAuthorName };
            Log.WriteLineAndConsole($"{msg.AuthorName}: {msg.Text}");
                
            Task.Run(async () => Plugin.ProcessAsync(msg));
        }
    }
}
