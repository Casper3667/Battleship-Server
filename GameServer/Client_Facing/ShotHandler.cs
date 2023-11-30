using GameServer.Client_Facing.Messages;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace GameServer.Client_Facing
{
    public static class ShotHandler
    {
        public static ShotFeedback HandleShot(ref Player Attacker, ref Player Defender, ShotMessage shotMessage)
        {
            ShotFeedback feedback;
            var shot = DecodeShot(shotMessage);
            if(CheckIfShotIsInsideBounds(Defender,shot))
            {
                (Player.Attack attacker_reaction, Player.Defence? defender_reaction) =SeeIfAttackHits(shot, Defender);
                ApplyHitToPlayers(shot, ref Attacker, attacker_reaction, ref Defender, defender_reaction);
                feedback = new(true,shot,defender_reaction,attacker_reaction);
            }
            else
            {
                feedback = new ShotFeedback(false, shot, null, null);
            }
            return feedback;
        }
        public static Vector2 DecodeShot(ShotMessage message)
        {
            Vector2 shot = new Vector2(message.X, message.Y);
            return shot;
        }

        // TODO: SOFIE Make Code For Checking if Shot is Inside Bounds
        public static bool CheckIfShotIsInsideBounds(Player defender,Vector2 shot)
        {
            bool valid = false;
            int sX = (int)shot.X;
            int sY = (int)shot.Y;

            int gX = defender.DefenceScreen.GetLength(0);
            int gY = defender.DefenceScreen.GetLength(1);

            if (sX >= 0 && sX < gX)
                if (sY >= 0 && sY < gY)
                    valid = true;




            return valid;
        }
        public static (Player.Attack attacker_reaction, Player.Defence? defender_reaction) SeeIfAttackHits(Vector2 shot,Player Defender)
        {
            Player.Defence? defender_reaction;
            Player.Attack attacker_reaction;
            if(Defender.CheckIfShotHits(shot))
            {
                defender_reaction = Player.Defence.HitShip;
                attacker_reaction = Player.Attack.Hit;
            }
            else
            {
                defender_reaction = null;
                attacker_reaction = Player.Attack.Miss;
            }

            return ( attacker_reaction, defender_reaction); 
        }
        
        public static void ApplyHitToPlayers(Vector2 shot,ref Player Attacker,  Player.Attack attacker_reaction,ref Player Defender, Player.Defence? defender_reaction)
        {
            Attacker.UpdateAttackScreen(shot, attacker_reaction);
            Defender.UpdateDefenceScreen(shot, defender_reaction);
        }
    }
    public class ShotFeedback
    {
        public bool IsValidShot { get; private set; }
        public Vector2 ShotCoordinates {  get; private set; }
        public Player.Defence? Defender_Reaction { get; private set; }
        public Player.Attack? Attacker_Reaction { get; private set; }


        public ShotFeedback(bool isValid, Vector2 shotCoordinates,Player.Defence? defence_reaction,Player.Attack? attack_reaction)
        {
            IsValidShot = isValid;
            ShotCoordinates = shotCoordinates;
            Defender_Reaction = defence_reaction;
            Attacker_Reaction = attack_reaction;



        }
    }
}
