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
        [Browsable(false)]
        public string GlobalColor = "White";

        [Display(Name = "Display SteamID for each player")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplaySteamID = false;

        [Display(Name = "Message that is displayed when user connects")]
        [Category("General")]
        [Browsable(false)]
        public string ConnectedMessage = ":sunny: {p} joined the server";

        [Display(Name = "Message that is displayed when user connects")]
        [Category("General")]
        [Browsable(false)]
        public string DisconnectedMessage = ":new_moon: {p} left the server";

        [Display(Name = "Message that is displayed when server starts")]
        [Category("General")]
        [Browsable(false)]
        public string ServerStartedMessage = ":white_check_mark: Server Started!";

        [Display(Name = "Message that is displayed when the server stops")]
        [Category("General")]
        [Browsable(false)]
        public string ServerStoppedMessage = ":x: Server Stopped!";

        [Display(Name = "Enable to display unknow signals messages")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplayContainerMessages = true;

        [Display(Name = "Enable to display only strong unknow signals messages")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplayOnlyStrongContainerMessages = true;

        [Display(Name = "Message that is displayed when the player dies")]
        [Category("General")]
        [Browsable(false)]
        public string ContainerMessage = ":package: {t} has spawn at {c}.";

        [Display(Name = "Enable to display respawn messages")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplayRespawnMessages = true;

        [Display(Name = "Message that is displayed when the player respawn")]
        [Category("General")]
        [Browsable(false)]
        public string RespawnMessage = ":wheel: The player {p} has respawn in a rover.";

        [Display(Name = "Enable to display player death messages")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplayDieMessages = true;

        [Display(Name = "Message that is displayed when the player dies")]
        [Category("General")]
        [Browsable(false)]
        public string DieMessage = ":skull: The player {p} has died.";

        [Display(Name = "Enable to display faction messages")]
        [Category("General")]
        [Browsable(false)]
        public bool DisplayFactionMessages = true;

        [Display(Name = "Ignore Bot In Faction Messages")]
        [Category("General")]
        [Browsable(false)]
        public bool IgnoreBotInFactionMessages = true;

        [Display(Name = "Message that is displayed when a faction is creted")]
        [Category("General")]
        [Browsable(false)]
        public string FactionCretedMessage = ":bust_in_silhouette: The faction {f} has been creted by the player {p}.";

        [Display(Name = "Message that is displayed when a faction is removed")]
        [Category("General")]
        [Browsable(false)]
        public string FactionRemovedMessage = ":bust_in_silhouette: A faction has been removed by the player {p}.";

        [Display(Name = "Message that is displayed when a faction take a action (ex.: Send Peace Request)")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionMessage = ":busts_in_silhouette: The faction {f} has {a} to {f2} by the player {p}.";

        [Display(Name = "Message that is displayed when a faction member take a action to other faction (ex.: Send Join Request)")]
        [Category("General")]
        [Browsable(false)]
        public string FactionMemberActionFactionMessage = ":busts_in_silhouette: The player {p} has {a} to the faction {f}.";

        [Display(Name = "Message that is displayed when a faction member take a action to other member (ex.: Accept Join Request)")]
        [Category("General")]
        [Browsable(false)]
        public string FactionMemberActionMemberMessage = ":busts_in_silhouette: The player {p} has {a} from {p2} at the faction {f}.";

        [Display(Name = "Send messages into game as Server")]
        [Category("General")]
        [Browsable(false)]
        public bool AsServer = false;

        [Display(Name = "Use Status")]
        [Browsable(false)]
        public bool UseStatus = true;

        [Display(Name = "Status")]
        [Browsable(false)]
        public string Status = "{p} players | SS {ss}";

        [Display(Name = "Discord to Game")]
        [Category("General")]
        [Browsable(false)]
        public bool DiscordToGame = true;

        [Display(Name = "Debug")]
        [Category("General")]
        [Browsable(false)]
        public bool DebugMode = false;

        [Display(Name = "Name unknown user as server name")]
        [Category("General")]
        [Browsable(false)]
        public bool NameUnknownUserAsServer = true;

        [Display(Name = "Server user name")]
        [Category("General")]
        [Browsable(false)]
        public string ServerUserName = "Server";

        [Display(Name = "Action title when Remove Faction")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionRemoveFaction = "Remove Faction";

        [Display(Name = "Action title when Send Peace Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionSendPeaceRequest = "Send Peace Request";

        [Display(Name = "Action title when Cancel Peace Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionCancelPeaceRequest = "Cancel Peace Request";

        [Display(Name = "Action title when Accept Peace")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionAcceptPeace = "Accept Peace";

        [Display(Name = "Action title when Declare War")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionDeclareWar = "Declare War";

        [Display(Name = "Action title when Send Friend Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionSendFriendRequest = "Send Friend Request";

        [Display(Name = "Action title when Cancel Friend Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionCancelFriendRequest = "Cancel Friend Request";

        [Display(Name = "Action title when Accept Friend Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionAcceptFriendRequest = "Accept Friend Request";

        [Display(Name = "Action title when Send Join Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberSendJoin = "Send Join Request";

        [Display(Name = "Action title when Cancel Join Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberCancelJoin = "Cancel Join Request";

        [Display(Name = "Action title when Accept Join Request")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberAcceptJoin = "Accept Join Request";

        [Display(Name = "Action title when Kick")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberKick = "Kick";

        [Display(Name = "Action title when Promote")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberPromote = "Promote";

        [Display(Name = "Action title when Demote")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberDemote = "Demote";

        [Display(Name = "Action title when Leave")]
        [Category("General")]
        [Browsable(false)]
        public string FactionActionFactionMemberLeave = "Leave";

        [Display(Name = "Action title when Not Possible Join")]
        [Category("General")]
        [Browsable(false)]
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

    }

}
