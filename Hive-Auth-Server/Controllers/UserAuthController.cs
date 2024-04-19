using Hive_Auth_Server.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class UserAuthController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;

        public UserAuthController(IConfiguration configuration, IMemoryDb memoryDb) {
            _configuration = configuration;
            _memoryDb = memoryDb;
        }


        //GameAPIServer의 LoginController가 참조함
        [HttpPost("checkuserauth")]
        public async Task<IActionResult> CheckUserAuth(ReqUserAuthDTO auth)
        {
            ErrorCode result = await _memoryDb.CheckUserAuthAsync(auth.Email, auth.Token);

            if(result != ErrorCode.None)
            {
                // :: TODO :: 로깅 추가
                return BadRequest();
            }
            return Ok();
        }
    }
}
