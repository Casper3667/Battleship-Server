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
            var JWT = new JWT("Username",DateTime.Now,DateTime.Now.AddDays(2));
            ValidateJwt(jwt,key);
            return (true, JWT);
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
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
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
                tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                var t = new JWT(validatedToken.Id, validatedToken.ValidFrom, validatedToken.ValidTo); // Added By Sofie
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
