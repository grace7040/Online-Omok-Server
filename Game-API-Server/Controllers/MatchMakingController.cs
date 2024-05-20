using GameAPIServer.DTOs;
using Microsoft.AspNetCore.Mvc;
using GameAPIServer.Services;


namespace GameAPIServer.Controllers;

[ApiController]
public class MatchMakingController : Controller
{
    IMatchMakingService _matchMakingService;


    public MatchMakingController(IMatchMakingService matchMakingService)
    {
        _matchMakingService = matchMakingService;
    }

    [HttpPost("matching")]
    public async Task<ResponseDTO> Matching(RequestDTO request)
    {
        var Id = request.Id;
        var userMatchingInfo = await _matchMakingService.TryGetUserMatchingInfo(Id);

        if(userMatchingInfo == null)
        {
            // ::TODO:: exception
            return new ResponseDTO { Result = ErrorCode.MatchingReqFailException };
        }

        if (userMatchingInfo.IsMatchSucceed)
        {
            return new ResMatchingDTO
            {
                Result = ErrorCode.None,
                OmokServerIP = userMatchingInfo.OmokServerIP,
                OmokServerPort = userMatchingInfo.OmokServerPort,
                RoomNumber = userMatchingInfo.RoomNumber
            };
        }

        return new ResponseDTO { Result = ErrorCode.MatchingWait };
    }

    [HttpPost("cancelmatching")]
    public async Task<ResponseDTO> CancelMatching(RequestDTO request)
    {
        var Id = request.Id;
        var result = await _matchMakingService.RequestCancelMatching(Id);
        return result;
    }
}
