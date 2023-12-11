using GameServer;
using GameServer.Client_Facing;
using GameServer.Client_Facing.Messages;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameTest
{
    internal class ShotHandler_Tests
    {
        Game game;
        Player p1;
        Player p2;

        Vector2 Invalid_Shot = new Vector2(100, 42); // Should Most Definately be invalid
        Vector2 Valid_Shot = new Vector2(5, 5); //Should Most Definately Be Valid

        Vector2 Shot_Hits_P1_Default = new Vector2(0, 5);
        ShotMessage SM_Hit_P1_Default=new ShotMessage() { X= 0, Y = 5 };
        Vector2 Shot_Misses_P1_Default = new Vector2(0, 0);
        ShotMessage SM_Misses_P1_Default = new ShotMessage() { X = 0, Y = 0 };

        [SetUp]
        public void Setup()
        {
            GameServer.GameServer server = new(IPAddress.Any, 000, "Main_Lobby", 2);
            p1 = new Player("player 1", new System.Net.Sockets.TcpClient(), server, new JWT("player 1", DateTime.Now, DateTime.Now.AddDays(2)));
            p2 = new Player("player 2", new System.Net.Sockets.TcpClient(), server, new JWT("player 2", DateTime.Now, DateTime.Now.AddDays(2)));
            List<Player> players = new List<Player>() { p1, p2 };
            game = new Game(server, players);
            game.GivePlayersDefaultGameBoards();
            Testing.IsTesting = true;
        }
        [Test]
        public void Test_ShotHandler_Setup()
        {
            Assert.Pass();
        }
        #region - HandleShot Tests
        [Test]
        public void Test_ShotHandler_HandleShot_Invalid()
        {
            ShotMessage sm = new ShotMessage() { X = 100, Y = 42 };
            ShotFeedback feedback=ShotHandler.HandleShot(ref p2,ref p1, sm);

            Assert.Multiple(() =>
            {
                Assert.That(feedback.IsValidShot, Is.False, "Checks that IsValid is False");
                Assert.That(feedback.ShotCoordinates, Is.EqualTo(new Vector2(100, 42)), "Checks That Shot Was Decoded Correctly");
            });
            
        }
        [Test]
        public void Test_ShotHandler_HandleShot_Valid_Hit()
        {
            ShotMessage sm = SM_Hit_P1_Default;
           

            bool expectValid = true;
            Vector2 expectShot = Shot_Hits_P1_Default;
            Player.Attack expectAttack = Player.Attack.Hit;
            Player.Defence expectDefence = Player.Defence.HitShip;

            ShotFeedback feedback = ShotHandler.HandleShot(ref p2,ref p1, sm);

            Assert.Multiple(() =>
            {
                Assert.That(feedback.IsValidShot, Is.EqualTo(expectValid), "Checks that IsValid is TRUE");
                Assert.That(feedback.ShotCoordinates, Is.EqualTo(expectShot), "Checks That Shot Was Decoded Correctly");
                Assert.That(feedback.Attacker_Reaction, Is.EqualTo(expectAttack), "Checks That Attack Reaction is Hit");
                Assert.That(feedback.Defender_Reaction, Is.EqualTo(expectDefence), "Checks That Defence Reaction is HitShip");
            });

            

        }
        [Test]
        public void Test_ShotHandler_HandleShot_Valid_Miss()
        {

            ShotMessage sm =SM_Misses_P1_Default;
           

            bool expectValid = true;
            Vector2 expectShot = Shot_Misses_P1_Default;
            Player.Attack expectAttack = Player.Attack.Miss;
            Player.Defence? expectDefence = null;

            ShotFeedback feedback = ShotHandler.HandleShot(ref p2,ref p1, sm);

            Assert.Multiple(() =>
            {
                Assert.That(feedback.IsValidShot, Is.EqualTo(expectValid), "Checks that IsValid is TRUE");
                Assert.That(feedback.ShotCoordinates, Is.EqualTo(expectShot), "Checks That Shot Was Decoded Correctly");
                Assert.That(feedback.Attacker_Reaction, Is.EqualTo(expectAttack), "Checks That Attack Reaction is Hit");
                Assert.That(feedback.Defender_Reaction, Is.EqualTo(expectDefence), "Checks That Defence Reaction is HitShip");
            });
        }
        #endregion
        #region - DecodeShot Tests
        [Test]
        public void Test_ShotHandler_DecodeShot()
        {
            ShotMessage sm=new ShotMessage() { X = 5, Y = 7 };
            Vector2 expect = new Vector2(5, 7);
            Vector2 result = ShotHandler.DecodeShot(sm);

            Assert.That(result, Is.EqualTo(expect));
            
        }
        #endregion
        #region - CheckIfShotIsInsideBounds Tests
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_True_X5Y5()
        {
            Vector2 minimum = new Vector2(0, 0);
            Vector2 middle = Valid_Shot; //5,5
            Vector2 maximum = new Vector2(9, 9);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, minimum), Is.True, "Minimum Value For True(0,0)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, middle), Is.True, "Middle Value For True(5,5)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, maximum), Is.True, "Maximum Value For True(9,9)");
            });
            //Assert.Inconclusive();
        }
        [Test]
        public void Test_ShotHandler_CheckIfShotIsInsideBounds_False()
        {
            Vector2 closestToMinimum = new Vector2(-1, -1);
            Vector2 extreme = Invalid_Shot; //100,42
            Vector2 closestToMaximum = new Vector2(10, 10);

            Assert.Multiple(() =>
            {
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, closestToMinimum), Is.False, "Closest False Value to Minimum Value For True(-1,-1)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, extreme), Is.False, "Extreme Value For False(100,42)");
                Assert.That(ShotHandler.CheckIfShotIsInsideBounds(p1, closestToMaximum), Is.False, "Closest False Value to Maximum Value For True(10,10)");
            });
        }
        #endregion
        #region - SeeIfAttackHits Tests
        [Test]
        public void Test_ShotHandler_SeeIfAttackHits_Hit()
        {
            Vector2 shot = Shot_Hits_P1_Default; //0,5
            Player defender = p1;

            Assert.Multiple(() =>
            {
                Assert.That(defender.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo((byte)Player.Defence.Ship), "Check That There is A Ship There");
                Assert.That(ShotHandler.SeeIfAttackHits(shot, defender).attacker_reaction, Is.EqualTo(Player.Attack.Hit), "Checking if Attack Hits Like its Supposed To");
            });
        }
        [Test]
        public void Test_ShotHandler_SeeIfAttackHits_Miss()
        {
            Vector2 shot = Shot_Misses_P1_Default; //0,0
            Player defender = p1;

            Assert.Multiple(() =>
            {
                Assert.That(defender.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo((byte)Player.Defence.Empty), "Check That There is NOT A Ship There");
                Assert.That(ShotHandler.SeeIfAttackHits(shot, defender).attacker_reaction, Is.EqualTo(Player.Attack.Miss), "Checking if Attack Misses Like its Supposed To");
            });
        }

        //[Test]
        //public void Test_ShotHandler_SeeIfAttackHits_P1_All()
        //{
        //    Assert.Inconclusive();
        //}
        //[Test]
        //public void Test_ShotHandler_SeeIfAttackHits_P2_All()
        //{
        //    Assert.Inconclusive();
        //}
        #endregion
        #region - ApplyHitToPlayers Tests
        [Test]
        public void Test_ShotHandler_ApplyHitToPlayers_Hit()
        {
            Vector2 shot = Shot_Hits_P1_Default;
            Player a = p2;
            Player.Attack ar= Player.Attack.Hit;
            Player d = p1;
            Player.Defence dr = Player.Defence.HitShip;
            ShotHandler.ApplyHitToPlayers(shot, ref a, ar,ref d, dr);

            byte expectA =(byte)ar;
            byte expectD = (byte)dr;

            Assert.Multiple(() =>
            {
                Assert.That(p1, Is.SameAs(game.Player1), "Checks That tests p1 is the same as games Player1");
                Assert.That(p2, Is.SameAs(game.Player2), "Checks That tests p2 is the same as games Player2");
                Assert.That(p2.AttackScreen[(int)shot.X,(int)shot.Y],Is.EqualTo(expectA),"Checks That Attacker Regisered the Hit");
                Assert.That(p1.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectD), "Checks That Defender Regisered the Hit");
            });

        }
        [Test]
        public void Test_ShotHandler_ApplyHitToPlayers_Miss()
        {
            Vector2 shot = Shot_Misses_P1_Default;
            Player a = p2;
            Player.Attack ar = Player.Attack.Miss;
            Player d = p1;
            Player.Defence? dr = null;
            ShotHandler.ApplyHitToPlayers(shot, ref a, ar,ref d, dr);

            byte expectA = (byte)ar;
            byte expectD = (byte)Player.Defence.Empty;

            Assert.Multiple(() =>
            {
                Assert.That(p1, Is.SameAs(game.Player1), "Checks That tests p1 is the same as games Player1");
                Assert.That(p2, Is.SameAs(game.Player2), "Checks That tests p2 is the same as games Player2");
                Assert.That(p2.AttackScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectA), "Checks That Attacker Regisered the Miss");
                Assert.That(p1.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectD), "Checks That Defender Didnt Register anything since Attacker missed");
            });
        }
        [Test]
        public void Test_ShotHandler_ApplyHitToPlayers_AND_SeeIfAttackHits_Hit()
        {
            Vector2 shot = Shot_Hits_P1_Default;
            Player a = p2;
            Player d = p1;
            (Player.Attack ar, Player.Defence? dr) =ShotHandler.SeeIfAttackHits(shot, d);
            ShotHandler.ApplyHitToPlayers(shot, ref a, ar, ref d, dr);

            byte expectA = (byte)ar;
            byte? expectD = (byte?)dr;

            Assert.Multiple(() =>
            {
                Assert.That(p1, Is.SameAs(game.Player1), "Checks That tests p1 is the same as games Player1");
                Assert.That(p2, Is.SameAs(game.Player2), "Checks That tests p2 is the same as games Player2");
                Assert.That(p2.AttackScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectA), "Checks That Attacker Regisered the Hit");
                Assert.That(p1.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectD), "Checks That Defender Regisered the Hit");
            });
        }
        [Test]
        public void Test_ShotHandler_ApplyHitToPlayers_AND_SeeIfAttackHits_Miss()
        {
            Vector2 shot = Shot_Misses_P1_Default;
            Player a = p2;
            Player d = p1;
            (Player.Attack ar, Player.Defence? dr) = ShotHandler.SeeIfAttackHits(shot, d);
            ShotHandler.ApplyHitToPlayers(shot, ref a, ar, ref d, dr);

            byte expectA = (byte)ar;
            byte expectD = (byte)Player.Defence.Empty;

            Assert.Multiple(() =>
            {
                Assert.That(p1, Is.SameAs(game.Player1), "Checks That tests p1 is the same as games Player1");
                Assert.That(p2, Is.SameAs(game.Player2), "Checks That tests p2 is the same as games Player2");
                Assert.That(p2.AttackScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectA), "Checks That Attacker Regisered the Miss");
                Assert.That(p1.DefenceScreen[(int)shot.X, (int)shot.Y], Is.EqualTo(expectD), "Checks That Defender Didnt Register anything since Attacker missed");
            });
            
        }
        #endregion

    }
}
