using CloudStructures;
using CloudStructures.Structures;

namespace GameAPIServer.Repositories;
public class RedisDb : IMemoryDb
{
    IConfiguration _configuration;
    RedisConnection _redisConnection;
    string _roomNumberKey;
    string _userRoomKey;
    string _matchingQueueKey;
    public RedisDb(IConfiguration configuration) {
        _configuration = configuration;

        var redisConnectString = _configuration.GetConnectionString("GameRedis");
        var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
        _redisConnection = new RedisConnection(redisConfig);
        _userRoomKey = _configuration.GetSection("RedisKeys")["UserRoom"];
        _matchingQueueKey = _configuration.GetSection("RedisKeys")["MatchMaking"];
        _roomNumberKey = _configuration.GetSection("RedisKeys")["RoomNumber"];
    }

    public async Task<ErrorCode> RegistUserAuthAsync(string id, string authToken, TimeSpan expiry)
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

    public async Task<ErrorCode> RemoveUserAuthAsync(string id)
    {
        try
        {
            var query = new RedisString<string>(_redisConnection, id, null);
            await query.DeleteAsync();
        }
        catch
        {
            return ErrorCode.RemoveUserFailException;
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

    public async Task<int> TryGetUserRoomNumberAsync(string id)
    {
        try
        {
            var redisKey = id + _userRoomKey;
            var query = new RedisString<string>(_redisConnection, redisKey, null);
            var userRoom = await query.GetAsync();

            if (userRoom.HasValue)
            {
                var roomNumber = int.Parse(userRoom.Value);
                return roomNumber;
            }

        }
        catch
        {
            return -1;
        }

        return -1;
    }

    public async Task<bool> IsUserInMatchingQueue(string id)
    {
        var query = new RedisSortedSet<string>(_redisConnection, _matchingQueueKey, null);
        var result = await query.ScoreAsync(id);

        if(result.HasValue)
        {
            return true;
        }
        return false;
    }
    public async Task<ErrorCode> AddUserToMatchingQueueAsync(string id)
    {
        try
        {
            var query = new RedisSortedSet<string>(_redisConnection, _matchingQueueKey, null);
            await query.AddAsync(id, DateTime.Now.Ticks);
        }
        catch
        {
            return ErrorCode.AddUserToMatchingQueueFailException;
        }

        return ErrorCode.None;
    }

    public async Task<int> GetMatchingQueueSizeAsync()
    {
        var query = new RedisSortedSet<string>(_redisConnection, _matchingQueueKey, null);
        var result = await query.LengthAsync();

        return (int)result;
    }

    public async Task<(string, string)> GetMatchedUsers()
    {
        var query = new RedisSortedSet<string>(_redisConnection, _matchingQueueKey, null);
        var result = await query.RangeByRankWithScoresAsync(0, 1);

        if(result.Length != 2)
        {
            return (null, null);
        }

        return (result[0].Value, result[1].Value);
    }

    public async Task<int> GetRoomNumberAsync()
    {
        var query = new RedisString<int>(_redisConnection, _roomNumberKey, null);
        var result = await query.IncrementAsync(1);
        return (int)result;
    }

    public void AddUserRoomNumber(string id, int roomNumber)
    {
        var redisKey = id + _userRoomKey;
        var query = new RedisString<string>(_redisConnection, redisKey, null);
        query.SetAsync(roomNumber.ToString(), null);
    }

    public void RemoveUserFromMatchingQueue(string id)
    {
        var query = new RedisSortedSet<string>(_redisConnection, _matchingQueueKey, null);
        query.RemoveAsync(id);
    }

}
