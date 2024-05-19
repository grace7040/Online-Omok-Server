namespace Game_API_Server.Services
{
    public interface ICheckAuthService
    {
        public Task<bool> CheckUserAuthToHiveAsync(string id, string token);
        public Task<bool> CheckUserAuthToMemoryDbAsync(string id, string token);
    }
}
