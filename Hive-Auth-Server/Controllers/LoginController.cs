using Microsoft.AspNetCore.Mvc;
using HiveAuthServer.DTOs;
using HiveAuthServer.Servicies;
using ZLogger;

namespace HiveAuthServer.Controllers;

[ApiController]
public class LoginController : Controller
{
    ILogger<LoginController> _logger;
    ILoginService _loginService;
    
    public LoginController(ILogger<LoginController> logger, ILoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
    }


    [HttpPost("login")]
    public async Task<ResponseDTO> Login(ReqAccountDTO account)
    {
        var (result, token) = await _loginService.Login(account.Id, account.Password);

        if(result != ErrorCode.None)
        {
            _logger.ZLogError($"[Login Failed] {result}, request.Id: {account.Id}");
            return new ResponseDTO { Result = result };
        }

        _logger.ZLogInformation($"[Login Succeed] request.Id: {account.Id}");
        return new ResUserAuthDTO { Result = ErrorCode.None, Token = token };
    }
}
