using Sandbox.Game.World;
using SEDB_LITE.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEDB_LITE
{

    static class Utilities
    {
        private static string TryGetAsServerName(string defaultToUse)
        {
            if (Plugin.PluginInstance != null && 
                Plugin.PluginInstance.m_configuration.NameUnknownUserAsServer &&
                !string.IsNullOrWhiteSpace(Plugin.PluginInstance.m_configuration.ServerUserName))
                return Plugin.PluginInstance.m_configuration.ServerUserName;
            return defaultToUse;
        }

        public static bool IsPlayerAdmin(ulong steamUserID)
        {
            try
            {
                if (MySession.Static != null)
                {
                    return MySession.Static.IsUserAdmin(steamUserID);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(Utilities), e);
            }
            return false;
        }

        public static string GetPlayerName(ulong steamId)
        {
            long identityId = 0;
            try
            {
                if (MySession.Static?.Players != null)
                {
                    identityId = MySession.Static.Players.TryGetIdentityId(steamId);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(Utilities), e);
            }
            if (identityId == 0)
                return TryGetAsServerName(steamId.ToString());
            return GetPlayerName(identityId);
        }

        public static string GetPlayerName(long identityId)
        {
            try
            {
                if (MySession.Static?.Players != null)
                {
                    MyPlayer.PlayerId id;
                    if (MySession.Static.Players.TryGetPlayerId(identityId, out id))
                    {
                        var player = MySession.Static.Players.GetPlayerById(id);
                        if (!string.IsNullOrWhiteSpace(player.DisplayName))
                        {
                            return player.DisplayName;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(Utilities), e);
            }
            return TryGetAsServerName(identityId.ToString());
        }
    }

}
