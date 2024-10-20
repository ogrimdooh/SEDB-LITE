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
using System.Windows.Interop;

namespace SEDB_LITE.Patches
{

    [PatchingClass]
    public class ChatPatch
    {

        private static Plugin Plugin;

        public ChatPatch(Plugin plugin)
        {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerBase), Method = "RaiseChatMessageReceived")]
        public static void ProcessChat(ulong steamUserID, string messageText, ChatChannel channel, long targetId, VRage.GameServices.ChatMessageCustomData customData)
        {
            if (messageText.StartsWith("!SEDB") && Utilities.IsPlayerAdmin(steamUserID))
            {
                var command = new ChatCommand() { Author = steamUserID, Arguments = messageText.Split(' ').Skip(1).ToArray() };
                Task.Run(async () => await Plugin.ProcessCommandAsync(command));
            }
            else
            {
                string playerName = Utilities.GetPlayerName(steamUserID);
                ChatMsg msg = new ChatMsg() { Author = steamUserID, AuthorName = playerName, Text = messageText, Channel = channel, Target = targetId, CustomAuthor = customData.AuthorName };
                Task.Run(async () => await Plugin.ProcessAsync(msg));
            }
        }

    }

}
