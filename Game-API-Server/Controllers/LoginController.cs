using GameAPIServer.DTOs;
using Microsoft.AspNetCore.Mvc;
using HiveAuthServer;
using GameAPIServer.Services;


namespace GameAPIServer.Controllers;

[ApiController]
public class LoginController : Controller
{
    ICheckAuthService _checkAuthService;
    ILoginService _loginService;

    public LoginController(ICheckAuthService checkAuthService, ILoginService loginService)
    {
        _checkAuthService = checkAuthService;
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<ResponseDTO> Login(ReqUserAuthDTO request)
    {
        var isAuthedOnHive = await _checkAuthService.CheckUserAuthToHiveAsync(request.Id, request.Token);
        if (!isAuthedOnHive)
        {
            return new ResponseDTO { Result = ErrorCode.LoginFailOnHive };
        }

        var result = await _loginService.Login(request.Id, request.Token);

        return new ResponseDTO { Result = result };
    }

    [HttpPost("logout")]
    public async Task<ResponseDTO> Logout(RequestDTO request)
    {
        var result = await _loginService.Logout(request.Id);

        return new ResponseDTO { Result = result };
    }
}
