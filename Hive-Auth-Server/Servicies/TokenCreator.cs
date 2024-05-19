using System.Security.Cryptography;

namespace Hive_Auth_Server.Servicies
{
    public class TokenCreator : ITokenCreator
    {
        const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
        public string CreateAuthToken()
        {
            var bytes = new byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            var token = new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());

            return token;
        }
    }
}
