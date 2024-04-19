using System.Net;
using ZLogger;
using Microsoft.Extensions.Logging;

namespace Game_API_Server.Services
{
    public class CheckAuthService : ICheckAuthService
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        ILogger<CheckAuthService> _logger;

        public CheckAuthService(IConfiguration configuration, ILogger<CheckAuthService> logger, IMemoryDb memoryDb)
        {
            _configuration = configuration;
            _logger = logger;
            _memoryDb = memoryDb;
        }

        public async Task<bool> CheckAuthToHiveAsync(string email, string token)
        {
            string hiveUrl = _configuration.GetConnectionString("HiveServer") + "/checkuserauth";
            HttpClient client = new();
            var hiveResponse = await client.PostAsJsonAsync(hiveUrl, new { Email = email, Token = token });
            if (hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> CheckAuthToMemoryDbAsync(string email, string token)
        {
            ErrorCode redisResult = await _memoryDb.CheckUserAuthAsync(email, token);
            if (redisResult != ErrorCode.None)
            {
                _logger.ZLogInformation($"[CheckAuthToRedis Failed] {redisResult}, request.email: {email}");
                return false;
            }

            return true;
        }
    }
}
