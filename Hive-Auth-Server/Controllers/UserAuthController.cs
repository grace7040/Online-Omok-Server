using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using System.Security.Principal;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Hive_Auth_Server.Controllers
{
    public class UserAuthController : Controller
    {
        IConfiguration _configuration;
        RedisConnection _redisConnection;
        public UserAuthController(IConfiguration configuration) {
            _configuration = configuration;

            var redisConnectString = configuration.GetConnectionString("HiveRedis");
            var redisConfig = new RedisConfig("HiveRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);
        }


         /* :: TODO :: 내부 기능들 서비스 단위로 분리하기. */
         /* :: TODO :: 반환 타입 변경; 타이머 시간도 포함하는 DTO 반환 */
        //GameAPIServer의 LoginController가 참조함
        [HttpPost("checkauth")]
        public IActionResult CheckAuth(UserAuthDTO auth)
        {
            var query = new RedisString<string>(_redisConnection, auth.Email, null);
            ////var result = await query.GetAsync();
            string token = query.GetAsync().Result.Value;

            //게임서버가 보낸 auth와 redis서버의 email-token값이 같은지 체크
            if (auth.Token == token)
            {
                return Ok();
            }


            // (??) 실패시 이런거 반환하는거 맞나
            return BadRequest();
        }
    }
}
