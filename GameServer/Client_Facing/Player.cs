using GameServer.Client_Facing.Messages;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer.Client_Facing
{
    public class Player
    {
        public string Username { get; set; }
        public byte[,] AttackScreen { get; private set; } = new byte[10, 10];
        public byte[,] DefenceScreen { get;private set; }=new byte[10, 10];
        
        public int Score { get
            {
                int score = 0;
                foreach(var a in AttackScreen)
                {
                    if(a==(byte)Attack.Hit) score++;
                }
                Testing.Print("Score: " + score);
                return score;
            } }
        public int Health { get
            {
                int health = 0;
                foreach (var a in DefenceScreen)
                {
                    if ((int)a == (int)Defence.Ship) health++;
                }

                return health;
            } }
        public bool Alive 
        {
            get { return Health>0;}
        }
        //bool IsLeading { get; set; }

        public ClientMessageHandler ClientMessageHandler { get; private set; }
     


        private JWT token;
        private TcpClient client;
        public  GameServer gameServer { get; private set; }

        private NetworkStream stream;
        public enum Attack
        {
            Unknown, Miss, Hit
        }
        public enum Defence
        {
            Empty, Ship, HitShip
        }
        public Player(string username, TcpClient client,GameServer server,JWT token)
        {
            this.client = client;
            ClientMessageHandler = new(this, this.client);
            Username = username;
            this.token = token;
            this.gameServer = server;
            
            //AssignDefenceScreen();
        }
        public Player(string username, TcpClient client, GameServer server,JWT token, byte[,] defenceScreen)
        {
            this.client = client;

            ClientMessageHandler = new(this, this.client);
            Username = username;
            this.token=token;
            this.gameServer = server;
            DefenceScreen = defenceScreen;

        }
        public void AssignDefenceScreen(byte[,] screen)
        {
            DefenceScreen=screen;
        }

        public void UpdateDefenceScreen(Vector2 point, Defence? value)
        {
            if(value!=null)
            {
                Testing.Print($"Updating Point [{point.X},{point.Y}] in Defence Screen From:[{DefenceScreen[(int)point.X, (int)point.Y]}] To:[{(byte)value}]");
                DefenceScreen[(int)point.X, (int)point.Y] = (byte)value;
                Testing.Print($"[{point.X},{point.Y}] in Defence Screen is now: [{DefenceScreen[(int)point.X, (int)point.Y]}]");
            }
           
        }
        public void UpdateAttackScreen(Vector2 point, Attack value)
        {
            Testing.Print($"Updating Point [{point.X},{point.Y}] in Attack Screen From:[{AttackScreen[(int)point.X, (int)point.Y]}] To:[{(byte)value}]");
            AttackScreen[(int)point.X, (int)point.Y] = (byte)value;
            Testing.Print($"[{point.X},{point.Y}] in Attack Screen is now: [{AttackScreen[(int)point.X, (int)point.Y]}]");
        }
        public bool CheckIfShotHits(Vector2 shotAgainst)
        {
            switch (DefenceScreen[(int)shotAgainst.X,(int)shotAgainst.Y])
            {
                case ((byte)Defence.Empty):
                    return false;
                case ((byte)Defence.HitShip): 
                    return true;
                case ((byte)Defence.Ship):
                    return true;
                default:
                    Console.WriteLine("Player-> Check if Shot Hits, just went Default for some reason");
                    Debug.Fail("Player-> Check if Shot Hits, just went Default for some reason");
                    return false;
                    
            }
        }

        public void StartHandeling()
        {
            
            gameServer.AddPlayer(this);
            StartupProcedure();
        }
        public void StartupProcedure() {
            stream = client.GetStream();

            SendStartupMessage();
            ClientMessageHandler.StartListeningForMessages(stream);
            ConnectPlayerToChatService();
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
                Username,
                gameServer.IsGameReady,
                gameServer.GetOtherPlayerUsername(this));

            string data=gameServer.SerializeMessage(message);
            gameServer.SendMessage(data, stream);
        }

        public void SendRawGameStateMessage(RawGameStateMessage message)
        {
            string data = gameServer.SerializeMessage(message);
            gameServer.SendMessage(data, stream);
        }
        #endregion


    }
    
}
