using HiveAuthServer.DTOs;
using HiveAuthServer.Servicies;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace HiveAuthServer.Controllers;

[ApiController]
public class CreateAccountController : Controller
{
    ILogger<CreateAccountController> _logger;
    ICreateAccountService _createAccountService;

    public CreateAccountController(ILogger<CreateAccountController> logger, ICreateAccountService createAccountService)
    {
        _logger = logger;
        _createAccountService = createAccountService;
    }

    [HttpPost("createaccount")]
    public async Task<ResponseDTO> CreateAccount(ReqAccountDTO account)
    {
        var result = await _createAccountService.CreateAccount(account.Id, account.Password);
        
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
