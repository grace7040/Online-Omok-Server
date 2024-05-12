using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class RedisDb
    {
        RedisConnection _redisConnection;
        public RedisDb(string redisConnectString)
        {
            var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);

            //-- for Debug --
            RegistUserAsync("jacking", "123qwe", TimeSpan.FromHours(1));
            RegistUserAsync("hello@naver.com", "123qwe", TimeSpan.FromHours(1));
            RegistUserAsync("crazy@naver.com", "123qwe", TimeSpan.FromHours(1));
            //-- --------- --
        }

        //-- for Debug --
        public async Task<ErrorCode> RegistUserAsync(string id, string authToken, TimeSpan expiry)
        {
            var query = new RedisString<string>(_redisConnection, id, expiry);
            await query.SetAsync(authToken, expiry);

            return ErrorCode.None;
        }
        //-- --------- --

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
