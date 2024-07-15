using System;
using static SEDB_LITE.PatchController;
using Sandbox.Game.World;
using Sandbox.Game.Entities;
using VRage.Network;
using System.Diagnostics;
using System.Linq;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;

namespace SEDB_LITE.Patches
{

    [PatchingClass]
    public class ContainerOpenedPatch
    {

        private static Plugin Plugin;

        public ContainerOpenedPatch(Plugin plugin)
        {
            Plugin = plugin;
        }

        [PrefixMethod]
        [TargetMethod(Type = typeof(MyCubeGrid), Method = "ContainerOpened")]
        public static void ContainerOpened(long entityId)
        {
            try
            {
                if (Plugin.DEBUG)
                {
                    Logging.Instance.LogInfo(typeof(Plugin), $"ContainerOpened: entityId={entityId}!");
                }

                if (!Plugin.m_configuration.DisplayContainerMessages) return;

                if (Plugin.m_configuration.DisplayOnlyStrongContainerMessages) return;

                if (MySession.Static?.Players != null && MySession.Static?.Gpss != null)
                {
                    var gps = MySession.Static.Gpss.GetGpssByEntityId(entityId).FirstOrDefault();
                    if (gps != null)
                    {
                        var maxDistance = 10;
                        var players = new List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(players, (x) =>
                            x.Character != null &&
                            Vector3D.Distance(x.Character.GetPosition(), gps.Coords) <= maxDistance
                        );
                        if (players.Any())
                        {
                            var targetPlayer = players.OrderByDescending(x => Vector3D.Distance(x.Character.GetPosition(), gps.Coords)).FirstOrDefault();
                            if (targetPlayer != null && !string.IsNullOrWhiteSpace(targetPlayer.DisplayName))
                            {
                                var msgToUse = Plugin.PluginInstance.m_configuration.GetedContainerMessage;
                                var finalName = string.Join(" ",
                                    gps.Name
                                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Reverse()
                                    .Skip(1)
                                    .Reverse()
                                    .ToArray()
                                );
                                msgToUse = msgToUse.Replace("{t}", finalName);
                                Plugin.PluginInstance.DDBridge.SendStatusMessage(targetPlayer.DisplayName, targetPlayer.SteamUserId, msgToUse);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(PlayerJoinedPatch), e);
            }
        }

    }

}
