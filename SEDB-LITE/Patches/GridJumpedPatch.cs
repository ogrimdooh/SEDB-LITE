using System;
using static SEDB_LITE.PatchController;
using Sandbox.Game.World;
using Sandbox.Game.Entities;
using VRageMath;
using VRage.Network;
using Sandbox.Game.GameSystems;

namespace SEDB_LITE.Patches
{
    [PatchingClass]
    public class GridJumpedPatch
    {

        private static Plugin Plugin;

        public GridJumpedPatch(Plugin plugin)
        {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyCubeGrid), Method = "OnJumpRequested")]
        public static void OnJumpRequested(Vector3D jumpTarget, long userId, float jumpDriveDelay)
        {
            try
            {
                if (Plugin.DEBUG)
                {
                    Logging.Instance.LogInfo(typeof(Plugin), $"OnJumpRequested: userId={userId}!");
                }

                if (!Plugin.m_configuration.Enabled) return;

                if (!Plugin.m_configuration.DisplayGridsJumpMessages) return;

                if (MySession.Static?.Players != null)
                {
                    MyPlayer.PlayerId id;
                    if (MySession.Static.Players.TryGetPlayerId(userId, out id))
                    {
                        var player = MySession.Static.Players.GetPlayerById(id);
                        if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                        {
                            var gridName = Plugin.m_configuration.UnknowJumpGridName;
                            var cockpit = player.Controller?.ControlledEntity?.Entity as MyCockpit;
                            if (cockpit != null)
                            {
                                gridName = cockpit.CubeGrid.DisplayName;
                            }

                            var distance = Vector3D.Distance(player.GetPosition(), jumpTarget) / 1000;
                            if (distance < 0)
                            {
                                distance *= -1;
                            }

                            var msgToUse = Plugin.m_configuration.GridJumpMessage;
                            msgToUse = msgToUse.Replace("{g}", gridName);
                            msgToUse = msgToUse.Replace("{d}", distance.ToString("#0.0"));
                            Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

    }

}
