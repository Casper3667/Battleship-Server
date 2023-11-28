using System.Text.Json.Serialization;

namespace GameServer.Client_Facing.Messages
{
    public interface IServerMessage
    {
    }
    public class StartupMessage : IServerMessage
    {
        [JsonInclude]
        public string ClientID { get; set; }
        [JsonInclude]
        public bool GameReady { get; set; }
        [JsonInclude]
        public string? OtherPlayer { get; set; }

        public StartupMessage(string clientID, bool gameReady, string? otherPlayer)
        {
            ClientID = clientID;
            GameReady = gameReady;
            OtherPlayer = otherPlayer;
        }
    }
    public class RawChatMessage : IServerMessage
    {
        [JsonInclude]
        public string From { get; set; }
        [JsonInclude]
        public string To { get; set; }
        [JsonInclude]
        public string Message { get; set; }
        public RawChatMessage(string from,string to,string message)
        {
            From = from;
            To = to;
            Message = message;
        }
    }
    public class RawGameStateMessage : IServerMessage
    {
        [JsonInclude]
        public string Opponent { get; set; }
        [JsonInclude]
        public string LastAction { get; set; }
        [JsonInclude]
        public string AttackScreen { get; set; }
        [JsonInclude]
        public string DefenceScreen { get; set; }
        [JsonInclude]
        public bool GameDone { get; set; }
        [JsonInclude]
        public bool IsLeading { get; set; }
        [JsonInclude]
        public bool PlayerTurn { get; set; }
        public RawGameStateMessage(string opponent,string lastAction,string attackScreen,string defenceScreen,bool gameDone,bool isLeading,bool playerTurn)
        {
            Opponent = opponent;
            LastAction = lastAction;
            AttackScreen = attackScreen;
            DefenceScreen = defenceScreen;
            GameDone = gameDone;
            IsLeading = isLeading;
            PlayerTurn = playerTurn;
        }
    }
}
