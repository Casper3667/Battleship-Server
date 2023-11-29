using System.Net.Sockets;
using System.Net;
using System.Text;
using GameServer.Client_Facing;
using System.Text.Json;

namespace GameServer
{
    public class GameServer
    {
        const string END_OF_MESSAGE = "#END#";
        bool useEndMessage;

        public bool running { get; private set; } = false;

        private IPAddress address;
        private int port;
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
        Mutex playersMutex;

        

        //Dictionary<TcpClient, Thread> clientThreads;
        CancellationTokenSource source;
        public CancellationToken cancellationToken {  get; private set; }

        public GameServer(IPAddress _address, int _port, int _maxPlayers, bool _useEndMessage = false)
        {
            this.address = _address;
            this.port = _port;
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
            source = new CancellationTokenSource();
            cancellationToken = source.Token;
            ServerThread.Start();
            //AcceptNewClients(server);
            //acceptClientThread.Start();

        }
        public void Stop()
        {
            running = false;
            server.Stop();
            ServerThread.Abort();
            source.Cancel();
            //acceptClientThread.Abort();

        }

        private void AcceptNewClients(TcpListener server)
        {
            while (running)
            {
                if (Players.Count + NotVerifiedClients.Count < maxPlayers)
                {
                    ValueTask<TcpClient> temp = server.AcceptTcpClientAsync(cancellationToken);
                    if (temp.IsCompleted)
                    {
                        TcpClient client = temp.Result;
                        Thread t = new Thread(() => VerifyClient(client));
                        t.IsBackground = true;
                        t.Start();
                        AddTempClient(client);
                        //AddClient(client, t);

                    }

                    //TcpClient client =
                }
                else if (Players.Count == maxPlayers) 
                {
                    StartGame();
                }

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
                Console.WriteLine("GameServer_Tried and Failed to Start the Game");

                throw;
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
            }
            else
                Console.WriteLine("Game Server Tried to Add Player That was Already Regisered");
        }
        private void RemovePlayer(Player player)
        {

            playersMutex.WaitOne();
            Players.Remove(player);
            playersMutex.ReleaseMutex();

            if( Game!= null && Game.GameRunning==true)
            {
                Game.EndGame();
            }

            gameCancellationSource.Cancel(); // TODO: SOFIE Might want to move this cancellation
            
        }
        private void VerifyClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            

            try
            {
                (bool valid, JWT token)=TokenValidator.DecodeAndVerifyJWT(ListenForJWT(stream));
                if(valid)
                {
                    var player = new Player(token.Username, client,this, token);
                    player.StartHandeling();
                    
                }
                // string recievedData = RecieveData(stream);
                //Console.WriteLine($"{client.Client.RemoteEndPoint} > {recievedData}");
                //SendMessage(recievedData, stream);
            }
            catch (Exception)
            {
                Console.WriteLine($"{client.Client.RemoteEndPoint} Was Not Verified");

            }

            RemoveTempClient(client);
        }
        private string ListenForJWT(NetworkStream stream)
        {
            return RecieveData(stream);
        }

        

        internal string RecieveData(NetworkStream stream)
        {
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
        internal void SendMessage(string msg, NetworkStream stream)
        {
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

    }

}
