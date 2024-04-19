using Hive_Auth_Server.DTOs;
using Hive_Auth_Server.Servicies;
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
        IHasher _hasher;

        public CreateAccountController(IConfiguration configuration, IHiveDb hiveDb, IHasher hasher)
        {
            _configuration = configuration;
            _hiveDb = hiveDb;
            _hasher = hasher;
        }

        [HttpPost("createaccount")]
        public async Task<ResponseDTO> Create(ReqAccountDTO account)
        {
            var hashedPassword = _hasher.GetHashedString(account.Password);
            
            //DB에 추가
            ErrorCode result = await _hiveDb.InsertAccountAsync(account.Email, hashedPassword);

            return new ResponseDTO() { Result = result };
        }
    }
}
