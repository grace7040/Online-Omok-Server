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
        string _userRoomKey;
        public RedisDb(string redisConnectString, string userRoomKey)
        {
            var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);
            _userRoomKey = userRoomKey;
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

        public async Task<ErrorCode> RemoveUserRoomNumber(string id, int roomNumber)
        {
            var redisKey = id + _userRoomKey;
            var query = new RedisString<string>(_redisConnection, redisKey, null);
            var result = await query.DeleteAsync();
            if (result == false)
            {
                return ErrorCode.RemoveUserRoomNumberFail;
            }
            return ErrorCode.None;
        }

    }
}
