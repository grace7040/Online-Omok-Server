namespace Hive_Auth_Server.Servicies
{
    public interface IHasher
    {
        public string GetHashedString(string str);
    }
}
