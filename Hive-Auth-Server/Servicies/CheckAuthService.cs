using ZLogger;

namespace HiveAuthServer.Services
{
    public class CheckAuthService : ICheckAuthService
    {
        ILogger<CheckAuthService> _logger;
        IMemoryDb _memoryDb;

        public CheckAuthService(ILogger<CheckAuthService> logger, IMemoryDb memoryDb)
        {
            _logger = logger;
            _memoryDb = memoryDb;
        }

        public async Task<bool> CheckAuthToMemoryDbAsync(string email, string token)
        {
            var redisResult = await _memoryDb.CheckUserAuthAsync(email, token);
            if (redisResult != ErrorCode.None)
            {
                _logger.ZLogError($"[CheckAuthToRedis Fail] {redisResult}, request.email: {email}");
                return false;
            }

            return true;
        }
    }
}
