using GameServer.Client_Facing;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameTest
{
    public class Player_Tests
    {
        Player player;
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
        [Test]
        public void Test_Player_AssignDefenceScreen() 
        {
            var input = new byte[,] { { 1, 3 }, { 2, 0 } };
            byte[,] expect = input;
            player.AssignDefenceScreen(input);
            byte[,] result = player.DefenceScreen;
            Assert.That(result, Is.EqualTo(expect));
        }

        [Test]
        public void Test_Player_UpdateDefenceScreen()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Player_UpdateAttackScreen()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Player_CheckIfShotHits_True()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void Test_Player_CheckIfShotHits_False()
        {
            Assert.Inconclusive();
        }

    }
}
