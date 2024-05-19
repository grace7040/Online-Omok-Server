using Hive_Auth_Server.DTOs;
using Hive_Auth_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hive_Auth_Server.Controllers
{
    [ApiController]
    public class UserAuthController : Controller
    {
        ICheckAuthService _checkAuthService;

        public UserAuthController(IMemoryDb memoryDb, ICheckAuthService checkAuthService) {
            _checkAuthService = checkAuthService;
        }


        //Get HttpPost Request from GameAPIServer(LoginController)
        [HttpPost("checkuserauth")]
        public async Task<IActionResult> CheckUserAuth(ReqUserAuthDTO auth)
        {
            bool isAuthedUser = await _checkAuthService.CheckAuthToMemoryDbAsync(auth.Id, auth.Token);

            if(!isAuthedUser)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
