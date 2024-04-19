using CloudStructures;
using CloudStructures.Structures;

namespace Hive_Auth_Server.Repositories
{
    
    public class RedisDb : IMemoryDb
    {
        IConfiguration _configuration;
        RedisConnection _redisConnection;
        public RedisDb(IConfiguration configuration) {
            _configuration = configuration;

            var redisConnectString = _configuration.GetConnectionString("GameRedis");
            var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);
        }
        public async Task<ErrorCode> RegistUserAsync(string id, string authToken, TimeSpan expiry)
        {
            try
            {
                var query = new RedisString<string>(_redisConnection, id, expiry);
                if(await query.SetAsync(authToken, expiry) == false)    
                {
                    return ErrorCode.LoginFailRegistRedis;
                }
            }
            catch
            {
                return ErrorCode.LoginFailRegistRedis;
            }
            
            return ErrorCode.None;
        }

        public async Task<ErrorCode> CheckUserAuthAsync(string id, string authToken)
        {
            try
            {
                var query = new RedisString<string>(_redisConnection, id, null);
                var user = await query.GetAsync();

                if (!user.HasValue)
                {
                    return ErrorCode.CheckUserAuthFailNotExist;
                }

                if (user.Value != authToken)
                {
                    return ErrorCode.CheckUserAuthFailNotMatch;
                }
            }
            catch
            {
                return ErrorCode.CheckUserAuthFailException;
            }

            return ErrorCode.None;  
        }
    }
}
