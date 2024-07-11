using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEDB_LITE
{

    static class Utilities
    {
        public static string GetPlayerName(ulong steamId)
        {
            long identityId = MySession.Static != null ? MySession.Static.Players.TryGetIdentityId(steamId) : 0;
            if (identityId == 0)
                return steamId.ToString();
            return GetPlayerName(identityId);
        }

        public static string GetPlayerName(long identityId)
        {
            if (MySession.Static != null)
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
            return identityId.ToString();
        }
    }

}
