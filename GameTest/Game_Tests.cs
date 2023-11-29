using GameServer;
using GameServer.Client_Facing;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            GameServer.GameServer server = new(IPAddress.Any, 000, 2);
            p1 = new Player("player 1", new System.Net.Sockets.TcpClient(), server, new JWT("player 1", DateTime.Now, DateTime.Now.AddDays(2)));
            p2 = new Player("player 2", new System.Net.Sockets.TcpClient(), server, new JWT("player 2", DateTime.Now, DateTime.Now.AddDays(2)));
            List<Player> players = new List<Player>() { p1, p2 };
            game = new Game(server, players);
            Testing.IsTesting = true;
        }

        [Test]
        public void Test_Setup()
        {

            Assert.Pass();
        }
        [Test]
        public void Test_ConvertMultiArrayToString_YAxisIsCorrect()
        {
            var input = new byte[,] { { 1 /*[0,0]*/, 2 /*[0,1]*/}, { 0/*[1,0]*/, 3 /*[1,1]*/} };

            byte expect = 1;
            var result = input[0, 0];
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
        public void Test_ConvertMultiArrayToString_Works()
        {
            var input = new byte[,] { { 1, 2 }, { 0, 1 } };
            string expect = "1,2,0,1";
            var result = game.ConvertMultiArrayToString(input);

            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_GivePlayersGameBoard_GamesPlayers()
        {
            var expect1 = new byte[,] { { 1, 2 }, { 3, 0 } };
            var expect2 = new byte[,] { { 5, 8 }, { 2, 6 } };

            game.GivePlayersGameBoards(expect1, expect2);
            var result1 = game.Player1.DefenceScreen;
            var result2 = game.Player2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        public void Test_GivePlayersGameBoard_TestsPlayers()
        {
            var expect1 = new byte[,] { { 1, 2 }, { 3, 0 } };
            var expect2 = new byte[,] { { 5, 8 }, { 2, 6 } };

            game.GivePlayersGameBoards(expect1, expect2);
            var result1 = p1.DefenceScreen;
            var result2 = p2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        [Test]
        public void Test_GivePlayersDefaultGameBoard_GamesPlayers()
        {
            var expect1 = game.Player1DefaultDefence;
            var expect2 = game.Player2DefaultDefence;

            game.GivePlayersDefaultGameBoards();
            var result1 = game.Player1.DefenceScreen;
            var result2 = game.Player2.DefenceScreen;



            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        [Test]
        public void Test_GivePlayersDefaultGameBoard_TestsPlayers()
        {
            var expect1 = game.Player1DefaultDefence;
            var expect2 = game.Player2DefaultDefence;

            game.GivePlayersDefaultGameBoards();
            var result1 = p1.DefenceScreen;
            var result2 = p2.DefenceScreen;


           // Assert.Multiple()
            Assert.That(result1, Is.EqualTo(expect1));
            Assert.That(result2, Is.EqualTo(expect2));
        }
        
        //[Test]
        //public void Test_Game_Mock_TestInterruptingUpdateLoop()
        //{
        //    game.StartTestUpdateLoopBreak();
        //    int expect = 1;
            
        //    int result=game.UpdateLoopTestList.Count;

        //    Assert.That(result, Is.EqualTo(expect));
        //}

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

        [Test]
        public void Test_Game_GetOtherPlayer_In_Player1()
        {
            var input = p1;
            var expect = p2;
            var result= game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));


        }
        [Test]
        public void Test_Game_GetOtherPlayer_In_Player2()
        {
            var input = p2;
            var expect = p1;
            var result = game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Game_GetOtherPlayer_ERROR()
        {
            GameServer.GameServer server = new(IPAddress.Any, 000, 2);
            var input = new Player("Random Player", new System.Net.Sockets.TcpClient(), server, new JWT("Random Player", DateTime.Now, DateTime.Now.AddDays(2)));
            var expect = input;
            var result = game.GetOtherPlayer(input);
            Assert.That(result, Is.EqualTo(expect));
        }

        [Test]
        public void Test_Game_CheckWhoIsLeading_P1()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_CheckWhoIsLeading_P2()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_CheckWhoIsLeading_NoOne()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_CheckHasGameBeenWon_False()
        {

            Assert.Inconclusive();
        }
        [Test]
        public void Test_Game_CheckHasGameBeenWon_True()
        {

            Assert.Inconclusive();
        }
    }
}
