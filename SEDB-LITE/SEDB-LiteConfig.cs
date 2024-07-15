using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sandbox.Game.Screens.Helpers;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Plugins;

namespace SEDB_LITE
{

    public class SEDB_LiteConfig : IPluginConfiguration
    {

        /* General */

        [Display(Name = "Enabled", Order = 1)]
        [Category("General")]
        [Description("Enable/Disable the Plugin...")]
        public bool Enabled = false;

        [Display(Name = "Bot Token", Order = 2)]
        [Category("General")]
        public string Token = "";

        [Display(Name = "Chat Channel ID", Order = 3)]
        [Category("General")]
        public string ChannelID = "";

        [Display(Name = "Chat format ingame->Discord", Order = 4)]
        [Category("General")]
        public string GameChatFormat = ":rocket: **{p}**: {msg}";

        [Display(Name = "Chat format Discord->ingame", Order = 5)]
        [Category("General")]
        public string DiscordChatAuthorFormat = "[D]{p} {msg}";

        [Display(Name = "ChatColor", Order = 6)]
        [Category("General")]
        public string GlobalColor = "White";

        [Display(Name = "Display SteamID for each player", Order = 7)]
        [Category("General")]
        public bool DisplaySteamID = false;

        [Display(Name = "Debug", Order = 8)]
        [Category("General")]
        public bool DebugMode = false;

        /* Server */

        [Display(Name = "Msg when user connects", Order = 1)]
        [Category("Server")]
        public string ConnectedMessage = ":sunny: {p} joined the server";

        [Display(Name = "Msg when server starts", Order = 2)]
        [Category("Server")]
        public string ServerStartedMessage = ":white_check_mark: Server Started!";

        [Display(Name = "Msg when the server stops", Order = 3)]
        [Category("Server")]
        public string ServerStoppedMessage = ":x: Server Stopped!";

        [Display(Name = "Msg when user disconnects", Order = 4)]
        [Category("Server")]
        public string DisconnectedMessage = ":new_moon: {p} {a} the server";

        [Display(Name = "Action name when player left", Order = 5)]
        [Category("Server")]
        public string ServerLeftAction = "left";

        [Display(Name = "Action name when player disconnected", Order = 6)]
        [Category("Server")]
        public string ServerDisconnectedAction = "disconnect from";

        [Display(Name = "Action name when player kicked", Order = 7)]
        [Category("Server")]
        public string ServerKickedAction = "was kicked from";

        [Display(Name = "Action name when player banned", Order = 8)]
        [Category("Server")]
        public string ServerBannedAction = "was banned from";

        /* Unknow Signals */

        [Display(Name = "Enable to display unknow signals messages", Order = 1)]
        [Category("Unknow Signals")]
        public bool DisplayContainerMessages = true;

        [Display(Name = "Enable to display only strong unknow signals messages", Order = 2)]
        [Category("Unknow Signals")]
        public bool DisplayOnlyStrongContainerMessages = true;

        [Display(Name = "Message unknow signals spawn", Order = 3)]
        [Category("Unknow Signals")]
        public string ContainerMessage = ":package: {t} has spawn at {c}.";

        [Display(Name = "Message when player got the signal", Order = 4)]
        [Category("Unknow Signals")]
        public string GetedContainerMessage = ":package: {p} just got the {t}.";

        /* Unknow Signals */

        [Display(Name = "Enable to display grid jump messages", Order = 1)]
        [Category("Grids")]
        public bool DisplayGridsJumpMessages = true;

        [Display(Name = "Message when grid jump", Order = 2)]
        [Category("Grids")]
        public string GridJumpMessage = ":rocket: {p} just start a jump with {g} for {d}km through space.";

        [Display(Name = "Unknow Jump Grid Name", Order = 3)]
        [Category("Grids")]
        public string UnknowJumpGridName = "unknow grid";

        /* Player */

        [Display(Name = "Enable to display respawn messages", Order = 1)]
        [Category("Player")]
        public bool DisplayRespawnMessages = true;

        [Display(Name = "Msg when the player respawn", Order = 2)]
        [Category("Player")]
        public string RespawnMessage = ":wheel: The player {p} has respawn in a rover.";

        [Display(Name = "Enable to display player death messages", Order = 3)]
        [Category("Player")]
        public bool DisplayDieMessages = true;

        [Display(Name = "Msg when the player dies", Order = 4)]
        [Category("Player")]
        public string DieMessage = ":skull: The player {p} has died.";

        /* Faction */

        [Display(Name = "Enable to display faction messages", Order = 1)]
        [Category("Faction")]
        public bool DisplayFactionMessages = true;

        [Display(Name = "Ignore Bot In Faction Messages", Order = 2)]
        [Category("Faction")]
        public bool IgnoreBotInFactionMessages = true;

        [Display(Name = "Ignored factions (Ex.: TAG;TAG;TAG)", Order = 2)]
        [Category("Faction")]
        public string IgnoredFactionTags = "";

        [Display(Name = "Msg when a faction is creted", Order = 3)]
        [Category("Faction")]
        public string FactionCretedMessage = ":bust_in_silhouette: The faction {f} has been creted by the player {p}.";

        [Display(Name = "Msg when a faction is removed", Order = 4)]
        [Category("Faction")]
        public string FactionRemovedMessage = ":bust_in_silhouette: A faction has been removed by the player {p}.";

        [Display(Name = "Msg when a faction: simple action", Order = 5)]
        [Category("Faction")]
        public string FactionActionMessage = ":busts_in_silhouette: The faction {f} has {a} to {f2} by the player {p}.";

        [Display(Name = "Msg when a faction: member -> faction", Order = 6)]
        [Category("Faction")]
        public string FactionMemberActionFactionMessage = ":busts_in_silhouette: The player {p} has {a} to the faction {f}.";

        [Display(Name = "Msg when a faction: member -> member", Order = 7)]
        [Category("Faction")]
        public string FactionMemberActionMemberMessage = ":busts_in_silhouette: The player {p} has {a} from {p2} at the faction {f}.";

        [Display(Name = "Action title: Remove Faction", Order = 8)]
        [Category("Faction")]
        public string FactionActionRemoveFaction = "Remove Faction";

        [Display(Name = "Action title: Send Peace Request", Order = 9)]
        [Category("Faction")]
        public string FactionActionSendPeaceRequest = "Send Peace Request";

        [Display(Name = "Action title: Cancel Peace Request", Order = 10)]
        [Category("Faction")]
        public string FactionActionCancelPeaceRequest = "Cancel Peace Request";

        [Display(Name = "Action title: Accept Peace", Order = 11)]
        [Category("Faction")]
        public string FactionActionAcceptPeace = "Accept Peace";

        [Display(Name = "Action title: Declare War", Order = 12)]
        [Category("Faction")]
        public string FactionActionDeclareWar = "Declare War";

        [Display(Name = "Action title: Send Friend Request", Order = 13)]
        [Category("Faction")]
        public string FactionActionSendFriendRequest = "Send Friend Request";

        [Display(Name = "Action title: Cancel Friend Request", Order = 14)]
        [Category("Faction")]
        public string FactionActionCancelFriendRequest = "Cancel Friend Request";

        [Display(Name = "Action title: Accept Friend Request", Order = 15)]
        [Category("Faction")]
        public string FactionActionAcceptFriendRequest = "Accept Friend Request";

        [Display(Name = "Action title: Send Join Request", Order = 16)]
        [Category("Faction")]
        public string FactionActionFactionMemberSendJoin = "Send Join Request";

        [Display(Name = "Action title: Cancel Join Request", Order = 17)]
        [Category("Faction")]
        public string FactionActionFactionMemberCancelJoin = "Cancel Join Request";

        [Display(Name = "Action title: Accept Join Request", Order = 18)]
        [Category("Faction")]
        public string FactionActionFactionMemberAcceptJoin = "Accept Join Request";

        [Display(Name = "Action title: Kick", Order = 19)]
        [Category("Faction")]
        public string FactionActionFactionMemberKick = "Kick";

        [Display(Name = "Action title: Promote", Order = 20)]
        [Category("Faction")]
        public string FactionActionFactionMemberPromote = "Promote";

        [Display(Name = "Action title: Demote", Order = 21)]
        [Category("Faction")]
        public string FactionActionFactionMemberDemote = "Demote";

        [Display(Name = "Action title: Leave", Order = 21)]
        [Category("Faction")]
        public string FactionActionFactionMemberLeave = "Leave";

        [Display(Name = "Action title: Not Possible Join", Order = 23)]
        [Category("Faction")]
        public string FactionActionFactionMemberNotPossibleJoin = "Not Possible Join";

        /* Others */

        [Display(Name = "Send messages into game as Server", Order = 1)]
        [Category("Others")]
        public bool AsServer = false;

        [Display(Name = "Use Status", Order = 2)]
        [Category("Others")]
        public bool UseStatus = true;

        [Display(Name = "Status", Order = 3)]
        [Category("Others")]
        public string Status = "{p} players | SS {ss}";

        [Display(Name = "Discord to Game", Order = 4)]
        [Category("Others")]
        public bool DiscordToGame = true;

        [Display(Name = "Name unknown user as server name", Order = 5)]
        [Category("Others")]
        public bool NameUnknownUserAsServer = true;

        [Display(Name = "Server user name", Order = 6)]
        [Category("Others")]
        public string ServerUserName = "Server";

        public void Save(string userDataPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SEDB_LiteConfig));

            string configFile = Path.Combine(userDataPath, "SEDB-Lite.cfg");
            using (StreamWriter stream = new StreamWriter(configFile, false, Encoding.UTF8))
            {
                serializer.Serialize(stream, this);
            }
        }

        public bool Set(string key, string value)
        {
            switch (key)
            {
                case "enabled":
                    if (bool.TryParse(value, out bool enabled))
                    {
                        Enabled = enabled;
                        return true;
                    }
                    break;
                case "gamechatformat":
                    GameChatFormat = value;
                    return true;
                case "discordchatauthorformat":
                    DiscordChatAuthorFormat = value;
                    return true;
                case "globalcolor":
                    GlobalColor = value;
                    return true;
                case "displaysteamid":
                    if (bool.TryParse(value, out bool displaysteamid))
                    {
                        DisplaySteamID = displaysteamid;
                        return true;
                    }
                    break;
                case "connectedmessage":
                    ConnectedMessage = value;
                    return true;
                case "disconnectedmessage":
                    DisconnectedMessage = value;
                    return true;
                case "serverleftaction":
                    ServerLeftAction = value;
                    return true;
                case "serverdisconnectedaction":
                    ServerDisconnectedAction = value;
                    return true;
                case "serverkickedaction":
                    ServerKickedAction = value;
                    return true;
                case "serverbannedaction":
                    ServerBannedAction = value;
                    return true;
                case "serverstartedmessage":
                    ServerStartedMessage = value;
                    return true;
                case "serverstoppedmessage":
                    ServerStoppedMessage = value;
                    return true;
                case "displaycontainermessages":
                    if (bool.TryParse(value, out bool displaycontainermessages))
                    {
                        DisplayContainerMessages = displaycontainermessages;
                        return true;
                    }
                    break;
                case "displayonlystrongcontainermessages":
                    if (bool.TryParse(value, out bool displayonlystrongcontainermessages))
                    {
                        DisplayOnlyStrongContainerMessages = displayonlystrongcontainermessages;
                        return true;
                    }
                    break;
                case "containermessage":
                    ContainerMessage = value;
                    return true;
                case "displayrespawnmessages":
                    if (bool.TryParse(value, out bool displayrespawnmessages))
                    {
                        DisplayRespawnMessages = displayrespawnmessages;
                        return true;
                    }
                    break;
                case "respawnmessage":
                    RespawnMessage = value;
                    return true;
                case "displaydiemessages":
                    if (bool.TryParse(value, out bool displaydiemessages))
                    {
                        DisplayDieMessages = displaydiemessages;
                        return true;
                    }
                    break;
                case "diemessage":
                    DieMessage = value;
                    return true;
                case "displayfactionmessages":
                    if (bool.TryParse(value, out bool displayfactionmessages))
                    {
                        DisplayFactionMessages = displayfactionmessages;
                        return true;
                    }
                    break;
                case "ignorebotinfactionmessages":
                    if (bool.TryParse(value, out bool ignorebotinfactionmessages))
                    {
                        IgnoreBotInFactionMessages = ignorebotinfactionmessages;
                        return true;
                    }
                    break;
                case "factioncretedmessage":
                    FactionCretedMessage = value;
                    return true;
                case "factionremovedmessage":
                    FactionRemovedMessage = value;
                    return true;
                case "factionactionmessage":
                    FactionActionMessage = value;
                    return true;
                case "factionmemberactionfactionmessage":
                    FactionMemberActionFactionMessage = value;
                    return true;
                case "factionmemberactionmembermessage":
                    FactionMemberActionMemberMessage = value;
                    return true;
                case "status":
                    Status = value;
                    return true;
                case "discordtogame":
                    if (bool.TryParse(value, out bool discordtogame))
                    {
                        DiscordToGame = discordtogame;
                        return true;
                    }
                    break;
                case "debugmode":
                    if (bool.TryParse(value, out bool debugmode))
                    {
                        DebugMode = debugmode;
                        return true;
                    }
                    break;
                case "nameunknownuserasserver":
                    if (bool.TryParse(value, out bool nameunknownuserasserver))
                    {
                        NameUnknownUserAsServer = nameunknownuserasserver;
                        return true;
                    }
                    break;
                case "serverusername":
                    ServerUserName = value;
                    return true;
                case "factionactionremovefaction":
                    FactionActionRemoveFaction = value;
                    return true;
                case "factionactionsendpeacerequest":
                    FactionActionSendPeaceRequest = value;
                    return true;
                case "factionactioncancelpeacerequest":
                    FactionActionCancelPeaceRequest = value;
                    return true;
                case "factionactionacceptpeace":
                    FactionActionAcceptPeace = value;
                    return true;
                case "factionactiondeclarewar":
                    FactionActionDeclareWar = value;
                    return true;
                case "factionactionsendfriendrequest":
                    FactionActionSendFriendRequest = value;
                    return true;
                case "factionactioncancelfriendrequest":
                    FactionActionCancelFriendRequest = value;
                    return true;
                case "factionactionacceptfriendrequest":
                    FactionActionAcceptFriendRequest = value;
                    return true;
                case "factionactionfactionmembersendjoin":
                    FactionActionFactionMemberSendJoin = value;
                    return true;
                case "factionactionfactionmembercanceljoin":
                    FactionActionFactionMemberCancelJoin = value;
                    return true;
                case "factionactionfactionmemberacceptjoin":
                    FactionActionFactionMemberAcceptJoin = value;
                    return true;
                case "factionactionfactionmemberkick":
                    FactionActionFactionMemberKick = value;
                    return true;
                case "factionactionfactionmemberpromote":
                    FactionActionFactionMemberPromote = value;
                    return true;
                case "factionactionfactionmemberdemote":
                    FactionActionFactionMemberDemote = value;
                    return true;
                case "factionactionfactionmemberleave":
                    FactionActionFactionMemberLeave = value;
                    return true;
                case "factionactionfactionmembernotpossiblejoin":
                    FactionActionFactionMemberNotPossibleJoin = value;
                    return true;
            }
            return false;
        }

    }

}
