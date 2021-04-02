using DSharpPlus;
using DSharpPlus.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Multiplayer;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox.Game;
using System.Timers;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace SEDB_LITE {
    public class ChatMsg {
        public ulong Author { get; set; }  = 0;

        public string AuthorName { get; set; } = null;

        public string Text { get; set; } = null;

        public ChatChannel Channel { get; set; } = ChatChannel.Global;

        public long Target { get; set; } = 0;

        public string CustomAuthor { get; set; } = null;
    }

    public class Bridge {
        private static Plugin Plugin;
        private Timer _timer;
        public static MyLog Log = new MyLog();
        private string lastMessage = "";
        private int retry = 0;
        public bool Ready { get; set; } = false;
        public static DiscordClient Discord { get; set; }

        public Bridge(Plugin plugin) {
            Plugin = plugin;
            RegisterDiscord().ConfigureAwait(false).GetAwaiter().GetResult();
        }


        private Task RegisterDiscord() {
            Discord = new DiscordClient(new DiscordConfiguration {
                Token = Plugin.m_configuration.Token,
                TokenType = TokenType.Bot
            });

            Discord.ConnectAsync();

            Discord.MessageCreated += Discord_MessageCreated;

            Discord.Ready += async (c,e) => {
                Ready = true;
                await Task.CompletedTask;
            };
            return Task.CompletedTask;
        }

        private Task Discord_MessageCreated(DiscordClient discord, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
            if (!e.Author.IsBot && Plugin.m_configuration.DiscordToGame) {
                if (Plugin.m_configuration.ChannelID.Contains(e.Channel.Id.ToString())) {
                    string sender = e.Guild.GetMemberAsync(e.Author.Id).Result.Username;

                    var dSender = Plugin.m_configuration.DiscordChatAuthorFormat.Replace("{p}", sender);
                    lastMessage = dSender + e.Message.Content;
                    MyVisualScriptLogicProvider.SendChatMessageColored(e.Message.Content, VRageMath.Color.MediumPurple, dSender, default, Plugin.m_configuration.GlobalColor);
                  
                }
            }
                return Task.CompletedTask;
        }

            public void SendStatus(string status) {
            DiscordActivity game = new DiscordActivity();
            UserStatus state = new UserStatus();
            if (Ready && status?.Length > 0) {
                game.Name = status;
                state = UserStatus.Online;
                Discord.UpdateStatusAsync(game, state);
            }
        }


        public async void SendStatusMessage(string user, string msg, ulong steamID) {
            if (Ready && Plugin.m_configuration.ChannelID.Length > 0) {
                try {
                    DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(Plugin.m_configuration.ChannelID)).Result;
                    Log.WriteLineAndConsole($"{user} | {msg} | {steamID}");
                    if (user != null) {
                        if (user.StartsWith("ID:"))
                            return;

                        user = $"{user} ({steamID.ToString()})";

                        msg = msg.Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    }
                    await Discord.SendMessageAsync(chann, msg.Replace("/n", "\n"));
                }
                catch (Exception e) {
                    Log.WriteLineToConsole($"SendStatusMessage: {e.Message}");
                }
            }
        }

        public async Task SendChatMessage(string user, string msg) {
            if (lastMessage.Equals(user + msg)) return;
            if (Ready && Plugin.m_configuration.ChannelID.Length > 0) {
                foreach (var chanID in Plugin.m_configuration.ChannelID.Split(' ')) {

                    DiscordChannel chann = Discord.GetChannelAsync(ulong.Parse(chanID)).Result;
                    //mention
                    //msg = MentionNameToID(msg, chann);

                    if (user != null) {
                        msg = Plugin.m_configuration.GameChatFormat.Replace("{msg}", msg).Replace("{p}", user).Replace("{ts}", TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToString());
                    }
                    try {
                        await Discord.SendMessageAsync(chann, msg.Replace("/n", "\n"));
                    }
                    catch (DSharpPlus.Exceptions.RateLimitException) {
                        if (retry <= 5) {
                            retry++;
                            SendChatMessage(user, msg);
                            retry = 0;
                        }
                        else {
                            Log.WriteLineToConsole($"Aborting send chat message (Too many attempts)");
                            Log.WriteLineToConsole($"Message: {msg}");
                        }
                    }
                    catch (DSharpPlus.Exceptions.RequestSizeException) {
                        Log.WriteLineToConsole($"Aborting send chat message (Request too large)");
                        Log.WriteLineToConsole($"Message: {msg}");
                        retry = 0;
                    }
                    catch (System.Net.Http.HttpRequestException) {
                        Log.WriteLineToConsole($"Unable to send message");
                        Log.WriteLineToConsole($"Message: {msg}");
                    }
                    catch (DSharpPlus.Exceptions.NotFoundException) {
                        Log.WriteLineToConsole($"Could not find channel with ID of {chanID}");
                    }
                }
            }
        }

        public void UnloadBot() {
            Ready = false;
            Discord?.DisconnectAsync();
        }


        public void StartTimer() {
            if (_timer != null) StopTimer();

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        public void StopTimer() {
            if (_timer != null) {
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;
            }
        }


        private int i = 0;
        private DateTime timerStart = new DateTime(0);
        private void _timer_Elapsed(object sender, ElapsedEventArgs e) {

            if (timerStart.Ticks == 0) timerStart = e.SignalTime;

            string status = Plugin.m_configuration.Status;
            DateTime upTime = new DateTime(e.SignalTime.Subtract(timerStart).Ticks);

            Regex regex = new Regex(@"{uptime@(.*?)}");
            if (regex.IsMatch(status)) {
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
