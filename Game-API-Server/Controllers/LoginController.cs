using Game_API_Server.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Hive_Auth_Server;


namespace Game_API_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        IGameDb _gameDb;

        public LoginController(IConfiguration configuration, IMemoryDb memoryDb, IGameDb gameDb)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _gameDb = gameDb;
        }

         /* :: TODO :: 내부 기능들 서비스 단위로 분리하기 */
        [HttpPost("login")]
        public async Task<ResponseDTO> Login(ReqUserAuthDTO auth)
        {
            //auth와 Hive의 토큰이 같은지체크. (Hive로 HttpRequest 보내기)
            string hiveUrl = _configuration.GetConnectionString("HiveServer") + "/checkuserauth";
            HttpClient client = new();
            var hiveResponse = await client.PostAsJsonAsync(hiveUrl, auth);

            //로그인 실패
            if (hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailOnHive };
            }


            //로그인 성공 - redis에 등록.
            ErrorCode redisResult = await _memoryDb.RegistUserAsync(auth.Email, auth.Token, Expiries.LoginToken);

            if (redisResult != ErrorCode.None)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailRegistRedis };
            }


            //첫 접속인 경우, db에 등록. (db에 유저 정보가 없는 경우)
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
