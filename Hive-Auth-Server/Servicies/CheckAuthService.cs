namespace Hive_Auth_Server.Services
{
    public class CheckAuthService : ICheckAuthService
    {
        IMemoryDb _memoryDb;

        public CheckAuthService(IMemoryDb memoryDb)
        {
            _memoryDb = memoryDb;
        }

        public async Task<bool> CheckAuthToMemoryDbAsync(string email, string token)
        {
            ErrorCode redisResult = await _memoryDb.CheckUserAuthAsync(email, token);
            if (redisResult != ErrorCode.None)
            {
                // :: TODO :: 로깅 추가
                return false;
            }

            return true;
        }
    }
}
