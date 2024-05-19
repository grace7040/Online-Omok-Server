using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Hive_Auth_Server.Servicies
{
    public class HasherSHA256 : IHasher
    {
        public string GetHashedString(string str)
        {
            var hashedValues = SHA256.HashData(Encoding.UTF8.GetBytes(str));

            var tmp = new StringBuilder();
            for (int i = 0; i < hashedValues.Length; i++)
            {
                tmp.Append(hashedValues[i].ToString("x2"));
            }
            var result = tmp.ToString();

            return result;
        }
    }
}
