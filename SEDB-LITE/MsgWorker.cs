using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;

namespace SEDB_LITE
{
    public static class MsgWorker
    {

        public struct DiscordMessage
        {

            public DiscordChannel Chann { get; set; }
            public string Message { get; set; }
            public DateTime Time { get; set; }

        }
        
        private static ConcurrentQueue<DiscordMessage> messagesToSend = new ConcurrentQueue<DiscordMessage>();
        private static List<DiscordMessage> lastMessages = new List<DiscordMessage>();
        private static Thread mainThread = null;

        private static void DoWork()
        {
            if (Plugin.DEBUG)
            {
                Logging.Instance.LogDebug(typeof(MsgWorker), $"Thread work start!");
            }
            while (true)
            {
                try
                {
                    if (messagesToSend.Any())
                    {
                        DiscordMessage msgToSend;
                        if (messagesToSend.TryDequeue(out msgToSend))
                        {
                            if (!lastMessages.Any(x => x.Message == msgToSend.Message))
                            {
                                if (Plugin.DEBUG)
                                {
                                    Logging.Instance.LogInfo(typeof(MsgWorker), $"Send message : MSG={msgToSend.Message}");
                                }

                                if (Bridge.Discord != null && Bridge.Ready)
                                {
                                    Bridge.Discord.SendMessageAsync(msgToSend.Chann, msgToSend.Message.Replace("/n", "\n")).Wait();
                                }
                                else
                                {
                                    Logging.Instance.LogWarning(typeof(MsgWorker), $"Bridge is not ready!");
                                }
                            }
                            lastMessages.Add(new DiscordMessage() { Chann = msgToSend.Chann, Message = msgToSend.Message, Time = DateTime.Now });
                        }
                    }
                    if (lastMessages.Any())
                    {
                        lastMessages.RemoveAll(x => (DateTime.Now - x.Time).TotalMilliseconds > 1500);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Instance.LogError(typeof(MsgWorker), ex);
                }
                Thread.Sleep(50);
            }
        }

        public static void DoLoad()
        {
            if (mainThread != null)
            {
                if (Plugin.DEBUG)
                {
                    Logging.Instance.LogDebug(typeof(MsgWorker), $"Abort current main Thread!");
                }
                mainThread.Abort();
                mainThread = null;
            }
            mainThread = new Thread(DoWork);
            mainThread.Start();
            if (Plugin.DEBUG)
            {
                Logging.Instance.LogDebug(typeof(MsgWorker), $"Start main Thread!");
            }
        }

        public static void SendToDiscord(DiscordChannel chann, string msg)
        {
            messagesToSend.Enqueue(new DiscordMessage()
            {
                Chann = chann,
                Message = msg,
                Time = DateTime.Now
            });
            if (Plugin.DEBUG)
            {
                Logging.Instance.LogDebug(typeof(MsgWorker), $"Register message : MSG={msg}");
            }
        }

    }

}
