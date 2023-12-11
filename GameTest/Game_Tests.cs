using GameServer;
using GameServer.Client_Facing;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace GameTest
{
    internal class Game_Tests
    {
        Game game;
        Player p1;
        Player p2;

        [SetUp]
        public void Setup()
        {
            GameServer.GameServer server = new(IPAddress.Any, 000,"GameLobby", 2);
            p1 = new Player("player 1", new System.Net.Sockets.TcpClient(), server, new JWT("player 1", DateTime.Now, DateTime.Now.AddDays(2)));
            p2 = new Player("player 2", new System.Net.Sockets.TcpClient(), server, new JWT("player 2", DateTime.Now, DateTime.Now.AddDays(2)));
            List<Player> players = new List<Player>() { p1, p2 };
            game = new Game(server, players);
            Testing.IsTesting = true;
        }
        [Test]
        public void Test_Game_Setup()
        {

            Assert.Pass();
        }
        #region - ConvertMultiArrayToString TESTS
        [Test]
        public void Test_Game_ConvertMultiArrayToString_YAxisIsCorrect()
        {
            byte[,] input = new byte[,] { { 1 /*[0,0]*/, 2 /*[0,1]*/}, { 0/*[1,0]*/, 3 /*[1,1]*/} };

            byte expect = 1;
            byte result = input[0, 0];
            Assert.That(result, Is.EqualTo(expect));
            expect = 0;
            result = input[1, 0];
            Assert.That(result, Is.EqualTo(expect));

            expect = 2;
            result = input[0, 1];
            Assert.That(result, Is.EqualTo(expect));
            expect = 3;
            result = input[1, 1];
            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Game_ConvertMultiArrayToString_Works()
        {
            byte[,] input = new byte[,] { { 1, 2 }, { 0, 3 } };
            string expect = "1,0,2,3";
            string result = game.ConvertMultiArrayToString(input);

            Assert.That(result, Is.EqualTo(expect));
        }
        #endregion
        #region - GivePlayersGameBoard & GivePlayersDefaultGameBoard TESTS
        [Test]
        public void Test_Game_GivePlayersGameBoard_GamesPlayers()
        {
            byte[,] expect1 = new byte[,] { { 1, 2 }, { 3, 0 } };
            byte[,] expect2 = new byte[,] { { 5, 8 }, { 2, 6 } };

            game.GivePlayersGameBoards(expect1, expect2);
            byte[,] result1 = game.Player1.DefenceScreen;
            byte[,] result2 = game.Player2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        [Test]
        public void Test_Game_GivePlayersGameBoard_TestsPlayers()
        {
            byte[,] expect1 = new byte[,] { { 1, 2 }, { 3, 0 } };
            byte[,] expect2 = new byte[,] { { 5, 8 }, { 2, 6 } };

            game.GivePlayersGameBoards(expect1, expect2);
            byte[,] result1 = p1.DefenceScreen;
            byte[,] result2 = p2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        [Test]
        public void Test_Game_GivePlayersDefaultGameBoard_GamesPlayers()
        {
            byte[,] expect1 = game.Player1DefaultDefence;
            byte[,] expect2 = game.Player2DefaultDefence;

            game.GivePlayersDefaultGameBoards();
            byte[,] result1 = game.Player1.DefenceScreen;
            byte[,] result2 = game.Player2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        [Test]
        public void Test_Game_GivePlayersDefaultGameBoard_TestsPlayers()
        {
            byte[,] expect1 = game.Player1DefaultDefence;
            byte[,] expect2 = game.Player2DefaultDefence;

            game.GivePlayersDefaultGameBoards();
            byte[,] result1 = p1.DefenceScreen;
            byte[,] result2 = p2.DefenceScreen;


            // Assert.Multiple()
            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        #endregion
        #region - WriteGameStateMessage TESTS
        [Test]
        public void Test_Game_WriteGameStateMessage_DuringGame()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_WriteGameStateMessage_EfterWin()
        {
            Assert.Inconclusive();
        }
        #endregion
        #region - GetOtherPlayer TESTS
        [Test]
        public void Test_Game_GetOtherPlayer_In_Player1_Out_Player2()
        {
            Player input = p1;
            Player expect = p2;
            Player result = game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));


        }
        [Test]
        public void Test_Game_GetOtherPlayer_In_Player2_Out_Player1()
        {
            Player input = p2;
            Player expect = p1;
            Player result = game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Game_GetOtherPlayer_ERROR()
        {
            GameServer.GameServer server = new(IPAddress.Any, 000, "GameLobby", 2);
            Player input = new Player("Random Player", new System.Net.Sockets.TcpClient(), server, new JWT("Random Player", DateTime.Now, DateTime.Now.AddDays(2)));
            Player expect = input;
            Player result = game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));
        }
        #endregion
        #region - CheckWhoIsLeading TESTS
        [Test]
        public void Test_Game_CheckWhoIsLeading_P1()
        {
            p1.UpdateAttackScreen(new System.Numerics.Vector2(0, 0), Player.Attack.Hit);
            p1.UpdateAttackScreen(new System.Numerics.Vector2(0, 1), Player.Attack.Hit);
            //p1.AttackScreen[0, 0] = 2; // Makes it So that P1 is leading since p2 only has 0s
            Player? expect = p1;

            Player? result = game.CheckWhoIsLeading();

            Assert.That(result, Is.SameAs(expect));
        }
        [Test]
        public void Test_Game_CheckWhoIsLeading_P2()
        {
            p2.UpdateAttackScreen(new System.Numerics.Vector2(0, 0), Player.Attack.Hit);
            p2.UpdateAttackScreen(new System.Numerics.Vector2(0, 3), Player.Attack.Hit);
            //p2.AttackScreen[0, 0] = 2; // Makes it So that P2 is leading since p1 only has 0s
            Player? expect = p2;

            Player? result = game.CheckWhoIsLeading();

            Assert.That(result, Is.SameAs(expect));
           
        }
        [Test]
        public void Test_Game_CheckWhoIsLeading_NoOne_1HitEach()
        {
            p1.AttackScreen[0, 0] = 2; // Makes it So that P1 is leading since p2 only has 0s
            p2.AttackScreen[0, 0] = 2; // Makes it So that both have the same ammount of 2's
            Player? expect = null;

            Player? result = game.CheckWhoIsLeading();

            Assert.That(result, Is.SameAs(expect));
            //Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_CheckWhoIsLeading_NoOne_0HitsEach()
        {
           
            Player? expect = null;

            Player? result = game.CheckWhoIsLeading();

            Assert.That(result, Is.SameAs(expect));
            //Assert.Inconclusive();
        }
        #endregion
        #region - CheckHasGameBeenWon TESTS
        [Test]
        public void Test_Game_CheckHasGameBeenWon_False()
        {
            bool expectWon = false;
            byte[,] expectp1 = game.Player1DefaultDefence;
            byte[,] expectp2 = game.Player2DefaultDefence;
            game.GivePlayersDefaultGameBoards();
            bool result=game.CheckHasGameBeenWon();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectWon), "Won");
                Assert.That(game.Player1.DefenceScreen, Is.EqualTo(expectp1), "Checks Player 1's Defence Screen");
                Assert.That(game.Player2.DefenceScreen, Is.EqualTo(expectp2), "Checks Player 2's Defence Screen");
            });
        }
        [Test]
        public void Test_Game_CheckHasGameBeenWon_True_BothDead()
        {
            //When No Gameboard is Setup for either Player both palyers have no Ships and thus both are dead, thus game is won
            //Though such a thing wouldnt happen in game since Is Won is checked after every turn so when one is dead thats the end of the game
            bool expectWon = true;
            byte[,] expectp1 = new byte[10, 10];
            byte[,] expectp2 = new byte[10, 10];

            bool result = game.CheckHasGameBeenWon();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectWon), "Won");
                Assert.That(game.Player1.DefenceScreen, Is.EqualTo(expectp1), "Checks Player 1's Defence Screen");
                Assert.That(game.Player2.DefenceScreen, Is.EqualTo(expectp2), "Checks Player 2's Defence Screen");
            });
        }
        [Test]
        public void Test_Game_CheckHasGameBeenWon_True_Player1Wins()
        {
            bool expectWon = true;
            byte[,] expectp1 = game.Player1DefaultDefence;
            byte[,] expectp2 = new byte[10, 10];

            game.Player1.AssignDefenceScreen(game.Player1DefaultDefence);
            bool result = game.CheckHasGameBeenWon();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectWon), "Won");
                Assert.That(game.Player1.DefenceScreen, Is.EqualTo(expectp1), "Checks Player 1's Defence Screen");
                Assert.That(game.Player2.DefenceScreen, Is.EqualTo(expectp2), "Checks Player 2's Defence Screen");
            });
        }
        [Test]
        public void Test_Game_CheckHasGameBeenWon_True_Player2Wins()
        {
            bool expectWon = true;
            byte[,] expectp1 = new byte[10, 10];
            byte[,] expectp2 = game.Player2DefaultDefence;

            game.Player2.AssignDefenceScreen(game.Player2DefaultDefence);
            bool result = game.CheckHasGameBeenWon();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectWon), "Won");
                Assert.That(game.Player1.DefenceScreen, Is.EqualTo(expectp1), "Checks Player 1's Defence Screen");
                Assert.That(game.Player2.DefenceScreen, Is.EqualTo(expectp2), "Checks Player 2's Defence Screen");
            });
        }
        #endregion
        //[Test]
        //public void Test_Game_Mock_TestInterruptingUpdateLoop()
        //{
        //    game.StartTestUpdateLoopBreak();
        //    int expect = 1;

        //    int result=game.UpdateLoopTestList.Count;

        //    Assert.That(result, Is.EqualTo(expect));
        //}
    }
}
