using GameAPIServer.DTOs;
using Microsoft.AspNetCore.Mvc;
using HiveAuthServer;
using GameAPIServer.Services;


namespace GameAPIServer.Controllers;

[ApiController]
public class LoginController : Controller
{
    IMemoryDb _memoryDb;
    IGameDb _gameDb;
    ICheckAuthService _checkAuthService;

    public LoginController(IMemoryDb memoryDb, IGameDb gameDb, ICheckAuthService checkAuthService)
    {
        _memoryDb = memoryDb;
        _gameDb = gameDb;
        _checkAuthService = checkAuthService;
    }

    [HttpPost("login")]
    public async Task<ResponseDTO> Login(ReqUserAuthDTO request)
    {
        var isAuthedOnHive = await _checkAuthService.CheckUserAuthToHiveAsync(request.Id, request.Token);
        if (!isAuthedOnHive)
        {
            return new ResponseDTO { Result = ErrorCode.LoginFailOnHive };
        }

        //redis에 로그인용 토큰 등록
        var redisResult = await _memoryDb.RegistUserAuthAsync(request.Id, request.Token, Expiries.LoginToken);
        if (redisResult != ErrorCode.None)
        {
            return new ResponseDTO { Result = ErrorCode.LoginFailRegistRedis };
        }

        //첫 접속인 경우, db에 game data 추가
        var isNewUser = !(await _gameDb.IsUserIdExistAsync(request.Id));
        if (isNewUser)
        {
            ErrorCode dbResult = await _gameDb.InsertAccountAsync(request.Id);
            if(dbResult != ErrorCode.None)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailInsertDB };
            }
        }

        return new ResponseDTO { Result = ErrorCode.None };
    }

    [HttpPost("logout")]
    public async Task<ResponseDTO> Logout(RequestDTO request)
    {
        var result = await _memoryDb.RemoveUserAuthAsync(request.Id);
        return new ResponseDTO { Result = result };
    }
}
