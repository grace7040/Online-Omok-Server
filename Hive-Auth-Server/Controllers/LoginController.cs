using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;
using System.Security.Cryptography;
using System.Text;
using System;
using Hive_Auth_Server.DTOs;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;
        
        public LoginController(IConfiguration configuration, IMemoryDb memoryDb)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;

            var dbConnectString = _configuration.GetConnectionString("HiveDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }


       
        /* :: TODO :: 내부의 기능들 서비스 단위로 분리하기 */
        [HttpPost("login")]
        public async Task<ResponseDTO> Login(ReqAccountDTO account)
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
                return new ResponseDTO { Result = ErrorCode.LoginFailWrongPassword };
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
            ErrorCode result = await _memoryDb.RegistUserAsync(account.Email, token, TimeSpan.FromHours(expiry));
            if(result != ErrorCode.None)
            {
                return new ResponseDTO { Result = result };
            }

            return new ResUserAuthDTO { Result = ErrorCode.None, Token = token };
        }
    }
}
