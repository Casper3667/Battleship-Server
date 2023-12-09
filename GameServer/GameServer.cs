using System.Net.Sockets;
using System.Net;
using System.Text;
using GameServer.Client_Facing;
using System.Text.Json;
using System.Diagnostics;
using GameServer.Chat_ServiceFacing;

namespace GameServer
{
    public class GameServer
    {
        public const string END_OF_MESSAGE = "#END#";
        public bool useEndMessage { get; private set; } = true;

        public bool running { get; private set; } = false;

        public IPAddress address { get; private set; }
        public int port { get; private set; }
        public string LobbyID { get; private set; }


        TcpListener server;
        public Thread ServerThread { get; private set; }
        private Thread acceptClientThread;
        //List<TcpClient> clients;

        public Game Game { get; private set; }     
        public CancellationTokenSource gameCancellationSource { get; private set; }

        private int maxPlayers = 2;
        public List<TcpClient> NotVerifiedClients = new List<TcpClient>();
        Mutex clientMutex;
        public List<Player> Players { get; private set; }
        Mutex playersMutex = new Mutex();

        public int PlayerCount { get 
            {
                return Players.Count + NotVerifiedClients.Count;
            } }

        //Dictionary<TcpClient, Thread> clientThreads;
        CancellationTokenSource source;
        public CancellationToken cancellationToken {  get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_address"></param>
        /// <param name="_port"></param>
        /// <param name="_lobbyID">For Chat Service, this determines what Group You are in</param>
        /// <param name="_maxPlayers"></param>
        /// <param name="_useEndMessage"></param>
        public GameServer(IPAddress _address, int _port,string _lobbyID, int _maxPlayers, bool _useEndMessage = false)
        {
            this.address = _address;
            this.port = _port;
            this.LobbyID = _lobbyID;
            this.maxPlayers = _maxPlayers;
            this.useEndMessage = _useEndMessage;
            server = new TcpListener(address, port);
            //clientThreads = new Dictionary<TcpClient, Thread>();
            Players = new List<Player>();
            clientMutex = new Mutex();

            source = new CancellationTokenSource();
            cancellationToken = source.Token;
            gameCancellationSource = new CancellationTokenSource();

            ServerThread = new Thread(() => AcceptNewClients(server)) { IsBackground = true };
            //acceptClientThread = new Thread(() => AcceptNewClients(server));
            //acceptClientThread.IsBackground = true;
        }
        public void Start()
        {
            running = true;
            server.Start();
            Print("SERVER STARTED. Listening on Port: " + port);
            source = new CancellationTokenSource();
            cancellationToken = source.Token;
            ServerThread.Start();
            //AcceptNewClients(server);
            //acceptClientThread.Start();

        }
        public void Stop()
        {
            Print("Stopping Game Server");
            running = false;
            server.Stop();
            ServerThread.Abort();
            source.Cancel();

            Print("Clearing Lists");
            NotVerifiedClients.Clear();
            Players.Clear();
            //acceptClientThread.Abort();

        }
        

        private void AcceptNewClients(TcpListener server)
        {
            while (running)
            {
                Print("Server is Checking if it Needs More Clients\nPlayer Count: "+PlayerCount);
                
                if (PlayerCount < maxPlayers)
                {
                    Print("Server Is Accepting New Clients");
                    //ValueTask<TcpClient> temp = server.AcceptTcpClientAsync(cancellationToken);
                    TcpClient temp=server.AcceptTcpClient();

                    if (/*temp.IsCompleted*/true)
                    {
                        TcpClient client = temp/*.Result*/;
                        AddTempClient(client);
                        Print($"Client {client.Client.RemoteEndPoint} Connected");
                        Thread t = new Thread(() => VerifyClient(client));
                        t.IsBackground = true;
                        t.Start();
                        
                        //AddClient(client, t);

                    }

                    //TcpClient client =
                }
                else if (Players.Count == maxPlayers) 
                {
                    Print("Starting Game");
                    StartGame();
                }

                Thread.Sleep(1000);

            }
        }
        public void StartGame()
        {
            gameCancellationSource = new CancellationTokenSource();
            Action action = () =>
            {
                Game = new Game(this, Players);
                Game.StartGame();
            };
            try
            {
                var gameCancellationToken = gameCancellationSource.Token;
                Task startGameTask = new Task(action, gameCancellationToken);
                startGameTask.Start();
                startGameTask.Wait(gameCancellationToken);
            }
            catch (Exception)
            {
                if(gameCancellationSource.IsCancellationRequested)
                {
                    Print("Game Was Cancelled");
                }
                else
                    Print("GameServer_Tried and Failed to Start the Game");
                
                //throw;
            }
            


            //Game = new Game(this, Players);
            //Game.StartGame();
        }
        private void AddTempClient(TcpClient client)
        {
            if(NotVerifiedClients.Contains(client)==false)
            {
                clientMutex.WaitOne();
                NotVerifiedClients.Add(client);
                clientMutex.ReleaseMutex();
            }
            
        }
        private void RemoveTempClient(TcpClient client)
        {
            clientMutex.WaitOne();
            NotVerifiedClients.Remove(client);
            clientMutex.ReleaseMutex();
        }
        
        
        public void AddPlayer(Player player)
        {
            if (Players.Contains(player) == false)
            {
                   
                playersMutex.WaitOne();
                Players.Add(player);
                playersMutex.ReleaseMutex();
                Print($"Added [{player.Username}] To List of Players");
                ChatServiceInterface.SendPublicMessageToService($"{player.Username} Just Joined");
            }
            else
                Console.WriteLine("Game Server Tried to Add Player That was Already Regisered");
        }
        internal void RemovePlayer(Player player)
        {

            playersMutex.WaitOne();
            Players.Remove(player);
            playersMutex.ReleaseMutex();

            if( Game!= null && Game.GameRunning==true)
            {
                Game.EndGame();
            }

            gameCancellationSource.Cancel(); // TODO: SOFIE Might want to move this cancellation
            Print($"Removed [{player.Username}] From List of Players");
            ChatServiceInterface.SendPublicMessageToService($"{player.Username} Just Left");
        }
        private void VerifyClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new(stream);
            Print("Verifing Client");
            Player? player=null;
            try
            {

                (bool valid, JWT? token)=TokenValidator.DecodeAndVerifyJWT(ListenForJWT(stream));
                if(valid)
                {
                    if(token != null)
                    {
                        
                        player = new Player(((JWT)token).Username, client, this, token);

                        Print(client.Client.RemoteEndPoint + "Accepted and will from now on be referred to as: " + player.Username);
                        player.StartHandeling();
                    }
                    else
                    {
                        Debug.Fail("Token is Valid, but for some reason it wasnt returned");
                        Print("Token is Valid, but for some reason it wasnt returned");
                        writer.WriteLine("");
                        writer.Flush();
                    }
                    
                    
                }
                else
                {
                    writer.WriteLine("");
                    writer.Flush();
                }
                // string recievedData = RecieveData(stream);
                //Console.WriteLine($"{client.Client.RemoteEndPoint} > {recievedData}");
                //SendMessage(recievedData, stream);
            }
            catch (Exception)
            {
                Print($"{client.Client.RemoteEndPoint} Was Not Verified");
                writer.WriteLine("");
                writer.Flush();
            }
            if(player!=null)
            {
                Print($"Removing [{player.Username}] From Temp List");
            }
            else
                Print("Removing Client From Temp List");
            RemoveTempClient(client);
        }
        private string ListenForJWT(NetworkStream stream)
        {
            //string message= RecieveData(stream);
            string message= RecieveDataLine(stream);
            Console.WriteLine("Recieved JWT: " + message);
            return message;
        }

        

        internal string RecieveData(NetworkStream stream)
        {
            throw new Exception("We dont Want to Recieve Messages like this");
            StringBuilder sb = new StringBuilder();
            byte[] tempBytes = new byte[1];
            while (true)
            {
                int bytesRead = stream.Read(tempBytes);
                if (bytesRead == 0) { break; }
                string recievedData = Encoding.UTF8.GetString(tempBytes);
                if (useEndMessage)
                {
                    if (recievedData.Contains(END_OF_MESSAGE)) { break; }
                    sb.Append(recievedData);
                }
                else
                {
                    sb.Append(recievedData);
                    break;
                }


            }
            return sb.ToString();
        }
        internal string RecieveDataLine(NetworkStream stream)
        {
            StreamReader reader = new(stream);

            string message = reader.ReadLine() ?? "";
            return message;
        }
        internal void SendMessageLine(string msg, NetworkStream stream)
        {
            //Byte[] data = Encoding.UTF8.GetBytes((useEndMessage) ? msg + END_OF_MESSAGE : msg);
            StreamWriter writer = new(stream);
            writer.WriteLine(msg);
            writer.Flush();
            //stream.Write(data, 0, data.Length);
        }

        internal void SendMessage(string msg, NetworkStream stream)
        {
            throw new Exception("We dont Want to Send Messages like this");
            Byte[] data = Encoding.UTF8.GetBytes((useEndMessage) ? msg + END_OF_MESSAGE : msg);
            stream.Write(data, 0, data.Length);
        }
        public string SerializeMessage(object message)
        {
            string jsonstring = JsonSerializer.Serialize(message);
            return jsonstring;
        }



        #region Plyer Communication
        public bool IsGameReady
        {

            get 
            {
                return Players.Count == 2; 
            }
        }
        public string? GetOtherPlayerUsername(Player player)
        {
            string? OtherPlayer = null;
            Players.ForEach(p => { if (p != player) OtherPlayer = p.Username; });
            return OtherPlayer;
        }

        #endregion
        private void Print(string message)
        {
            Console.WriteLine("[GameServer]: "+message);
        }

    }

}
