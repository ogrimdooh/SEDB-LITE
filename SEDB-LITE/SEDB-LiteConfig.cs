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

        [Display(Name = "Enabled", Order = 1)]
        [Category("General")]
        [Description("Enable/Disable the Plugin...")]
        public bool Enabled = false;

        [Display(Name = "Bot Token", Order = 2)]
        [Category("General")]
        public string Token = "";

        [Display(Name = "Chat Channel ID", Order = 1)]
        [Category("General")]
        public string ChannelID = "";

        [Display(Name = "Chat format ingame->Discord", Order = 2)]
        [Category("General")]
        public string GameChatFormat = ":rocket: **{p}**: {msg}";

        [Display(Name = "Chat format Discord->ingame", Order = 2)]
        [Category("General")]
        public string DiscordChatAuthorFormat = "[D]{p} {msg}";

        [Display(Name = "ChatColor")]
        [Category("General")]
        public string GlobalColor = "White";

        [Display(Name = "Display SteamID for each player")]
        [Category("General")]
        public bool DisplaySteamID = false;

        [Display(Name = "Msg when user connects")]
        [Category("Server")]
        public string ConnectedMessage = ":sunny: {p} joined the server";

        [Display(Name = "Msg when user disconnects")]
        [Category("Server")]
        public string DisconnectedMessage = ":new_moon: {p} left the server";

        [Display(Name = "Msg when server starts")]
        [Category("Server")]
        public string ServerStartedMessage = ":white_check_mark: Server Started!";

        [Display(Name = "Msg when the server stops")]
        [Category("Server")]
        public string ServerStoppedMessage = ":x: Server Stopped!";

        [Display(Name = "Enable to display unknow signals messages")]
        [Category("Unknow Signals")]
        public bool DisplayContainerMessages = true;

        [Display(Name = "Enable to display only strong unknow signals messages")]
        [Category("Unknow Signals")]
        public bool DisplayOnlyStrongContainerMessages = true;

        [Display(Name = "Message unknow signals spawn")]
        [Category("Unknow Signals")]
        public string ContainerMessage = ":package: {t} has spawn at {c}.";

        [Display(Name = "Enable to display respawn messages")]
        [Category("Player")]
        public bool DisplayRespawnMessages = true;

        [Display(Name = "Msg when the player respawn")]
        [Category("Player")]
        public string RespawnMessage = ":wheel: The player {p} has respawn in a rover.";

        [Display(Name = "Enable to display player death messages")]
        [Category("Player")]
        public bool DisplayDieMessages = true;

        [Display(Name = "Msg when the player dies")]
        [Category("Player")]
        public string DieMessage = ":skull: The player {p} has died.";

        [Display(Name = "Enable to display faction messages")]
        [Category("Faction")]
        public bool DisplayFactionMessages = true;

        [Display(Name = "Ignore Bot In Faction Messages")]
        [Category("Faction")]
        public bool IgnoreBotInFactionMessages = true;

        [Display(Name = "Msg when a faction is creted")]
        [Category("Faction")]
        public string FactionCretedMessage = ":bust_in_silhouette: The faction {f} has been creted by the player {p}.";

        [Display(Name = "Msg when a faction is removed")]
        [Category("Faction")]
        public string FactionRemovedMessage = ":bust_in_silhouette: A faction has been removed by the player {p}.";

        [Display(Name = "Msg when a faction: simple action")]
        [Category("Faction")]
        public string FactionActionMessage = ":busts_in_silhouette: The faction {f} has {a} to {f2} by the player {p}.";

        [Display(Name = "Msg when a faction: member -> faction")]
        [Category("Faction")]
        public string FactionMemberActionFactionMessage = ":busts_in_silhouette: The player {p} has {a} to the faction {f}.";

        [Display(Name = "Msg when a faction: member -> member")]
        [Category("Faction")]
        public string FactionMemberActionMemberMessage = ":busts_in_silhouette: The player {p} has {a} from {p2} at the faction {f}.";

        [Display(Name = "Send messages into game as Server")]
        [Category("General")]
        public bool AsServer = false;

        [Display(Name = "Use Status")]
        [Category("General")]
        public bool UseStatus = true;

        [Display(Name = "Status")]
        [Category("General")]
        public string Status = "{p} players | SS {ss}";

        [Display(Name = "Discord to Game")]
        [Category("General")]
        public bool DiscordToGame = true;

        [Display(Name = "Debug")]
        [Category("General")]
        public bool DebugMode = false;

        [Display(Name = "Name unknown user as server name")]
        [Category("General")]
        public bool NameUnknownUserAsServer = true;

        [Display(Name = "Server user name")]
        [Category("General")]
        public string ServerUserName = "Server";

        [Display(Name = "Action title: Remove Faction")]
        [Category("Faction")]
        public string FactionActionRemoveFaction = "Remove Faction";

        [Display(Name = "Action title: Send Peace Request")]
        [Category("Faction")]
        public string FactionActionSendPeaceRequest = "Send Peace Request";

        [Display(Name = "Action title: Cancel Peace Request")]
        [Category("Faction")]
        public string FactionActionCancelPeaceRequest = "Cancel Peace Request";

        [Display(Name = "Action title: Accept Peace")]
        [Category("Faction")]
        public string FactionActionAcceptPeace = "Accept Peace";

        [Display(Name = "Action title: Declare War")]
        [Category("Faction")]
        public string FactionActionDeclareWar = "Declare War";

        [Display(Name = "Action title: Send Friend Request")]
        [Category("Faction")]
        public string FactionActionSendFriendRequest = "Send Friend Request";

        [Display(Name = "Action title: Cancel Friend Request")]
        [Category("Faction")]
        public string FactionActionCancelFriendRequest = "Cancel Friend Request";

        [Display(Name = "Action title: Accept Friend Request")]
        [Category("Faction")]
        public string FactionActionAcceptFriendRequest = "Accept Friend Request";

        [Display(Name = "Action title: Send Join Request")]
        [Category("Faction")]
        public string FactionActionFactionMemberSendJoin = "Send Join Request";

        [Display(Name = "Action title: Cancel Join Request")]
        [Category("Faction")]
        public string FactionActionFactionMemberCancelJoin = "Cancel Join Request";

        [Display(Name = "Action title: Accept Join Request")]
        [Category("Faction")]
        public string FactionActionFactionMemberAcceptJoin = "Accept Join Request";

        [Display(Name = "Action title: Kick")]
        [Category("Faction")]
        public string FactionActionFactionMemberKick = "Kick";

        [Display(Name = "Action title: Promote")]
        [Category("Faction")]
        public string FactionActionFactionMemberPromote = "Promote";

        [Display(Name = "Action title: Demote")]
        [Category("Faction")]
        public string FactionActionFactionMemberDemote = "Demote";

        [Display(Name = "Action title: Leave")]
        [Category("Faction")]
        public string FactionActionFactionMemberLeave = "Leave";

        [Display(Name = "Action title: Not Possible Join")]
        [Category("Faction")]
        public string FactionActionFactionMemberNotPossibleJoin = "Not Possible Join";

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
