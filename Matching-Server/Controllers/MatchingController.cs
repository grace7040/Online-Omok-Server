using Microsoft.AspNetCore.Mvc;
using MatchingServer.DTOs;

namespace MatchingServer.Controllers;

[ApiController]
public class MatchingController : ControllerBase
{
    IMatchingWorker _matchWorker;

    public MatchingController(IMatchingWorker matchWorker)
    {
        _matchWorker = matchWorker;
    }

    [HttpPost("matching")]
    public ResMatchingDTO Matching(RequestDTO request)
    {
        var response = new ResMatchingDTO() { Result = ErrorCode.MatchingWait };

        _matchWorker.AddUser(request.UserID);

        var (isMatchingSucceed, data) = _matchWorker.GetMatchingData(request.UserID);
        if (isMatchingSucceed)
        {
            _matchWorker.RemoveUserFromMatchingDict(request.UserID, out var isSucceed);
            if (!isSucceed)
            {
                response.Result = ErrorCode.MatchingFailRemoveOnMatchingDict;
                return response;
            }
            response.Result = ErrorCode.None;
            response.OmokServerIP = data.OmokServerIP;
            response.OmokServerPort = data.OmokServerPort;
            response.RoomNumber = data.RoomNumber;
        }

        return response;
    }

    [HttpPost("cancelmatching")]
    public ResponseDTO CancelMatching(RequestDTO request)
    {
        var response = new ResponseDTO() { Result = ErrorCode.None };
        _matchWorker.RemoveUserFromMatchingDict(request.UserID, out var isSucceed);
        if (!isSucceed)
        {
            response.Result = ErrorCode.CancelMatchingFailRemoveOnMatchingDict;
        }
        
        return response;
    }
}
