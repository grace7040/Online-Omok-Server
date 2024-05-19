using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZLogger;
using Matching_Server.DTOs;
using Microsoft.Extensions.FileSystemGlobbing;


namespace Matching_Server.Controllers;

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

        var matchingResult = _matchWorker.GetMatchingData(request.UserID);
        var isMatchSucceed = matchingResult.Item1;
        if (isMatchSucceed)
        {
            _matchWorker.RemoveUserFromMatchingDict(request.UserID, out var isSucceed);
            if (!isSucceed)
            {
                response.Result = ErrorCode.MatchingFailRemoveOnMatchingDict;
                return response;
            }
            response.Result = ErrorCode.None;
            response.OmokServerIP = matchingResult.Item2.OmokServerIP;
            response.OmokServerPort = matchingResult.Item2.OmokServerPort;
            response.RoomNumber = matchingResult.Item2.RoomNumber;
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
