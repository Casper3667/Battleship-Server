﻿using GameServer.Client_Facing.Messages;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Text;

namespace GameServer.Client_Facing
{
    public class Game
    {
        public GameServer Server { get; private set; }
        public List<Player>Players { get; private set; }
        public Player Player1 { get { return Players[0]; } }
        public Player Player2 { get { return Players[1]; } }

        public Player CurrentPlayersTurn { get; private set; }
        public Player? CurrentLeader { get; private set; }

        public bool GameRunning { get; private set; }

        public TimeSpan MaxTurnTime { get; private set; } = TimeSpan.FromMinutes(1); // TODO: This Might need to be Set via a JSON file

        public byte[,] Player1DefaultDefence { get; private set; } = new byte[,] { { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        public byte[,] Player2DefaultDefence { get; private set; } = new byte[,] { { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, { 0, 1, 0, 1, 0, 1, 0, 1, 1, 1 }, { 0, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };

        public byte[,] Player1NewDefaultDefence { get; private set; } = new byte[,]
        {
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 } };
        public byte[,] Player2NewDefaultDefence { get; private set; } = new byte[,]
        {
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
         { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
         { 1, 0, 1, 0, 1, 0, 1, 0, 0, 0 },
         { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 },
         { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 } };


        CancellationTokenSource cancellationSource;

        public string LastAction { get; private set; }     

        public Game(GameServer server, List<Player> players)
        {
            Server = server;
            Players = players;
            cancellationSource=new CancellationTokenSource();
        }
        public void StartGame()
        {
            GameRunning= true;
            //GivePlayersDefaultGameBoards();
            GivePlayersNewDefaultGameBoards();
            //TEST_GivePlayer1CustomGameBoard();
            CurrentPlayersTurn = Player1;
            LastAction = $"Game Started"/*+$" The First Turn Goes To: {CurrentPlayersTurn.Username}"*/;
            SendPlayersCurrentGameState(false);
            
            StartUpdateLoop();
        }
        // TODO: SOFIE Might be a good idea to Send A Kind of End of Game Message To Clients
        /// <summary>
        /// Used to End Current Game
        /// For when either one Player wins or when one player quits
        /// </summary>
        public void EndGame()
        {
            Testing.Print("[Game] Ending Game");
            GameRunning = false;
            cancellationSource.Cancel();
        }
        /// <summary>
        /// Basically Just Runs Update in a while Loop,
        /// All the rest is to make sure the code can terminate the game instantly nomatter where it is in the UpdateMethod
        /// </summary>
        public void StartUpdateLoop()
        {
            //Action action = () => Update();
            //cancellationSource = new CancellationTokenSource();
            //Task updateTask = new Task(action, cancellationSource.Token);
            while (GameRunning && Server.running)
            {
                Update();
                //updateTask.Start();
                //updateTask.Wait(cancellationSource.Token);
            }
        }
        public void Update()
        {

            bool turnIsForfit=Turn();
            if(turnIsForfit == false)
            {
                CheckWhoIsLeading();
                bool HasBeenWon = CheckHasGameBeenWon();
                if (!HasBeenWon)
                {
                    ChangeCurrentPlayersTurn();
                }
                SendPlayersCurrentGameState(HasBeenWon);
            }
            else
            {
                if(cancellationSource.IsCancellationRequested==false)
                {
                    ChangeCurrentPlayersTurn();
                    SendPlayersCurrentGameState(false);
                }
                else
                {
                    //Cancel Game Code
                    SendPlayersGameCancelledGameState(true);
                    GameRunning= false;
                }
            }
            
            

        }
        #region Turn Code
        public bool Turn()
        {

            ShotMessage? shot = WaitForShot();
            if (shot == null)
            {
                LastAction = $"{CurrentPlayersTurn} Took Too Long in Sending in Their shot and has Thus Forfitted this Turn";
                //"Turn Is Forfit"
                return true;
            }
            ShotFeedback? feedback=null;
            if (CurrentPlayersTurn!=null)
            {
                Player otherPlayer = GetOtherPlayer(CurrentPlayersTurn);
                Player attacker = CurrentPlayersTurn;
                feedback = ShotHandler.HandleShot(ref attacker, ref otherPlayer, shot);
            }
           if(feedback!=null)
            {
                if (feedback.IsValidShot)
                {
                    LastAction = $"{CurrentPlayersTurn} Shot at [{(int)feedback.ShotCoordinates.X},{(int)feedback.ShotCoordinates.Y}], The shot was a {feedback.Attacker_Reaction.ToString().ToUpper()}";
                    return false;
                }
                else
                {
                    LastAction = $"{CurrentPlayersTurn}'s Shot was Invalid and they have thus forfitted their turn due to cheating";
                    return true;
                }
            }
           else
                return true;
            
           
        }
        //public ShotMessage? StartWaitForShot()
        //{
        //    Action<ShotMessage> action() =>{ W};
        //}
        public ShotMessage? WaitForShot() // There should be a Time Limit, If You miss it Its Not Your Turn anymore, and you forefit a shot
        {
            TimeSpan TurnTimer = MaxTurnTime;
            ShotMessage? shot = null;
            //(int x, int y) = Console.GetCursorPosition();
            while (TurnTimer.TotalSeconds>0 && TurnTimer!=TimeSpan.Zero)  
            {
                shot = CurrentPlayersTurn.ClientMessageHandler.LatestShotMessage;
                if (shot != null)
                    return shot;
                else
                {
                    Thread.Sleep(100);
                    TurnTimer=TurnTimer.Subtract(TimeSpan.FromMilliseconds(100));
                    //Console.SetCursorPosition(x, y);
                    //Console.WriteLine("Game TurnTimer: " + TurnTimer.TotalSeconds.ToString());
                    if (cancellationSource.IsCancellationRequested)
                    {
                        Print("Game Is Cancelled so Turn Has Ended");
                        return null;
                    }
                }

                
            }


            return shot;
        }
        
        
        #endregion

        public bool CheckHasGameBeenWon()
        {
            bool hasbeenwon = false;

            foreach(Player p in Players)
            {
                if(p.Alive==false)
                {
                    hasbeenwon = true;
                    break;
                }    
            }

            return hasbeenwon;
        }
        public Player? CheckWhoIsLeading()
        {
            Testing.Print("Getting Player 1s Score") ;
            int P1Score = Player1.Score;
            Testing.Print("Getting Player 2s Score");
            int P2Score = Player2.Score;
            Player leader;

            if (P1Score > P2Score) leader = Player1;
            else if (P2Score > P1Score) leader = Player2;
            else leader = null;

            CurrentLeader = leader;
            return leader;
        }

        #region Update Loop Test Code
        public void StartTestUpdateLoopBreak()
        {
            Testing.Print("Starting Test Update Loop");
            Action action = () => UpdateLoopBreakTest();
            cancellationSource = new CancellationTokenSource();
            Task updateTask = new Task(action, cancellationSource.Token);
            Testing.Print("Starting Test Update Loops while loop");
            GameRunning = true;
            while (GameRunning)
            {
                Testing.Print("Update Task Start");
                updateTask.Start();
                Testing.Print("Update Task Wait");
                updateTask.Wait(cancellationSource.Token);
                Testing.Print("Update Task After Wait");
            }
        }
        public List<string> UpdateLoopTestList = new List<string>();
        private void UpdateLoopBreakTest()
        {
            UpdateLoopTestList.Add("Before EndGame");
            Testing.Print("[Game] Before End Game");

            EndGame();
            UpdateLoopTestList.Add("After EndGame");
            Testing.Print("[Game] After End Game");
        }
        #endregion

       

        public void GivePlayersDefaultGameBoards()
        {
            GivePlayersGameBoards(Player1DefaultDefence, Player2DefaultDefence);
        }
        public void GivePlayersNewDefaultGameBoards()
        {
            GivePlayersGameBoards(Player1NewDefaultDefence, Player2NewDefaultDefence);
        }
        public void GivePlayersGameBoards(byte[,] player1, byte[,]player2)
        {
            Players[0].AssignDefenceScreen(player1);
            Players[1].AssignDefenceScreen(player2);

        }
       
        
        public void ChangeCurrentPlayersTurn()
        {
            if (cancellationSource.IsCancellationRequested == false)
            {
                if (CurrentPlayersTurn != null)
                    CurrentPlayersTurn = GetOtherPlayer(CurrentPlayersTurn);
                else
                    CurrentPlayersTurn = Player1;
            }
            else
                Print("Couldnt Change Player Since Game Is Cancelled");
        }

        public Player GetOtherPlayer(Player player)
        {
            if (player == Player1) return Player2;
            else if (player == Player2) return Player1;
            else
            {
                Print("Error: For Some reaon a non existing player was given");
                return player;
            }

        }

        #region Send Game State Message
        private void SendPlayersCurrentGameState(bool HasBeenWon)
        {
            foreach (var player in Players)
            {
                player.SendRawGameStateMessage(WriteGameStateMessage(player, HasBeenWon));

            }

        }
        private void SendPlayersGameCancelledGameState(bool HasBeenWon)
        {
            LastAction = "Since Your Opponent Left The Game You have Automatically WON!!!";


            foreach (var player in Players)
            {
                if(player.client.Connected)
                {
                    player.ResetScreens();
                    CurrentLeader = player;
                    player.SendRawGameStateMessage(WriteGameStateMessage(player, HasBeenWon));
                }
                

            }

        }
        public RawGameStateMessage WriteGameStateMessage(Player player, bool HasBeenWon)
        {
            string turnname="";
            if (CurrentPlayersTurn == player)
            {
                turnname = "Your";
            }
            else
                turnname = "Your Opponents";

            string feedback = LastAction;
            if(HasBeenWon==false)
                feedback = feedback+ $"[Its {turnname} Turn]";

            string? tempopponent = Server.GetOtherPlayerUsername(player);
            string opponent;
            if (tempopponent != null)
            {
                opponent = (string)tempopponent;
            }
            else
                opponent = "GONE";

            var message = new RawGameStateMessage
                (
                opponent,
                feedback,
                ConvertMultiArrayToString(player.AttackScreen),
                ConvertMultiArrayToString(player.DefenceScreen),
                HasBeenWon/*(HasBeenWon)? true: !GameRunning*/,
                CurrentLeader == player,
                CurrentPlayersTurn == player
                );
            return message;
        }
        /// <summary>
        /// This Method is Used to Convert the multi-dimentional byte array used to store players screens into string
        /// so that it can be sent to the client more easily
        /// </summary>
        /// <param name="array">Multi Dimentional array from Player</param>
        /// <returns>string to Send to Client in Game State Message</returns>
        public string ConvertMultiArrayToString(byte[,] array)
        {
            var xlength = array.GetLength(0);
            var ylength = array.GetLength(1);

            bool firstnumber = true;
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < ylength; y++)
            {
                for (int x = 0; x < xlength; x++)
                {
                    if (firstnumber)
                    {
                        firstnumber = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(array[x, y]);
                }
            }


            return sb.ToString();
        }
        #endregion

        #region TESTING
        private void TEST_GivePlayer1CustomGameBoard()
        {
            var player1board = new byte[10, 10];
            for(int y =0;  y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    player1board[x, y] = (byte)((x * 10) + y);
                }
            }
            


            GivePlayersGameBoards(player1board, Player2DefaultDefence);
        }
        #endregion

        void Print(string message)
        {
            Console.WriteLine("[Game]: "+message);
        }

    }
}
