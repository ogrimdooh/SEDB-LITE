using System;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game;
using Sandbox.Game.World;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
using VRage.Game.ObjectBuilders.Components;
using Sandbox.Game.SessionComponents;

namespace SEDB_LITE
{
    public static class GameWatcherController
    {

        public static void Init()
        {
            Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to PlayerDied");
            MyVisualScriptLogicProvider.PlayerDied += MyPlayer_Die;
            Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to RespawnShipSpawned");
            MyVisualScriptLogicProvider.RespawnShipSpawned += MyEntities_RespawnShipSpawned;
            if (MySession.Static != null && MySession.Static.Factions != null)
            {
                Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to FactionCreated");
                MySession.Static.Factions.FactionCreated += Factions_FactionCreated;
                Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to FactionStateChanged");
                MySession.Static.Factions.FactionStateChanged += Factions_FactionStateChanged;
                Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to GpsAdded");
                MySession.Static.Gpss.GpsAdded += Gpss_GpsAdded;
            }
        }

        private static void Gpss_GpsAdded(long playerId, int gps)
        {
            if (!Plugin.PluginInstance.m_configuration.Enabled) return;

            if (!Plugin.PluginInstance.m_configuration.DisplayContainerMessages) return;

            var gpsData = MySession.Static.Gpss.GetGps(playerId, gps);
            var gpsName = gpsData.Name.ToLower();
            if (gpsData.IsContainerGPS && gpsName.Contains("unknown"))
            {

                if (Plugin.PluginInstance.m_configuration.DisplayOnlyStrongContainerMessages && !gpsName.Contains("strong")) return;                

                var msgToUse = Plugin.PluginInstance.m_configuration.ContainerMessage;
                var finalName = string.Join(" ",
                    gpsData.Name
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Reverse()
                    .Skip(1)
                    .Reverse()
                    .ToArray()
                );

                msgToUse = msgToUse.Replace("{t}", finalName);
                msgToUse = msgToUse.Replace("{c}", $"{gpsData.Coords.X}:{gpsData.Coords.Y}:{gpsData.Coords.Z}");
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    var player = MySession.Static.Players.GetPlayerById(id);
                    if (!string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
                    }
                }
            }
        }

        public static void Dispose()
        {
            MyVisualScriptLogicProvider.PlayerDied -= MyPlayer_Die;
            MyVisualScriptLogicProvider.RespawnShipSpawned -= MyEntities_RespawnShipSpawned;
            if (MySession.Static != null && MySession.Static.Factions != null)
            {
                MySession.Static.Factions.FactionCreated -= Factions_FactionCreated;
                MySession.Static.Factions.FactionStateChanged -= Factions_FactionStateChanged;
            }
        }

        private static bool IsFactionChangeValidToMsg(MyFactionStateChange action, out int msgType)
        {
            msgType = 0;
            switch (action)
            {
                case MyFactionStateChange.SendPeaceRequest:
                case MyFactionStateChange.AcceptPeace:
                case MyFactionStateChange.DeclareWar:
                case MyFactionStateChange.SendFriendRequest:
                case MyFactionStateChange.AcceptFriendRequest:
                    return true;
                case MyFactionStateChange.FactionMemberSendJoin:
                case MyFactionStateChange.FactionMemberLeave:
                    msgType = 1;
                    return true;
                case MyFactionStateChange.FactionMemberAcceptJoin:
                case MyFactionStateChange.FactionMemberKick:
                case MyFactionStateChange.FactionMemberPromote:
                case MyFactionStateChange.FactionMemberDemote:
                    msgType = 2;
                    return true;
                case MyFactionStateChange.RemoveFaction:
                    msgType = 3;
                    return true;
                case MyFactionStateChange.CancelPeaceRequest:
                case MyFactionStateChange.CancelFriendRequest:
                case MyFactionStateChange.FactionMemberCancelJoin:
                case MyFactionStateChange.FactionMemberNotPossibleJoin:
                default:
                    return false;
            }
        }

        private static string GetActionTitle(MyFactionStateChange action)
        {
            switch (action)
            {
                case MyFactionStateChange.RemoveFaction:
                    return Plugin.PluginInstance.m_configuration.FactionActionRemoveFaction;
                case MyFactionStateChange.SendPeaceRequest:
                    return Plugin.PluginInstance.m_configuration.FactionActionSendPeaceRequest;
                case MyFactionStateChange.CancelPeaceRequest:
                    return Plugin.PluginInstance.m_configuration.FactionActionCancelPeaceRequest;
                case MyFactionStateChange.AcceptPeace:
                    return Plugin.PluginInstance.m_configuration.FactionActionAcceptPeace;
                case MyFactionStateChange.DeclareWar:
                    return Plugin.PluginInstance.m_configuration.FactionActionDeclareWar;
                case MyFactionStateChange.SendFriendRequest:
                    return Plugin.PluginInstance.m_configuration.FactionActionSendFriendRequest;
                case MyFactionStateChange.CancelFriendRequest:
                    return Plugin.PluginInstance.m_configuration.FactionActionCancelFriendRequest;
                case MyFactionStateChange.AcceptFriendRequest:
                    return Plugin.PluginInstance.m_configuration.FactionActionAcceptFriendRequest;
                case MyFactionStateChange.FactionMemberSendJoin:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberSendJoin;
                case MyFactionStateChange.FactionMemberCancelJoin:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberCancelJoin;
                case MyFactionStateChange.FactionMemberAcceptJoin:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberAcceptJoin;
                case MyFactionStateChange.FactionMemberKick:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberKick;
                case MyFactionStateChange.FactionMemberPromote:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberPromote;
                case MyFactionStateChange.FactionMemberDemote:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberDemote;
                case MyFactionStateChange.FactionMemberLeave:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberLeave;
                case MyFactionStateChange.FactionMemberNotPossibleJoin:
                    return Plugin.PluginInstance.m_configuration.FactionActionFactionMemberNotPossibleJoin;
                default:
                    return "";
            }
        }

        private static void Factions_FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (!Plugin.PluginInstance.m_configuration.Enabled) return;

            if (!Plugin.PluginInstance.m_configuration.DisplayFactionMessages) return;

            int msgType = 0;
            if (IsFactionChangeValidToMsg(action, out msgType))
            {
                var msgToUse = "";
                switch (msgType)
                {
                    case 1:
                        msgToUse = Plugin.PluginInstance.m_configuration.FactionMemberActionFactionMessage;
                        break;
                    case 2:
                        msgToUse = Plugin.PluginInstance.m_configuration.FactionMemberActionMemberMessage;
                        break;
                    case 3:
                        msgToUse = Plugin.PluginInstance.m_configuration.FactionRemovedMessage;
                        break;
                    default:
                        msgToUse = Plugin.PluginInstance.m_configuration.FactionActionMessage;
                        break;
                }
                var actionTitle = GetActionTitle(action);
                msgToUse = msgToUse.Replace("{a}", actionTitle);
                var fromFaction = MySession.Static.Factions.TryGetFactionById(fromFactionId);
                if (fromFaction != null)
                {
                    msgToUse = msgToUse.Replace("{f}", $"[{fromFaction.Tag}] {fromFaction.Name}");
                }
                var toFaction = MySession.Static.Factions.TryGetFactionById(toFactionId);
                if (toFaction != null)
                {
                    msgToUse = msgToUse.Replace("{f2}", $"[{toFaction.Tag}] {toFaction.Name}");
                }
                if (senderId != 0)
                {
                    var senderName = Utilities.GetPlayerName(senderId);
                    if (!string.IsNullOrWhiteSpace(senderName))
                    {
                        msgToUse = msgToUse.Replace("{p2}", senderName);
                    }
                }
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    var player = MySession.Static.Players.GetPlayerById(id);

                    if (player.IsBot && Plugin.PluginInstance.m_configuration.IgnoreBotInFactionMessages) return;

                    if (!string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
                    }
                }
            }
        }

        private static void Factions_FactionCreated(long factionId)
        {
            if (!Plugin.PluginInstance.m_configuration.Enabled) return;

            if (!Plugin.PluginInstance.m_configuration.DisplayFactionMessages) return;

            var faction = MySession.Static.Factions.TryGetFactionById(factionId);
            if (faction != null)
            {
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(faction.FounderId, out id))
                {
                    var player = MySession.Static.Players.GetPlayerById(id);

                    if (player.IsBot && Plugin.PluginInstance.m_configuration.IgnoreBotInFactionMessages) return;

                    if (!string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        var msgToUse = Plugin.PluginInstance.m_configuration.FactionCretedMessage.Replace("{f}", $"[{faction.Tag}] {faction.Name}");
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
                    }
                }
            }
        }

        private static void MyEntities_RespawnShipSpawned(long shipEntityId, long playerId, string respawnShipPrefabName)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                if (!MySession.Static.Ready) return; /* Avoid loading messages */

                if (!Plugin.PluginInstance.m_configuration.DisplayRespawnMessages) return;

                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    var player = MySession.Static.Players.GetPlayerById(id);
                    if (!string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, Plugin.PluginInstance.m_configuration.RespawnMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void MyPlayer_Die(long playerId)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                if (!Plugin.PluginInstance.m_configuration.DisplayDieMessages) return;

                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    var player = MySession.Static.Players.GetPlayerById(id);
                    if (!string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, Plugin.PluginInstance.m_configuration.DieMessage);
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
