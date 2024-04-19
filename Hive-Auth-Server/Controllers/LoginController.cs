using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Hive_Auth_Server.DTOs;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        IHiveDb _hiveDb;
        
        public LoginController(IConfiguration configuration, IMemoryDb memoryDb, IHiveDb hiveDb)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _hiveDb = hiveDb;
            
        }


       
        /* :: TODO :: 내부의 기능들 서비스 단위로 분리하기 */
        [HttpPost("login")]
        public async Task<ResponseDTO> Login(ReqAccountDTO account)
        {
            /* 서비스로 통일해야 할 임시코드 */
            string hashedPassword;
            byte[] hashedValue = SHA256.HashData(Encoding.UTF8.GetBytes(account.Password)); 

            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < hashedValue.Length; i++)
            {
                tmp.Append(hashedValue[i].ToString("x2"));  
            }
            hashedPassword = tmp.ToString();

            //pw비교 with db
            var password = _hiveDb.GetPasswordByEmailAsync(account.Email);

            if (await password != hashedPassword)
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
            ErrorCode result = await _memoryDb.RegistUserAsync(account.Email, token, Expiries.LoginToken);

            if(result != ErrorCode.None)
            {
                return new ResponseDTO { Result = result };
            }

            return new ResUserAuthDTO { Result = ErrorCode.None, Token = token };
        }
    }
}
