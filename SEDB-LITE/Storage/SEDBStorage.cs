using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace SEDB_LITE.Patches
{

    public class SEDBStorage : BaseStorage
    {

        private const int CURRENT_VERSION = 1;
        private const string FILE_NAME = "SEDB.Lite.Storage.xml";

        public const string KEY_DID_JUMP = "KEY_DID_JUMP";
        public const string KEY_JUMP_COUNT = "KEY_JUMP_COUNT";

        private static SEDBStorage _instance;
        public static SEDBStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }

        private static bool Validate(SEDBStorage settings)
        {
            var res = true;
            return res;
        }

        private static SEDBStorage Upgrade(SEDBStorage settings)
        {

            return settings;
        }

        public static SEDBStorage Load()
        {
            _instance = Load(FILE_NAME, CURRENT_VERSION, Validate, () => { return new SEDBStorage(); }, Upgrade);
            return _instance;
        }

        public static void Save()
        {
            try
            {
                Save(Instance, FILE_NAME);
            }
            catch (Exception e)
            {
                Logging.Instance.LogError(typeof(BaseStorage), e);
            }
        }

        [XmlArray("Players"), XmlArrayItem("Player", typeof(PlayerStorage))]
        public List<PlayerStorage> Players { get; set; } = new List<PlayerStorage>();

        private void CheckPlayers()
        {
            if (Players == null)
                Players = new List<PlayerStorage>();
            Players.RemoveAll(x => x == null);
        }

        public PlayerStorage GetEntity(ulong id)
        {
            CheckPlayers();
            if (Players.Any(x => x.SteamId == id))
                return Players.FirstOrDefault(x => x.SteamId == id);
            var storage = new PlayerStorage() { SteamId = id };
            lock (Players)
            {
                Players.Add(storage);
            }
            return storage;
        }

        public T GetEntityValue<T>(ulong id, string key)
        {
            return GetEntity(id).GetValue<T>(key);
        }

        public void SetEntityValue<T>(ulong id, string key, T value)
        {
            GetEntity(id).SetValue<T>(key, value);
        }

        public void RemoveEntity(ulong id)
        {
            if (Players.Any(x => x.SteamId == id))
                lock (Players)
                {
                    Players.RemoveAll(x => x.SteamId == id);
                }
        }

    }

}
