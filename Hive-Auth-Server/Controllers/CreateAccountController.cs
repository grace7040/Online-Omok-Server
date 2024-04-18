using Hive_Auth_Server.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class CreateAccountController : Controller
    {
        IConfiguration _configuration;
        IHiveDb _hiveDb;


        public CreateAccountController(IConfiguration configuration, IHiveDb hiveDb)
        {
            _configuration = configuration;
            _hiveDb = hiveDb;
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
            ErrorCode result = await _hiveDb.InsertAccountAsync(account.Email, hashedPassword);

            return new ResponseDTO() { Result = result };
        }
    }
}
