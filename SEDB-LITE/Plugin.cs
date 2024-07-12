using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Plugins;
using Sandbox.Game.Gui;
using HarmonyLib;
using VRage.Utils;
using Sandbox.Engine.Multiplayer;

using System.Reflection;
using Sandbox.Engine.Utils;
using System.Windows.Input;
using System.Linq;

namespace SEDB_LITE
{

    public class Plugin : IConfigurablePlugin
    {
        public SEDB_LiteConfig m_configuration;
        public static Plugin PluginInstance;
        public Bridge DDBridge;

        public static bool DEBUG
        {
            get
            {
                if (PluginInstance != null)
                    return PluginInstance.m_configuration?.DebugMode ?? false;
                return false;
            }
        }

        public void Init(object gameInstance)
        {
            PluginInstance = this;
            var harmony = new Harmony("SEDB-LITE");
            try
            {
                PatchController.PatchMethods();
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(GetType(), e, "PATCHING FAILED ");
            }

            try
            {
                GameWatcherController.Init();
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(GetType(), e, "WATCHER FAILED ");
            }

            try
            {
                GetConfiguration(VRage.FileSystem.MyFileSystem.UserDataPath);
                Logging.Instance.LogInfo(GetType(), "Starting Discord Bridge!");
                if (m_configuration.Enabled)
                {

                    if (m_configuration.Token.Length <= 0)
                    {
                        Logging.Instance.LogWarning(GetType(), "No BOT token set, plugin will not work at all! Add your bot TOKEN, Update the configuration and restart the server");
                        return;
                    }

                    DDBridge = new Bridge(this);
                    if (m_configuration.UseStatus)
                        DDBridge.StartTimer();

                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(GetType(), e);
            }
        }

        public void Update()
        {

        }

        public IPluginConfiguration GetConfiguration(string userDataPath)
        {
            if (m_configuration == null)
            {
                string configFile = Path.Combine(userDataPath, "SEDB-Lite.cfg");
                if (File.Exists(configFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SEDB_LITE.SEDB_LiteConfig));
                    using (FileStream stream = File.OpenRead(configFile))
                    {
                        m_configuration = serializer.Deserialize(stream) as SEDB_LITE.SEDB_LiteConfig;
                    }
                }
                else
                {
                    Logging.Instance.LogWarning(GetType(), $"Config file not found {configFile}!");
                }

                if (m_configuration == null)
                {
                    m_configuration = new SEDB_LiteConfig();
                }
            }

            return m_configuration;
        }

        public void Dispose()
        {
            Logging.Instance.LogInfo(GetType(), "Unloading SEDB Lite!");
            DDBridge.SendStatusMessage(default, default, m_configuration.ServerStoppedMessage);
            try
            {
                GameWatcherController.Dispose();
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(GetType(), e);
            }
        }

        public string GetPluginTitle()
        {
            return "SEDiscordBridge - Lite! v1.0.3.5";
        }

        public Task ProcessStatusMessage(string user, ulong player, string message)
        {
            DDBridge.SendStatusMessage(user, player, message);
            return Task.CompletedTask;
        }

        public Task ProcessCommandAsync(ChatCommand command)
        {
            try
            {
                if (DEBUG)
                {
                    Logging.Instance.LogDebug(GetType(), $"ProcessCommandAsync : {string.Join(" ", command.Arguments)}");
                }
                if (command.Arguments != null && command.Arguments.Length > 0)
                {
                    switch (command.Arguments[0].ToLower())
                    {
                        case "set":
                            if (command.Arguments.Length >= 3)
                            {
                                if (m_configuration.Set(command.Arguments[1], string.Join(" ", command.Arguments.Skip(2))))
                                {
                                    m_configuration.Save(VRage.FileSystem.MyFileSystem.UserDataPath);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(GetType(), e);
            }
            return Task.CompletedTask;
        }

        public Task ProcessAsync(ChatMsg msg)
        {
            switch (msg.Channel)
            {
                case ChatChannel.Global:
                    return DDBridge.SendChatMessage(msg.AuthorName, msg.Text);

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
