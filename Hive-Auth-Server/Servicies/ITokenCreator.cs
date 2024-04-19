using System.Security.Cryptography;

namespace Hive_Auth_Server.Servicies
{
    public interface ITokenCreator
    {
        public string CreateAuthToken();

    }
}
