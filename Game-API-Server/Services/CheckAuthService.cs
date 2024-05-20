using System.Net;
using ZLogger;

namespace GameAPIServer.Services
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

        public async Task<bool> CheckUserAuthToHiveAsync(string id, string token)
        {
            var hiveUrl = _configuration.GetConnectionString("HiveServer") + "/checkuserauth";
            var client = new HttpClient();
            var hiveResponse = await client.PostAsJsonAsync(hiveUrl, new { Id = id, Token = token });
            if (hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> CheckUserAuthToMemoryDbAsync(string id, string token)
        {
            var redisResult = await _memoryDb.CheckUserAuthAsync(id, token);
            if (redisResult != ErrorCode.None)
            {
                _logger.ZLogInformation($"[CheckAuthToRedis Failed] {redisResult}, request.Id: {id}");
                return false;
            }

            return true;
        }
    }
}
