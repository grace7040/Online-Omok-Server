﻿using CloudStructures;
using CloudStructures.Structures;

namespace OmokServer;

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

    public ErrorCode CheckUserAuth(string id, string authToken)
    {
        try
        {
            var query = new RedisString<string>(_redisConnection, id, null);
            var user = query.GetAsync().Result;

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

    public ErrorCode RemoveUserRoomNumber(string id, int roomNumber)
    {
        var redisKey = id + _userRoomKey;
        var query = new RedisString<string>(_redisConnection, redisKey, null);
        var result = query.DeleteAsync().Result;
        if (result == false)
        {
            return ErrorCode.RemoveUserRoomNumberFail;
        }
        return ErrorCode.None;
    }

}
