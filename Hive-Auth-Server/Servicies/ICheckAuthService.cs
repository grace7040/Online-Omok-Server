namespace Hive_Auth_Server.Services
{
    public interface ICheckAuthService
    {
        public Task<bool> CheckAuthToMemoryDbAsync(string email, string token);
    }
}
