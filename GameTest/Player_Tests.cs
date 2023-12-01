using GameServer.Client_Facing;
using GameServer.Client_Facing.Messages;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameTest
{
    public class Player_Tests
    {
        Player player;
        public byte[,] Player1DefaultDefence { get; private set; } = new byte[,] { { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 0, 0, 0, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        [SetUp]
        public void Setup()
        {
            GameServer.GameServer server = new(IPAddress.Any, 000, 2);
            player =new Player("player 1", new System.Net.Sockets.TcpClient(), server, new JWT("player 1", DateTime.Now, DateTime.Now.AddDays(2)));
        }
        [Test]
        public void Test_Player_Setup()
        {
            Assert.Pass();
        }
        #region - Score TESTS
        [Test]
        public void Test_Player_Score()
        {
            Assert.Inconclusive();
        }
        #endregion
        #region - Health TESTS
        [Test]
        public void Test_Player_Health()
        {
            Assert.Inconclusive();
        }
        #endregion
        #region - Alive TESTS
        [Test]
        public void Test_Player_Alive_True()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Player_Alive_False()
        {
            Assert.Inconclusive();
        }
        #endregion

        #region - AssignDefenceScreen TESTS
        [Test]
        public void Test_Player_AssignDefenceScreen() 
        {
            byte[,] input = new byte[,] { { 1, 3 }, { 2, 0 } };
            byte[,] expect = input;
            player.AssignDefenceScreen(input);
            byte[,] result = player.DefenceScreen;
            Assert.That(result, Is.EqualTo(expect));
        }
        [Test]
        public void Test_Player_AssignDefenceScreen_Default()
        {
            byte[,] input =Player1DefaultDefence;
            byte[,] expect = input;
            player.AssignDefenceScreen(input);
            byte[,] result = player.DefenceScreen;
            Assert.That(result, Is.EqualTo(expect));
        }
        #endregion
        #region - Update Defence and Attack Screen  TESTS
        [Test]
        public void Test_Player_UpdateDefenceScreen()
        {
            Vector2 shot = new(0, 0);
            Player.Defence value = Player.Defence.HitShip;

            byte expect = 2;

            player.UpdateDefenceScreen(shot,value);

            byte result = player.DefenceScreen[(int)shot.X, (int)shot.Y];
            Assert.That(result,Is.EqualTo(expect));
        }
        [Test]
        public void Test_Player_UpdateAttackScreen()
        {
            Vector2 shot = new(0, 0);
            Player.Attack value = Player.Attack.Hit;

            byte expect = 2;

            player.UpdateAttackScreen(shot, value);

            byte result = player.AttackScreen[(int)shot.X, (int)shot.Y];
            Assert.That(result, Is.EqualTo(expect));
        }
        #endregion
        #region - CheckIfShotHits TESTS
        Vector2 Shot_Hits_P1_Default = new Vector2(0, 5);
        Vector2 Shot_Misses_P1_Default = new Vector2(0, 0);
        [Test]
        public void Test_Player_CheckIfShotHits_True()
        {
            player.AssignDefenceScreen(Player1DefaultDefence);
            Vector2 shot = Shot_Hits_P1_Default;
            bool expect = true;

            bool result=player.CheckIfShotHits(shot);    



            Assert.That(result,Is.EqualTo(expect));
        }
        [Test]
        public void Test_Player_CheckIfShotHits_False()
        {
            player.AssignDefenceScreen(Player1DefaultDefence);
            Vector2 shot = Shot_Misses_P1_Default;
            bool expect = false;

            bool result = player.CheckIfShotHits(shot);



            Assert.That(result, Is.EqualTo(expect));
        }
        #endregion
    }
}
