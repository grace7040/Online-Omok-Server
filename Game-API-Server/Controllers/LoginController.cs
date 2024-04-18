using Game_API_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Net;
using System.Security.Principal;
using CloudStructures;
using CloudStructures.Structures;
using static Humanizer.In;

namespace Game_API_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;
        RedisConnection _redisConnection;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;

            var dbConnectString = _configuration.GetConnectionString("GameDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);

            var redisConnectString = configuration.GetConnectionString("GameRedis");
            var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);
        }

         /* :: TODO :: 내부 기능들 서비스 단위로 분리하기 */
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserAuthDTO auth)
        {
            //auth와 Hive의 토큰이 같은지체크. (Hive로 HttpRequest 보내기)
            string email = auth.Email;
            string token = auth.Token;
            string hiveUrl = _configuration.GetConnectionString("HiveServer") + "/checkuserauth";
            HttpClient client = new();

            var hiveResponse = await client.PostAsJsonAsync(hiveUrl, auth);

            //로그인 실패
            if (hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                /* :: TODO :: 실패 처리 */
                return BadRequest();
            }


            //로그인 성공 - redis에 등록.
            /* :: TODO :: Hive 응답으로 타이머 시간도 받아와서 expiry 설정하기 */
            int expiry = 1; //임시
            var query = new RedisString<string>(_redisConnection, auth.Email, TimeSpan.FromHours(expiry));
            await query.SetAsync(auth.Token, TimeSpan.FromHours(expiry));
            //var tempredis = query.GetAsync().Result.Value;

            //접속한 적 있는 유저인지 확인 (유저 정보가 DB에 있는지 확인)
            var result = (await _queryFactory.Query("user_game_data")
                                         .Select("level").Where("email", auth.Email)
                                         .GetAsync<int>()).FirstOrDefault(); //firstordefault는 널 가능성 있을 때 사용

            //첫 접속인 경우, db에 등록. (db에 유저 정보가 없는 경우)
            if (result == 0)
            {
                var count = await _queryFactory.Query("user_game_data")
                                  .InsertAsync(new
                                  {
                                      email = auth.Email,
                                      level = 1,
                                      exp = 0,
                                      win_count = 0,
                                      lose_count = 0
                                  }); 

                //DB 추가 실패 처리
                if (count != 1)
                {
                    /* :: TODO :: 실패 처리 */
                }
            }

            // :: TODO :: DB에서 user 정보 가져와서 클라에게 보내주기

            return Ok();
        }

        [Route("Test")]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}
