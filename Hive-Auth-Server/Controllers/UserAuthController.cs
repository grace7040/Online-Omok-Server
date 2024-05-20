using HiveAuthServer.DTOs;
using HiveAuthServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace HiveAuthServer.Controllers
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
            var isAuthedUser = await _checkAuthService.CheckAuthToMemoryDbAsync(auth.Id, auth.Token);

            if(!isAuthedUser)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
