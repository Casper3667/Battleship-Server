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
    }
    public class RawChatMessageOut : IClientMessage
    {
        public string From { get; set; }
        public string Message { get; set; }
    }
}
