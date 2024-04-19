using Hive_Auth_Server.DTOs;
using Hive_Auth_Server.Servicies;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class CreateAccountController : Controller
    {
        ILogger<CreateAccountController> _logger;
        IHiveDb _hiveDb;
        IHasher _hasher;

        public CreateAccountController(ILogger<CreateAccountController> logger, IHiveDb hiveDb, IHasher hasher)
        {
            _logger = logger;
            _hiveDb = hiveDb;
            _hasher = hasher;
        }

        [HttpPost("createaccount")]
        public async Task<ResponseDTO> CreateAccount(ReqAccountDTO account)
        {
            var hashedPassword = _hasher.GetHashedString(account.Password);
            
            //DB에 추가
            ErrorCode result = await _hiveDb.InsertAccountAsync(account.Email, hashedPassword);
            
            if(result != ErrorCode.None)
            {
                _logger.ZLogInformation($"[CreateAccount Failed] {result}, request.email: {account.Email}");
            }
            else
            {
                _logger.ZLogInformation($"[CreateAccount Succeed] request.email: {account.Email}");
            }

            return new ResponseDTO() { Result = result };
        }
    }
}
