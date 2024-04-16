using Hive_Auth_Server.DTO;
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
    /* :: TODO :: db다루는 클래스 분리 & 에러 다루는 클래스 및 에러코드 구현*/
    [Route("createaccount")]
    public class CreateAccountController : Controller
    {
        QueryFactory _queryFactory;
        IConfiguration _configuration;
        MySqlConnection _dbConnection;


        public CreateAccountController(IConfiguration configuration)
        {
            _configuration = configuration;

            var DbConnectString = _configuration.GetConnectionString("HiveDB");
            _dbConnection = new MySqlConnection(DbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }

        /* :: TODO :: 비동기로 구현 */
        /* :: TODO :: 내부 기능들 서비스 단위로 분리하기 - 에러처리, 해싱, DB작업 */
        [HttpPost("create")]
        public async Task<ResponseDTO> Create(AccountDTO account)
        {
            //데이터 validation
            if (!ModelState.IsValid)
            {
                /* :: TODO :: 에러처리. 어떤 방식으로 하는 게? Error용 DTO 만들기? or.. */
            }

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
            var count = await _queryFactory.Query("user_account_data")
                                  .InsertAsync(new { email = account.Email, password = hashedPassword });

            //DB 추가 실패 처리
            if (count != 1)
            {
                /* :: TODO :: 실패 처리 */
            }

            return new ResponseDTO();
        }
    }
}
