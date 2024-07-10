using Sandbox.Game.Gui;

namespace SEDB_LITE
{
    public class ChatMsg
    {
        public ulong Author { get; set; } = 0;

        public string AuthorName { get; set; } = null;

        public string Text { get; set; } = null;

        public ChatChannel Channel { get; set; } = ChatChannel.Global;

        public long Target { get; set; } = 0;

        public string CustomAuthor { get; set; } = null;
    }

}
