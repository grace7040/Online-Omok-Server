using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SqlKata.Execution;

namespace Hive_Auth_Server.Controllers
{

    [Route("login")]
    public class LoginController : Controller
    {
        QueryFactory _queryFactory;
        IConfiguration _configuration;
        MySqlConnection _dbConnection;


        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;

            var DbConnectString = _configuration.GetSection("DBConnection")["HiveDB"];
            _dbConnection = new MySqlConnection(DbConnectString);
            _dbConnection.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            _queryFactory = new QueryFactory(_dbConnection, compiler);
        }


        [HttpPost("login")]
        public async Task<ResponseDTO> Login(AccountDTO account)
        {
            //account 정보 비교 with DB (+ id 갖고오기)

            //Token 생성

            //Redis에 저장

            //유저에게 Token 및 id 전달

            return new ResponseDTO();
        }
    }
}
