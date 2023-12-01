using GameServer.Client_Facing.Messages;
using System;
using GameServer;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GameTest
{
    internal class GameServer_Tests
    {
        GameServer.GameServer GS;

        [SetUp]
        public void Setup()
        {
            GS = new GameServer.GameServer(IPAddress.Any,000,2);
        }
        [Test]
        public void Test_GameServer_Setup()
        {
            Assert.Pass();
        }

        #region - SerializeMessage TESTS
        [Test]
        public void Test_GameServer_SerializeMessage_StartupMessage()
        {
            StartupMessage input = new StartupMessage("Bob", false, null);
            string expect = "{\"ClientID\":\"Bob\",\"GameReady\":false,\"OtherPlayer\":null}";
            string result = GS.SerializeMessage(input);
            Assert.That(expect, Is.EqualTo(result));
            //Assert.AreEqual(expect,result);
        }
        [Test]
        public void Test_GameServer_SerializeMessage_RawGameStateMessage()
        {
            RawGameStateMessage input = new RawGameStateMessage("Bob", "Shot", "001001", "2001010", false, true, false);
            string expect =
                "{" +
                "\"Opponent\":\"Bob\"," +
                "\"LastAction\":\"Shot\"," +
                "\"AttackScreen\":\"001001\"," +
                "\"DefenceScreen\":\"2001010\"," +
                "\"GameDone\":false," +
                "\"IsLeading\":true," +
                "\"PlayerTurn\":false" +
                "}";
            string result = GS.SerializeMessage(input);
            Assert.That(expect, Is.EqualTo(result));
            //            Assert.AreEqual(expect, result);

        }
        [Test]
        public void Test_GameServer_SerializeMessage_RawChatMessage()
        {
            RawChatMessage input = new RawChatMessage("Bob", "Sally", "Hello World");
            string expect = "{\"From\":\"Bob\",\"To\":\"Sally\",\"Message\":\"Hello World\"}";
            string result = GS.SerializeMessage(input);
            Assert.That(expect, Is.EqualTo(result));
            //Assert.AreEqual(expect, result);

        }
        #endregion


    }
}
