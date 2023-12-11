using System.Numerics;

namespace GameServer.Client_Facing.Messages
{
    public interface IClientMessage
    {
    }

    public class ShotMessage : IClientMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool ValidShotMessage { get; set; }
    }
    public class RawChatMessageFromClient : IClientMessage
    {
        public string From { get; set; } = "";
        public ChatType To { get; set; }
        public string Message { get; set; } = "";
        public bool ValidRawChatMessageFromClient { get; set; }
    }

    public enum ChatType
    {
        Private,
        Group
    }
}
