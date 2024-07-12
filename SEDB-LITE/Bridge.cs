using DSharpPlus;
using DSharpPlus.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox.Game;
using System.Timers;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using System.Diagnostics;
using System.Threading.Channels;
using System.Windows.Interop;

namespace SEDB_LITE
{

    public class Bridge
    {
        private static Plugin Plugin;
        private System.Timers.Timer _timer;
        private string lastMessage = "";
        private int retry = 0;
        public static bool Ready { get; set; } = false;
        public static DiscordClient Discord { get; set; }

        public Bridge(Plugin plugin)
        {
            Plugin = plugin;
            RegisterDiscord().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private Task RegisterDiscord()
        {
            Logging.Instance.LogInfo(GetType(), "Registering discord!");
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Plugin.m_configuration.Token,
                TokenType = TokenType.Bot
            });

            Discord.ConnectAsync();
             
            Discord.MessageCreated += Discord_MessageCreated;
            Discord.Ready += async (c, e) =>
            {
                Ready = true;
                MsgWorker.DoLoad();
                await Task.CompletedTask;
            };
            return Task.CompletedTask;
        }

        private Task Discord_MessageCreated(DiscordClient discord, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            try
            {
                if (Plugin.DEBUG)
                    Logging.Instance.LogDebug(GetType(), "Discord message received!");

                if (!e.Author.IsBot && Plugin.m_configuration.DiscordToGame)
                {
                    if (Plugin.m_configuration.ChannelID.Contains(e.Channel.Id.ToString()))
                    {
                        string sender = e.Guild.GetMemberAsync(e.Author.Id).Result.Username;
                        var dSender = Plugin.m_configuration.DiscordChatAuthorFormat.Replace("{p}", sender);

                        //Fix potential message event duplication?
                        if (lastMessage.Equals(dSender + e.Message.Content)) return Task.CompletedTask;

                        lastMessage = dSender + e.Message.Content;
                        MyVisualScriptLogicProvider.SendChatMessageColored(e.Message.Content, VRageMath.Color.MediumPurple, dSender, default, Plugin.m_configuration.GlobalColor);

                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.LogError(GetType(), ex);
            }
            return Task.CompletedTask;
        }

        public void SendStatus(string status)
        {
            DiscordActivity game = new DiscordActivity();
            UserStatus state = new UserStatus();
            if (Ready && status?.Length > 0)
            {
                game.Name = status;
                state = UserStatus.Online;
                Discord.UpdateStatusAsync(game, state);
            }
        }


        public Task SendStatusMessage(string user, ulong steamID, string msg)
        {
            if (Ready && Plugin.m_configuration.ChannelID.Length > 0)
            {
                try
                {
                    DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(Plugin.m_configuration.ChannelID)).Result;
                    if (user != null)
                    {
                        if (user.StartsWith("ID:"))
                            return Task.CompletedTask;

                        if (Plugin.m_configuration.DisplaySteamID)
                        {
                            user = $"{user} ({steamID.ToString()})";
                        }

                        msg = msg.Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    }
                    MsgWorker.SendToDiscord(chann, msg.Replace("/n", "\n"), true);
                }
                catch (Exception e)
                {
                    Logging.Instance.LogError(GetType(), e);
                }
            }
            return Task.CompletedTask;
        }

        public async Task SendChatMessage(string user, string msg)
        {
            if (lastMessage.Equals(user + msg)) return;
            if (Ready && Plugin.m_configuration.ChannelID.Length > 0)
            {
                foreach (var chanID in Plugin.m_configuration.ChannelID.Split(' '))
                {

                    DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(chanID)).Result;
                    
                    if (user != null)
                    {
                        msg = Plugin.m_configuration.GameChatFormat.Replace("{msg}", msg).Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    }
                    try
                    {
                        MsgWorker.SendToDiscord(chann, msg.Replace("/n", "\n"), false);
                    }
                    catch (DSharpPlus.Exceptions.RateLimitException)
                    {
                        if (retry <= 5)
                        {
                            retry++;
                            await SendChatMessage(user, msg);
                            retry = 0;
                        }
                        else
                        {
                            Logging.Instance.LogWarning(GetType(), $"Aborting send chat message (Too many attempts)");
                            Logging.Instance.LogWarning(GetType(), $"Message: {msg}");
                        }
                    }
                    catch (DSharpPlus.Exceptions.RequestSizeException)
                    {
                        Logging.Instance.LogWarning(GetType(), $"Aborting send chat message (Request too large)");
                        Logging.Instance.LogWarning(GetType(), $"Message: {msg}");
                        retry = 0;
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        Logging.Instance.LogWarning(GetType(), $"Unable to send message");
                        Logging.Instance.LogWarning(GetType(), $"Message: {msg}");
                    }
                    catch (DSharpPlus.Exceptions.NotFoundException)
                    {
                        Logging.Instance.LogWarning(GetType(), $"Could not find channel with ID of {chanID}");
                    }
                }
            }
        }

        public void UnloadBot()
        {
            Ready = false;
            Discord?.DisconnectAsync();
        }


        public void StartTimer()
        {
            if (_timer != null) StopTimer();

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        public void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;
            }
        }

        private DateTime timerStart = new DateTime(0);
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (timerStart.Ticks == 0) timerStart = e.SignalTime;

            string status = Plugin.m_configuration.Status;
            DateTime upTime = new DateTime(e.SignalTime.Subtract(timerStart).Ticks);

            Regex regex = new Regex(@"{uptime@(.*?)}");
            if (regex.IsMatch(status))
            {
                var match = regex.Match(status);
                string format = match.Groups[0].ToString().Replace("{uptime@", "").Replace("}", "");
                status = Regex.Replace(status, "{uptime@(.*?)}", upTime.ToString(format));
            }

            SendStatus(status
            .Replace("{p}", MySession.Static.Players.GetOnlinePlayers().Where(p => p.IsRealPlayer).Count().ToString())
            .Replace("{mp}", MySession.Static.MaxPlayers.ToString())
            .Replace("{mc}", MySession.Static.Mods.Count.ToString())
            .Replace("{ss}", MyMultiplayer.Static.ServerSimulationRatio.ToString("0.00")));

        }
    }

}
