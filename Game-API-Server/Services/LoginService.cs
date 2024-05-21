using HiveAuthServer;

namespace GameAPIServer.Services;

public class LoginService : ILoginService
{
    private IMemoryDb _memoryDb;
    private IGameDb _gameDb;

    public LoginService(IMemoryDb memoryDb, IGameDb gameDb)
    {
        _memoryDb = memoryDb;
        _gameDb = gameDb;
    }
    public async Task<ErrorCode> Login(string id, string token)
    {
        var redisResult = await _memoryDb.RegistUserAuthAsync(id, token, Expiries.LoginToken);
        if(redisResult != ErrorCode.None)
        {
            return ErrorCode.LoginFailRegistRedis;
        }

        var isNewUser = !(await _gameDb.IsUserIdExistAsync(id));
        if (isNewUser)
        {
            var dbResult = await _gameDb.InsertAccountAsync(id);
            if(dbResult != ErrorCode.None)
            {
                return ErrorCode.LoginFailInsertDB;
            }
        }

        return ErrorCode.None;
    }


    public async Task<ErrorCode> Logout(string id)
    {
        var result = await _memoryDb.RemoveUserAuthAsync(id);
        return result;
    }
}
