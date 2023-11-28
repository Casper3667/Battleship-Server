using GameServer.Client_Facing.Messages;
using System.IO;
using System.Net.Sockets;

namespace GameServer.Client_Facing
{
    public class Player
    {
        public string Username { get; set; }
        public byte[,] AttackScreen { get; set; } = new byte[10, 10];
        public byte[,] DefenceScreen { get; set; }=new byte[10, 10];
        bool IsLeading { get; set; }


        private JWT token;
        private TcpClient client;
        private GameServer gameServer;

        private NetworkStream stream;

        public Player(string username, TcpClient client,GameServer server,JWT token)
        {
            this.client = client;
            Username = username;
            this.token = token;
            this.gameServer = server;
            AssignDefenceScreen();
        }
        public Player(string username, TcpClient client, GameServer server,JWT token, byte[,] defenceScreen)
        {
            this.client = client;
            Username = username;
            this.token=token;
            this.gameServer = server;
            DefenceScreen = defenceScreen;

        }
        public void AssignDefenceScreen()
        {

        }
        public void StartHandeling()
        {
            gameServer.AddPlayer(this);
            
        }
        public void StartupProcedure() {
            stream = client.GetStream();

            SendStartupMessage();
            ConnectPlayerToChatService();
        }
        private void HandlePlayer()
        {
            while (gameServer.running)
            {
                try
                {
                    string recievedData = gameServer.RecieveData(stream);
                    Console.WriteLine($"{client.Client.RemoteEndPoint} > {recievedData}");
                    gameServer.SendMessage(recievedData, stream);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{client.Client.RemoteEndPoint} DISCONNECTED");
                    break;
                }
            }
        }

        // TODO: SOFIE Make Code for Connecting Player To Chat Service
        public void ConnectPlayerToChatService()
        {

        }
        #region SENDING

        // TODO: SOFIE Make Send Startup Message Code
        public void SendStartupMessage()
        {
            /// USE PLAYER AND STREAM
            /// 
            var message = new StartupMessage(
                client.Client.RemoteEndPoint.ToString(),
                gameServer.IsGameReady,
                gameServer.GetOtherPlayerUsername(this));

            string data=gameServer.SerializeMessage(message);
            gameServer.SendMessage(data, stream);
        }
        #endregion


    }
}
