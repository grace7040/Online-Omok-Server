using System.Security.Cryptography;

namespace Hive_Auth_Server.Servicies
{
    public class TokenCreator : ITokenCreator
    {
        public string CreateAuthToken()
        {
            const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = new byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            string token = new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());

            return token;
        }
    }
}
