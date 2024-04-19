using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Hive_Auth_Server.DTOs;
using Hive_Auth_Server.Servicies;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class LoginController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        IHiveDb _hiveDb;
        IHasher _hasher;
        
        public LoginController(IConfiguration configuration, IMemoryDb memoryDb, IHiveDb hiveDb, IHasher hasher)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _hiveDb = hiveDb;
            _hasher = hasher;
            
        }


        [HttpPost("login")]
        public async Task<ResponseDTO> Login(ReqAccountDTO account)
        {
            var passwordFromRequest = _hasher.GetHashedString(account.Password);
            var passwordFromDb = _hiveDb.GetPasswordByEmailAsync(account.Email);

            if (await passwordFromDb != passwordFromRequest)
            {
                return new ResponseDTO { Result = ErrorCode.LoginFailWrongPassword };
            }

            //Token 랜덤 생성 및 Redis에 저장
            string token = CreateAuthToken();
            ErrorCode result = await _memoryDb.RegistUserAsync(account.Email, token, Expiries.LoginToken);
            if(result != ErrorCode.None)
            {
                return new ResponseDTO { Result = result };
            }

            return new ResUserAuthDTO { Result = ErrorCode.None, Token = token };
        }

        string CreateAuthToken()
        {
            const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            string token = new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());

            return token;
        }
    }
}
