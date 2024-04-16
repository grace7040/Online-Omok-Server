using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using System.Security.Cryptography;
using System.Text;
using System;
using StackExchange.Redis;

namespace Hive_Auth_Server.Controllers
{
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;
        ConnectionMultiplexer _redisConnection;
        IDatabase _redisDb;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;

            var dbConnectString = _configuration.GetConnectionString("HiveDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);

            var redisConnectString = _configuration.GetConnectionString("HiveRedis");
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectString);
            _redisDb = _redisConnection.GetDatabase();
        }


        /* :: TODO :: id 전달해서, 유저가 id+토큰으로 요청하고, 겜서버가 id 기준으로 redis 뒤지도록 할까? */
        /* :: TODO :: 내부의 기능들 서비스 단위로 분리하기 */
        [HttpPost("login")]
        public async Task<UserAuthDTO> Login(AccountDTO account)
        {
            //account 정보 비교 with DB
            var password = (await _queryFactory.Query("user_account_data")
                                         .Select("password").Where("email", account.Email)
                                         .GetAsync<string>()).FirstOrDefault();


            /* 서비스로 통일해야 할 임시코드 */
            string hashedPassword;
            byte[] hashedValue = SHA256.HashData(Encoding.UTF8.GetBytes(account.Password)); 

            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < hashedValue.Length; i++)
            {
                tmp.Append(hashedValue[i].ToString("x2"));  
            }
            hashedPassword = tmp.ToString();


            if(password != hashedPassword)
            {
                /* :: TODO :: 로그인 실패. 반환 */
                return new UserAuthDTO();
            }

            //Token 랜덤 생성.   :: 알고리즘은 추후 수정 ::
            const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            string token = new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());


            //Redis에 저장
            int expiry = 1;
            _redisDb.StringSet(account.Email, token, TimeSpan.FromHours(expiry));
            //var tmpredis = _redisDb.StringGet(account.Email);

            //유저에게 Token 전달
            UserAuthDTO response = new UserAuthDTO();
            response.Email = account.Email;
            response.Token = token;

            // (?) Token만 주면 되는데, 이런식으로 DTO를 주는 게 좋을까? 아님 string만? 아님 따로 ResponseDTO 형태 만들어서?
            return response;
        }
    }
}
