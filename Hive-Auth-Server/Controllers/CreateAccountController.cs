﻿using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using static System.Net.WebRequestMethods;
using System.Text;
using SqlKata;
using SqlKata.Execution;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Humanizer.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Html;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class CreateAccountController : Controller
    {
        IConfiguration _configuration;
        QueryFactory _queryFactory;
        MySqlConnection _dbConnection;


        public CreateAccountController(IConfiguration configuration)
        {
            _configuration = configuration;

            var dbConnectString = _configuration.GetConnectionString("HiveDB");
            _dbConnection = new MySqlConnection(dbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }

        /* :: TODO :: 비동기로 구현 */
        /* :: TODO :: 내부 기능들 서비스 단위로 분리하기 - 에러처리, 해싱, DB작업 */
        [HttpPost("createaccount")]
        public async Task<ResponseDTO> Create(ReqAccountDTO account)
        {
            //pw암호화(해싱); logincontroller에서 재사용^^ 분리 ㄱ
            string hashedPassword;
            byte[] hashedValue = SHA256.HashData(Encoding.UTF8.GetBytes(account.Password)); //8비트로 인코딩. 너무 큰 값은

            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < hashedValue.Length; i++)
            {
                tmp.Append(hashedValue[i].ToString("x2"));  //16진법으로 변환 후
            }
            hashedPassword = tmp.ToString();

            //DB에 추가
            /* :: TODO :: DB에 이미 존재하는지 확인*/
            var count = await _queryFactory.Query("user_account_data")
                                  .InsertAsync(new { email = account.Email, password = hashedPassword });

            //DB 추가 실패 처리
            if (count != 1)
            {
                /* :: TODO :: 실패 처리 */
            }

            return new ResponseDTO() { Result = ErrorCode.None };
        }
    }
}
