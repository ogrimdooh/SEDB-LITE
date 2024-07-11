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
using Sandbox.Game.World;

namespace SEDB_LITE.Patches
{

    [PatchingClass]
    public class PlayerJoinedPatch
    {

        private static Plugin Plugin;

        public PlayerJoinedPatch(Plugin plugin)
        {
            Plugin = plugin;
        }


        [PrefixMethod]
        [TargetMethod(Type = typeof(MyMultiplayerBase), Method = "RaiseClientJoined")]
        public static void PlayerConnected(ulong changedUser, string userName)
        {
            try
            {
                Task.Run(async () => await Plugin.ProcessStatusMessage(userName.Replace("", ""), changedUser, Plugin.m_configuration.ConnectedMessage));
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(PlayerJoinedPatch), e);
            }
        }

    }

}
