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
    IMatchWorker _matchWorker;

    public MatchingController(IMatchWorker matchWorker)
    {
        _matchWorker = matchWorker;
    }

    [HttpPost("matching")]
    public ResMatchingDTO Matching(ReqMatchingDTO request)
    {
        var response = new ResMatchingDTO() { Result = ErrorCode.MatchingWait };

        _matchWorker.AddUser(request.UserID);

        var matchingResult = _matchWorker.GetMatchingData(request.UserID);
        if (matchingResult.Item1)
        {
            response.Result = ErrorCode.None;
            response.OmokServerIP = matchingResult.Item2.OmokServerIP;
            response.OmokServerPort = matchingResult.Item2.OmokServerPort;
            response.RoomNumber = matchingResult.Item2.RoomNumber;

            _matchWorker.DeleteMatchingData(request.UserID);
        }

        return response;
    }
}
