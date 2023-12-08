using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GameServer.Client_Facing
{
    public static class TokenValidator
    {

        public static readonly string key = "Thesecretstomakeatokenkeyistodothis";
        // TODO: SOFIE Insert Proper Code for Verifying JWT
        public static (bool verified, JWT? TokenData) DecodeAndVerifyJWT(string jwt)
        {
            Console.WriteLine("Validating this Token: " + jwt);
            //var JWT = new JWT("Username",DateTime.Now,DateTime.Now.AddDays(2));
            (bool isValid,JWT? JWT)=ValidateJwt(jwt,key);
            return (isValid, JWT);
        }
        /// <summary>
        /// Code taken DIRECTLY from Game Lobby, with only MINOR edits from Sofie
        /// Thus All Credit Goes to Casper
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private static (bool valid,JWT? token)ValidateJwt(string jwtToken, string secretKey)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the JWT
                tokenHandler.ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);
                Console.WriteLine("Token Validation> Doing Code By Sofie");
                var temp = tokenHandler.ReadJwtToken(jwtToken);
                string name=temp.Claims.First().Value;
                Console.WriteLine($"Got Username Out of Token: [{name}]");
                 var t = new JWT(name, validatedToken.ValidFrom, validatedToken.ValidTo); // Added By Sofie
                return (true,t); // If the validation is successful
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (false,null); // If the validation fails
            }
        }

    }
    public class JWT
    {
        public string Username { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }   

        public JWT(string username,DateTime validFrom,DateTime validTo)
        {
            Username = username;
            ValidFrom = validFrom;  
            ValidTo = validTo;
        }
    }
}
