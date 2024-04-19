using Game_API_Server.DTOs;

namespace Game_API_Server.Services
{
    public interface ICheckAuthService
    {
        public Task<bool> CheckAuthToHiveAsync(string email, string token);
        public Task<bool> CheckAuthToMemoryDbAsync(string email, string token);
    }
}
