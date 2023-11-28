namespace GameServer.Client_Facing
{
    public class Game
    {
        public GameServer Server { get; private set; }
        public List<Player>Players { get; private set; }

        public Player CurrentPlayersTurn { get; private set; }
        
        public bool GameRunning { get; private set; }

        public Game(GameServer server, List<Player> players)
        {
            Server = server;
            Players = players;

        }
        public void StartGame()
        {
            GameRunning= true;

            while (GameRunning && Server.running)
            {
                Update();
            }
        }
        public void Update() 
        {
            
        }

    }
}
