using GameServer;
using GameServer.Controllers;
using Microsoft.Extensions.Logging;

namespace GameTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            ServerHealth serv = new();
            int players = serv.Get();
            int expectedPlayers = 0;

            Assert.NotNull(players);
            Assert.That(expectedPlayers, Is.EqualTo(players));
        }
    }
}