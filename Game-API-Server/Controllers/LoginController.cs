using Game_API_Server.DTOs;
using Microsoft.AspNetCore.Mvc;
using Hive_Auth_Server;
using Game_API_Server.Services;


namespace Game_API_Server.Controllers
{
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
        public async Task<ResponseDTO> Login(ReqUserAuthDTO auth)
        {
            var isAuthedOnHive = await _checkAuthService.CheckAuthToHiveAsync(auth.Id, auth.Token);
            if (!isAuthedOnHive)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailOnHive };
            }

            //redis에 로그인용 토큰 등록
            var redisResult = await _memoryDb.RegistUserAsync(auth.Id, auth.Token, Expiries.LoginToken);
            if (redisResult != ErrorCode.None)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailRegistRedis };
            }

            //첫 접속인 경우, db에 game data 추가
            var isNewUser = !(await _gameDb.IsUserIdExistAsync(auth.Id));
            if (isNewUser)
            {
                ErrorCode dbResult = await _gameDb.InsertAccountAsync(auth.Id);
                if(dbResult != ErrorCode.None)
                {
                    return new ResponseDTO { Result = ErrorCode.LoginFailInsertDB };
                }
            }

            return new ResponseDTO { Result = ErrorCode.None };
        }
    }
}
