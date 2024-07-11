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
using Sandbox.Game;
using Sandbox.Engine.Utils;
using Sandbox.Game.World;
using System.Windows.Input;
using VRage.Game.ModAPI;

namespace SEDB_LITE
{

    public static class GameWatcherController
    {

        public static void Init()
        {
            MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
            MyVisualScriptLogicProvider.PlayerDied += MyPlayer_Die;
        }

        public static void Dispose()
        {
            MyEntities.OnEntityAdd -= MyEntities_OnEntityAdd;
            MyVisualScriptLogicProvider.PlayerDied -= MyPlayer_Die;
        }

        private static void MyEntities_OnEntityAdd(VRage.Game.Entity.MyEntity obj)
        {
            try
            {
                if (!Plugin.PluginInstance.m_configuration.Enabled) return;

                var cubeGrid = obj as IMyCubeGrid;
                if (cubeGrid != null)
                {
                    if (cubeGrid.IsRespawnGrid && cubeGrid.BigOwners.Any())
                    {
                        var playerId = cubeGrid.BigOwners[0];
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

    public class Plugin : IConfigurablePlugin
    {
        public SEDB_LiteConfig m_configuration;
        public static Plugin PluginInstance;
        public Bridge DDBridge;
        public static bool DEBUG = false;

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
                    DDBridge.SendStatusMessage(default, default, m_configuration.ServerStartedMessage);


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
            return "SEDiscordBridge - Lite! v1.0.3.1";
        }

        public Task ProcessStatusMessage(string user, ulong player, string message)
        {
            DDBridge.SendStatusMessage(user, player, message);
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
