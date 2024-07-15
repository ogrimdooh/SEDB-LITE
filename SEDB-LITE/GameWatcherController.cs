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
using System.Diagnostics;
using Sandbox.ModAPI;
using SEDB_LITE.Patches;

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
            if (MySession.Static != null)
            {
                Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to MySession OnReady");
                MySession.Static.OnReady += Static_OnReady;
                Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to MySession OnUnloading");
                MySession.OnUnloading += MySession_OnUnloading;
                if (MySession.Static.Factions != null)
                {
                    Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to FactionCreated");
                    MySession.Static.Factions.FactionCreated += Factions_FactionCreated;
                    Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to FactionStateChanged");
                    MySession.Static.Factions.FactionStateChanged += Factions_FactionStateChanged;
                }
                if (MySession.Static.Gpss != null)
                {
                    Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to GpsAdded");
                    MySession.Static.Gpss.GpsAdded += Gpss_GpsAdded;
                }
            }
        }

        private static void MySession_OnUnloading()
        {
            Plugin.DoDispose();
        }

        private static void Static_OnReady()
        {
            try
            {
                if (Plugin.PluginInstance?.DDBridge != null)
                {
                    Plugin.PluginInstance.DDBridge.SendStatusMessage(default, default, Plugin.PluginInstance.m_configuration.ServerStartedMessage);
                }
                else
                {
                    Logging.Instance.LogWarning(typeof(GameWatcherController), "DDBridge not found when Session Ready!");
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void Gpss_GpsAdded(long playerId, int gps)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                if (!Plugin.PluginInstance.m_configuration.DisplayContainerMessages) return;

                if (MySession.Static?.Gpss != null)
                {
                    var gpsData = MySession.Static.Gpss.GetGps(playerId, gps);
                    if (gpsData != null)
                    {
                        var gpsName = gpsData.Name ?? "";
                        if (gpsData.IsContainerGPS && gpsName.ToLower().Contains("signal"))
                        {

                            bool isStrong = gpsName.Contains("strong");

                            if (Plugin.PluginInstance.m_configuration.DisplayOnlyStrongContainerMessages && !isStrong) return;

                            var msgToUse = isStrong ?
                                Plugin.PluginInstance.m_configuration.StrongContainerMessage : 
                                Plugin.PluginInstance.m_configuration.ContainerMessage;
                            var finalName = string.Join(" ",
                                gpsName
                                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Reverse()
                                .Skip(1)
                                .Reverse()
                                .ToArray()
                            );

                            msgToUse = msgToUse.Replace("{t}", finalName);
                            if (isStrong)
                            {
                                if (gpsData != null && gpsData.Entity != null)
                                {
                                    msgToUse = msgToUse.Replace("{c}", $"{gpsData.Coords.X}:{gpsData.Coords.Y}:{gpsData.Coords.Z}");
                                }
                                else
                                {
                                    msgToUse = msgToUse.Replace("{c}", "Lost Position");
                                }
                            }
                            else
                            {
                                msgToUse = msgToUse.Replace("{c}", "Unknow Position");
                            }
                            if (MySession.Static?.Players != null)
                            {
                                MyPlayer.PlayerId id;
                                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                                {
                                    var player = MySession.Static.Players.GetPlayerById(id);
                                    if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                                    {
                                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        public static void Dispose()
        {
            MyVisualScriptLogicProvider.PlayerDied -= MyPlayer_Die;
            MyVisualScriptLogicProvider.RespawnShipSpawned -= MyEntities_RespawnShipSpawned;
            if (MySession.Static != null)
            {
                MySession.Static.OnReady -= Static_OnReady;
                MySession.OnUnloading -= MySession_OnUnloading;
                if (MySession.Static.Factions != null)
                {
                    MySession.Static.Factions.FactionCreated -= Factions_FactionCreated;
                    MySession.Static.Factions.FactionStateChanged -= Factions_FactionStateChanged;
                }
                if (MySession.Static.Gpss != null)
                {
                    MySession.Static.Gpss.GpsAdded -= Gpss_GpsAdded;
                }
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
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                if (!Plugin.PluginInstance.m_configuration.DisplayFactionMessages) return;

                if (MySession.Static?.Factions == null) return;

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
                        if (Plugin.PluginInstance.m_configuration.IgnoreNpcFactionsInMessages && MySession.Static.Factions.IsNpcFaction(fromFaction.Tag)) return;

                        if (Plugin.PluginInstance.m_configuration.IgnoredFactionTags.Split(';').Contains(fromFaction.Tag)) return;

                        msgToUse = msgToUse.Replace("{f}", $"[{fromFaction.Tag}] {fromFaction.Name}");
                    }
                    var toFaction = MySession.Static.Factions.TryGetFactionById(toFactionId);
                    if (toFaction != null)
                    {
                        if (Plugin.PluginInstance.m_configuration.IgnoreNpcFactionsInMessages && MySession.Static.Factions.IsNpcFaction(toFaction.Tag)) return;

                        if (Plugin.PluginInstance.m_configuration.IgnoredFactionTags.Split(';').Contains(toFaction.Tag)) return;

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

                        if (player == null) return;

                        if (player.IsBot && Plugin.PluginInstance.m_configuration.IgnoreBotInFactionMessages) return;

                        if (!string.IsNullOrWhiteSpace(player.DisplayName))
                        {
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

        private static void Factions_FactionCreated(long factionId)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                if (!Plugin.PluginInstance.m_configuration.DisplayFactionMessages) return;

                var faction = MySession.Static.Factions.TryGetFactionById(factionId);
                if (faction != null)
                {
                    if (Plugin.PluginInstance.m_configuration.IgnoreNpcFactionsInMessages && MySession.Static.Factions.IsNpcFaction(faction.Tag)) return;

                    if (Plugin.PluginInstance.m_configuration.IgnoredFactionTags.Split(';').Contains(faction.Tag)) return;

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
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void MyEntities_RespawnShipSpawned(long shipEntityId, long playerId, string respawnShipPrefabName)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

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

        private static string GetDamageTypeDescription(MyDamageInformationExtensions.DamageType damageType)
        {
            switch (damageType)
            {
                case MyDamageInformationExtensions.DamageType.Creature:
                    return Plugin.PluginInstance.m_configuration.DieCauseCreature;
                case MyDamageInformationExtensions.DamageType.Bullet:
                    return Plugin.PluginInstance.m_configuration.DieCauseBullet;
                case MyDamageInformationExtensions.DamageType.Explosion:
                    return Plugin.PluginInstance.m_configuration.DieCauseExplosion;
                case MyDamageInformationExtensions.DamageType.Radioactivity:
                    return Plugin.PluginInstance.m_configuration.DieCauseRadioactivity;
                case MyDamageInformationExtensions.DamageType.Fire:
                    return Plugin.PluginInstance.m_configuration.DieCauseFire;
                case MyDamageInformationExtensions.DamageType.Toxicity:
                    return Plugin.PluginInstance.m_configuration.DieCauseToxicity;
                case MyDamageInformationExtensions.DamageType.Fall:
                    return Plugin.PluginInstance.m_configuration.DieCauseFall;
                case MyDamageInformationExtensions.DamageType.Tool:
                    return Plugin.PluginInstance.m_configuration.DieCauseTool;
                case MyDamageInformationExtensions.DamageType.Environment:
                    return Plugin.PluginInstance.m_configuration.DieCauseEnvironment;
                case MyDamageInformationExtensions.DamageType.Suicide:
                    return Plugin.PluginInstance.m_configuration.DieCauseSuicide;
                case MyDamageInformationExtensions.DamageType.Asphyxia:
                    return Plugin.PluginInstance.m_configuration.DieCauseAsphyxia;
                case MyDamageInformationExtensions.DamageType.Other:
                    return Plugin.PluginInstance.m_configuration.DieCauseOther;
                case MyDamageInformationExtensions.DamageType.None:
                default:
                    return Plugin.PluginInstance.m_configuration.DieCauseNone;
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
                    if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                    {
                        long attackerPlayerId = 0;
                        MyDamageInformationExtensions.DamageType damageType;
                        MyDamageInformationExtensions.AttackerType attackerType = MyDamageInformationExtensions.AttackerType.None;
                        VRage.ModAPI.IMyEntity attackerEntity = null;
                        var damage = player.Character.StatComp.LastDamage;
                        if (damage.AttackerId != 0)
                            attackerEntity = damage.GetAttacker(out attackerPlayerId, out damageType, out attackerType);
                        else
                            damageType = MyDamageInformationExtensions.GetDamageType(damage.Type);
                        var isAttackerPlayer = MyAPIGateway.Players.TryGetSteamId(attackerPlayerId) != 0;
                        var msgToUse = Plugin.PluginInstance.m_configuration.DieMessage;
                        if (attackerPlayerId != 0 && isAttackerPlayer && attackerPlayerId != playerId)
                        {
                            MyPlayer.PlayerId id2;
                            if (MySession.Static.Players.TryGetPlayerId(attackerPlayerId, out id2))
                            {
                                var player2 = MySession.Static.Players.GetPlayerById(id2);
                                if (player2 != null && !string.IsNullOrWhiteSpace(player2.DisplayName))
                                {
                                    msgToUse = Plugin.PluginInstance.m_configuration.MurderMessage;
                                    msgToUse = msgToUse.Replace("{p2}", player2.DisplayName);
                                }
                            }
                        }
                        msgToUse = msgToUse.Replace("{c}", GetDamageTypeDescription(damageType));
                        msgToUse = msgToUse.Replace("{d}", damage.Amount.ToString("#0.0"));
                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);
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
