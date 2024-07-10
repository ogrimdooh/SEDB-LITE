﻿using System;
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

        [Display(Name = "Message that is displayed when the player respawn")]
        [Category("General")]
        [Browsable(false)]
        public string RespawnMessage = ":wheel: The player {p} has respawn in a rover.";

        [Display(Name = "Message that is displayed when the player dies")]
        [Category("General")]
        [Browsable(false)]
        public string DieMessage = ":skull: The player {p} has died.";

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
