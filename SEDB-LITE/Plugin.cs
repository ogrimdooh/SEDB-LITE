using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Plugins;
using Sandbox.Game.Gui;
using HarmonyLib;
using Sandbox.Game.Entities;
using VRage.Utils;
using Sandbox.Engine.Multiplayer;

using System.Reflection;

namespace SEDB_LITE {
    public class Plugin : IConfigurablePlugin {
        public SEDB_LiteConfig m_configuration;
        public static MyLog Log = new MyLog();
        public Bridge DDBridge;
        public Patches.ChatPatch chatPatch;
        public Patches.PlayerJoinedPatch PlayerJoinedPatch;


        public void Init(object gameInstance) {
            var harmony = new Harmony("SEDB-LITE");

            chatPatch = new Patches.ChatPatch(this);
            PlayerJoinedPatch = new Patches.PlayerJoinedPatch(this);
            try {
                PatchController.PatchMethods();
            } catch(Exception e) {
                Log.WriteLineAndConsole($"PATCHING FAILED {e.ToString()}");
            }

            try {
                GetConfiguration(VRage.FileSystem.MyFileSystem.UserDataPath);
                Log.WriteLineAndConsole("Starting Discord Bridge!");
                if (m_configuration.Enabled) {

                    if (m_configuration.Token.Length <= 0) {
                        Log.Error("No BOT token set, plugin will not work at all! Add your bot TOKEN, Update the configuration and restart the server");
                        return;
                    }

                    DDBridge = new Bridge(this);
                    if (m_configuration.UseStatus)
                        DDBridge.StartTimer();

                }
            } catch (Exception e) {
                Log.WriteLineAndConsole(e.ToString());
            }
        }

        public void Update() {
            
        }

        public IPluginConfiguration GetConfiguration(string userDataPath) {
            if (m_configuration == null) {
                string configFile = Path.Combine(userDataPath, "SEDB-Lite.cfg");
                if (File.Exists(configFile)) {
                    XmlSerializer serializer = new XmlSerializer(typeof(SEDB_LITE.SEDB_LiteConfig));
                    using (FileStream stream = File.OpenRead(configFile)) {
                        m_configuration = serializer.Deserialize(stream) as SEDB_LITE.SEDB_LiteConfig;
                    }
                }

                if (m_configuration == null) {
                    m_configuration = new SEDB_LITE.SEDB_LiteConfig();
                }
            }

            return m_configuration;
        }

        public void Dispose() {

        }

        public string GetPluginTitle() {
            return "SEDiscordBridge - Lite! v1.001";
        }


        public Task ProcessStatusMessage(string user, ulong player, string message) {
            DDBridge.SendStatusMessage(user, player, message);
            return Task.CompletedTask;
        }

        public Task ProcessAsync(ChatMsg msg) {
            switch (msg.Channel) {
                case ChatChannel.Global:
                    DDBridge.SendChatMessage(msg.AuthorName, msg.Text);
                    break;

                case ChatChannel.GlobalScripted:
                    break;

                case ChatChannel.Faction:
                    break;

                case ChatChannel.Private:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
