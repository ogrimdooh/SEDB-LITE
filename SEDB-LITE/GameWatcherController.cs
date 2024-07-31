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
using static SEDB_LITE.Patches.MyDamageInformationExtensions;
using VRage.Game.Entity;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.GameSystems;
using System.Collections.Concurrent;
using static VRage.Dedicated.Configurator.SelectInstanceForm;
using VRageMath;
using Sandbox.Game.Entities.Character;

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
                if (MySession.Static.Players != null)
                {
                    Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to Player Connected/Disconnected");
                    MySession.Static.Players.PlayersChanged += Players_PlayersChanged;
                }
            }
            Logging.Instance.LogInfo(typeof(GameWatcherController), "Do Initial Load Entities");
            DoInitialLoadEntities();
            Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to MyEntities OnEntityAdd");
            MyEntities.OnEntityAdd += Entities_OnEntityAdd;
            Logging.Instance.LogInfo(typeof(GameWatcherController), "Added Watcher to MyEntities OnEntityRemove");
            MyEntities.OnEntityRemove += Entities_OnEntityRemove;
        }

        private static void Players_PlayersChanged(bool connected, MyPlayer.PlayerId id)
        {
            if (connected)
                Players_PlayerConnected(id);
            else
                Players_PlayerDisconnected(id);
        }

        private static bool _inicialLoadComplete = false;
        private static void DoInitialLoadEntities()
        {
            if (!_inicialLoadComplete)
            {
                foreach (var entity in MyEntities.GetEntities())
                {
                    Entities_OnEntityAdd(entity);
                }
                _inicialLoadComplete = true;
            }
        }

        public static ConcurrentDictionary<long, MyPlanet> Planets { get; private set; } = new ConcurrentDictionary<long, MyPlanet>();
        public static ConcurrentDictionary<MyPlayer.PlayerId, MyPlayer> Players { get; private set; } = new ConcurrentDictionary<MyPlayer.PlayerId, MyPlayer>();

        public static MyPlanet GetPlanetAtRange(Vector3D position)
        {
            return Planets.Values.OrderBy(x => Vector3D.Distance(position, x.PositionComp.GetPosition())).FirstOrDefault();
        }

        private static void Players_PlayerConnected(MyPlayer.PlayerId id)
        {
            if (!Players.ContainsKey(id))
            {
                var p = MySession.Static.Players.GetPlayerById(id);
                if (p != null && p.IsValidPlayer())
                {
                    Players[id] = p;
                }
            }
        }

        private static void Players_PlayerDisconnected(MyPlayer.PlayerId id)
        {
            if (Players.ContainsKey(id))
                Players.Remove(id);
        }

        private static void UpdatePlayerList()
        {
            Players.Clear();
            var tempPlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(tempPlayers);

            foreach (var p in tempPlayers)
            {
                if (p.IsValidPlayer())
                {
                    Players[(p as MyPlayer).Id] = p as MyPlayer;
                }
            }
        }

        private static void Entities_OnEntityAdd(MyEntity entity)
        {
            try
            {
                var planet = entity as MyPlanet;
                if (planet != null)
                {
                    lock (Planets)
                    {
                        Planets[planet.EntityId] = planet;
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void CheckOnPlayerGravityMessages(MyPlayer.PlayerId playerId, MyCharacter character)
        {

            if (!Plugin.PluginInstance.m_configuration.DisplayGridsGravityMessages) return;

            if (!Players.ContainsKey(playerId))
                UpdatePlayerList();

            if (!Players.ContainsKey(playerId)) return;

            var player = Players[playerId];

            var didRegisterLocation = SEDBStorage.Instance.GetEntityValue<bool>(playerId.SteamId, SEDBStorage.KEY_DID_REGISTERLOCATION);
            var lastLocationIsGravity = SEDBStorage.Instance.GetEntityValue<bool>(playerId.SteamId, SEDBStorage.KEY_LASTLOCATION_ISGRAVITY);
            var lastLocationEntityId = SEDBStorage.Instance.GetEntityValue<long>(playerId.SteamId, SEDBStorage.KEY_LASTLOCATION_ENTITYID);

            var playerPos = player.GetPosition();

            if (MyGravityProviderSystem.IsPositionInNaturalGravity(playerPos))
            {
                /* Player Enters in Gravity Field */
                
                if (!lastLocationIsGravity)
                {

                    var action = Plugin.PluginInstance.m_configuration.GravityActionEnter;
                    var gridName = "";
                    var planetName = Plugin.PluginInstance.m_configuration.UnknowPlanetNameToUse;

                    var planet = GetPlanetAtRange(playerPos);

                    if (planet != null && lastLocationEntityId != planet.EntityId)
                    {

                        var KEY_VISITED = string.Format(SEDBStorage.KEY_LOCATION_VISITED, planet.EntityId);
                        var didVisitLocation = SEDBStorage.Instance.GetEntityValue<bool>(playerId.SteamId, KEY_VISITED);

                        IMyCubeBlock cockpit = null;
                        if (character != null)
                        {
                            cockpit = character.Parent as IMyCubeBlock;
                        }
                        else
                        {
                            cockpit = player.Controller?.ControlledEntity as IMyCubeBlock;                            
                        }

                        if (cockpit != null)
                        {
                            gridName = cockpit.CubeGrid?.DisplayName;
                            if (string.IsNullOrEmpty(gridName))
                                gridName = Plugin.PluginInstance.m_configuration.UnknowGravityGridName;
                        }

                        if (Plugin.PluginInstance.m_configuration.DisplayEnterGravityMessages || 
                            (!didVisitLocation && Plugin.PluginInstance.m_configuration.DisplayFirstEnterGravityMessages))
                        {

                            if (!didVisitLocation)
                            {
                                action = Plugin.PluginInstance.m_configuration.GravityActionFirstEnter;
                            }

                            planetName = planet.DisplayName;
                            if (string.IsNullOrEmpty(planetName))
                                planetName = planet.Generator?.Id.SubtypeName;
                            if (planetName.Contains("_"))
                            {
                                var nameParts = planetName.Split('_');
                                var namesToUse = nameParts.Where(x => !long.TryParse(x, out _)).ToArray();
                                planetName = string.Join(" ", nameParts);
                            }

                            var msgToUse = string.IsNullOrEmpty(gridName) ?
                                Plugin.PluginInstance.m_configuration.PilotNoGridGravityMessage :
                                Plugin.PluginInstance.m_configuration.GridGravityMessage;
                            msgToUse = msgToUse.Replace("{a}", action);
                            msgToUse = msgToUse.Replace("{g}", gridName);
                            msgToUse = msgToUse.Replace("{t}", planetName);

                            Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, playerId.SteamId, msgToUse);

                        }

                        SEDBStorage.Instance.SetEntityValue(playerId.SteamId, SEDBStorage.KEY_DID_REGISTERLOCATION, true);
                        SEDBStorage.Instance.SetEntityValue(playerId.SteamId, SEDBStorage.KEY_LASTLOCATION_ISGRAVITY, true);
                        SEDBStorage.Instance.SetEntityValue(playerId.SteamId, KEY_VISITED, true);
                        SEDBStorage.Instance.SetEntityValue(playerId.SteamId, SEDBStorage.KEY_LASTLOCATION_ENTITYID, planet.EntityId);
                        SEDBStorage.Save();

                    }

                }

            }
            else
            {

                /* Player Leaves in Gravity Field */
                if (didRegisterLocation && lastLocationIsGravity)
                {

                    var action = Plugin.PluginInstance.m_configuration.GravityActionLeave;
                    var gridName = "";
                    var planetName = Plugin.PluginInstance.m_configuration.UnknowPlanetNameToUse;

                    if (Plugin.PluginInstance.m_configuration.DisplayLeaveGravityMessages)
                    {

                        var cockpit = character.Parent as IMyCubeBlock;
                        if (cockpit != null)
                        {
                            gridName = cockpit.CubeGrid?.DisplayName;
                            if (string.IsNullOrEmpty(gridName))
                                gridName = Plugin.PluginInstance.m_configuration.UnknowGravityGridName;
                        }

                        var planet = GetPlanetAtRange(playerPos);

                        var distanceToPlanet = Math.Abs(Vector3D.Distance(planet.PositionComp.GetPosition(), playerPos)) / 1000;

                        if (planet != null && distanceToPlanet <= Plugin.PluginInstance.m_configuration.MaxDistanceToDetectAPlanet)
                        {
                            planetName = planet.DisplayName;
                            if (string.IsNullOrEmpty(planetName))
                                planetName = planet.Generator?.Id.SubtypeName;
                            if (planetName.Contains("_"))
                            {
                                var nameParts = planetName.Split('_');
                                var namesToUse = nameParts.Where(x => !long.TryParse(x, out _)).ToArray();
                                planetName = string.Join(" ", nameParts);
                            }
                        }

                        var msgToUse = string.IsNullOrEmpty(gridName) ?
                            Plugin.PluginInstance.m_configuration.PilotNoGridGravityMessage :
                            Plugin.PluginInstance.m_configuration.GridGravityMessage;
                        msgToUse = msgToUse.Replace("{a}", action);
                        msgToUse = msgToUse.Replace("{g}", gridName);
                        msgToUse = msgToUse.Replace("{t}", planetName);

                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player.DisplayName, player.Id.SteamId, msgToUse);

                    }

                    SEDBStorage.Instance.SetEntityValue(player.Id.SteamId, SEDBStorage.KEY_DID_REGISTERLOCATION, true);
                    SEDBStorage.Instance.SetEntityValue(player.Id.SteamId, SEDBStorage.KEY_LASTLOCATION_ISGRAVITY, false);
                    SEDBStorage.Instance.SetEntityValue<long>(player.Id.SteamId, SEDBStorage.KEY_LASTLOCATION_ENTITYID, 0);
                    SEDBStorage.Save();

                }

                if (!didRegisterLocation)
                {

                    SEDBStorage.Instance.SetEntityValue(player.Id.SteamId, SEDBStorage.KEY_DID_REGISTERLOCATION, true);
                    SEDBStorage.Instance.SetEntityValue(player.Id.SteamId, SEDBStorage.KEY_LASTLOCATION_ISGRAVITY, false);
                    SEDBStorage.Instance.SetEntityValue<long>(player.Id.SteamId, SEDBStorage.KEY_LASTLOCATION_ENTITYID, 0);
                    SEDBStorage.Save();

                }

            }
        }

        private static void CheckOnPlayerList()
        {
            try
            {
                if (_inicialLoadComplete)
                {

                    if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                    foreach (var playerId in Players.Keys)
                    {
                        CheckOnPlayerGravityMessages(playerId, Players[playerId].Character);
                    }

                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void Entities_OnEntityRemove(MyEntity entity)
        {
            try
            {
                var planet = entity as MyPlanet;
                if (planet != null && Planets.ContainsKey(planet.EntityId))
                {
                    lock (Planets)
                    {
                        Planets.Remove(planet.EntityId);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(GameWatcherController), e);
            }
        }

        private static void MySession_OnUnloading()
        {
            Plugin.DoDispose();
        }

        private static bool canRun;
        private static ParallelTasks.Task task;
        private static void Static_OnReady()
        {
            try
            {
                if (Plugin.PluginInstance?.DDBridge != null)
                {
                    Logging.Instance.LogInfo(typeof(GameWatcherController), "MySession OnReady");
                    Plugin.PluginInstance.DDBridge.SendStatusMessage(default, default, Plugin.PluginInstance.m_configuration.ServerStartedMessage);
                    UpdatePlayerList();
                    canRun = true;
                    task = MyAPIGateway.Parallel.StartBackground(() =>
                    {
                        Logging.Instance.LogInfo(typeof(GameWatcherController), "StartBackground [START]");
                        while (canRun)
                        {
                            CheckOnPlayerList();
                            if (MyAPIGateway.Parallel != null && Plugin.PluginInstance?.m_configuration != null)
                                MyAPIGateway.Parallel.Sleep(Plugin.PluginInstance.m_configuration.PlayerCheckStatusInterval);
                            else
                                break;
                        }
                    });
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
            canRun = false;
            task.Wait();
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
                if (MySession.Static.Players != null)
                {
                    MySession.Static.Players.PlayersChanged -= Players_PlayersChanged;
                }
            }
            MyEntities.OnEntityAdd -= Entities_OnEntityAdd;
            MyEntities.OnEntityRemove -= Entities_OnEntityRemove;
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

                        if (Plugin.PluginInstance.m_configuration.IgnoreBotDieMessages && player.IsBot) return;

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
                                    var didKill = SEDBStorage.Instance.GetEntityValue<bool>(player2.Id.SteamId, SEDBStorage.KEY_DID_KILL);
                                    if (!didKill)
                                    {
                                        Plugin.PluginInstance.DDBridge.SendStatusMessage(player2.DisplayName, player2.Id.SteamId, Plugin.PluginInstance.m_configuration.FirstKillMessage);
                                        SEDBStorage.Instance.SetEntityValue<bool>(player2.Id.SteamId, SEDBStorage.KEY_DID_KILL, true);
                                    }
                                    var killCount = SEDBStorage.Instance.GetEntityValue<int>(player2.Id.SteamId, SEDBStorage.KEY_KILL_COUNT);
                                    SEDBStorage.Instance.SetEntityValue<int>(player2.Id.SteamId, SEDBStorage.KEY_KILL_COUNT, killCount + 1);
                                    SEDBStorage.Save();

                                    msgToUse = Plugin.PluginInstance.m_configuration.MurderMessage;
                                    msgToUse = msgToUse.Replace("{p2}", player2.DisplayName);
                                }
                            }
                        }

                        if (Plugin.DEBUG)
                        {
                            Logging.Instance.LogInfo(typeof(Plugin), $"MyPlayer_Die: playerId={playerId} | AttackerId={damage.AttackerId} | attackerPlayerId={attackerPlayerId} | damage={damage.Type} | damageType={damageType}");
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
