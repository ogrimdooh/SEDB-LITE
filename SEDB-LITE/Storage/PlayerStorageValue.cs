using System.Xml.Serialization;

namespace SEDB_LITE.Patches
{
    public class PlayerStorageValue
    {

        [XmlElement]
        public string Key { get; set; }

        [XmlElement]
        public string Value { get; set; }

    }

}
