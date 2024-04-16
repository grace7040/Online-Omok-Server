using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Security.Principal;

namespace Hive_Auth_Server.Controllers
{
    public class UserAuthController : Controller
    {
        IConfiguration _configuration;
        ConnectionMultiplexer _redisConnection;
        IDatabase _redisDb;
        public UserAuthController(IConfiguration configuration) {
            _configuration = configuration;

            var redisConnectString = _configuration.GetConnectionString("HiveRedis");
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectString);
            _redisDb = _redisConnection.GetDatabase();
        }


        // (??) 반환형을 어떻게 해야 하는지???? 타이머 설정해야 하니까 숫자로? 아님 DTO 만들어서??
        [HttpPost("checkauth")]
        public async Task<IActionResult> CheckAuth(UserAuthDTO auth)
        {
            //게임서버가 보낸 auth와 redis서버의 email-token값이 같은지 체크
            var token = _redisDb.StringGet(auth.Email);
            if (auth.Token == token)
            {
                return Ok();
            }


            // (??) 실패시 이런거 반환하는거 맞나
            return BadRequest();
        }
    }
}
