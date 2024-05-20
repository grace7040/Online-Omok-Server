using HiveAuthServer.DTOs;
using HiveAuthServer.Servicies;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace HiveAuthServer.Controllers
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
            
            //DB에 유저 계정 추가
            var result = await _hiveDb.InsertAccountAsync(account.Id, hashedPassword);
            
            if(result != ErrorCode.None)
            {
                _logger.ZLogError($"[CreateAccount Failed] {result}, request.Id: {account.Id}");
            }
            else
            {
                _logger.ZLogInformation($"[CreateAccount Succeed] request.Id: {account.Id}");
            }

            return new ResponseDTO() { Result = result };
        }
    }
}
