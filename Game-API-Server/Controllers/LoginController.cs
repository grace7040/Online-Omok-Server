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
            bool isAuthedOnHive = await _checkAuthService.CheckAuthToHiveAsync(auth.Email, auth.Token);

            if (!isAuthedOnHive)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailOnHive };
            }


            ErrorCode redisResult = await _memoryDb.RegistUserAsync(auth.Email, auth.Token, Expiries.LoginToken);
            if (redisResult != ErrorCode.None)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailRegistRedis };
            }


            //첫 접속인 경우, db에 game data 추가
            if (!(await _gameDb.IsUserEmailExistAsync(auth.Email)))
            {
                ErrorCode dbResult = await _gameDb.InsertAccountAsync(auth.Email);
                if(dbResult != ErrorCode.None)
                {
                    return new ResponseDTO { Result = ErrorCode.LoginFailInsertDB };
                }
            }

            return new ResponseDTO { Result = ErrorCode.None };
        }

        [Route("Test")]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}
