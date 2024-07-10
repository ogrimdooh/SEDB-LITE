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
            long identityId = MySession.Static.Players.TryGetIdentityId(steamId);
            if (identityId == 0)
                return steamId.ToString();
            return GetPlayerName(identityId);
        }

        public static string GetPlayerName(long identityId)
        {
            var identity = MySession.Static.Players.TryGetIdentity(identityId);
            if (identity == null)
                return identityId.ToString();
            return identity.DisplayName;
        }
    }

}
