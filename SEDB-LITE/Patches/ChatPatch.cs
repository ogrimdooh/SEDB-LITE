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
    public class ChatPatch {
        private static Plugin Plugin;
        public static MyLog Log = new MyLog();

        public ChatPatch(Plugin plugin) {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerBase), Method = "RaiseChatMessageReceived")]
        public static void ProcessChat(ulong steamUserID, string messageText, ChatChannel channel, long targetId, string customAuthorName = null) {
            string playerName = Utilities.GetPlayerName(steamUserID);
            ChatMsg msg = new ChatMsg() {Author = steamUserID, AuthorName = playerName, Text = messageText, Channel = channel, Target = targetId, CustomAuthor = customAuthorName };
            Task.Run(async () => Plugin.ProcessAsync(msg));
        }
    }
}
