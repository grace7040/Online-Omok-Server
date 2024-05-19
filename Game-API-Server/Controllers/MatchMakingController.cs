using Game_API_Server.DTOs;
using Microsoft.AspNetCore.Mvc;
using Game_API_Server.Services;


namespace Game_API_Server.Controllers
{
    [ApiController]
    public class MatchMakingController : Controller
    {
        IConfiguration _configuration;
        IMemoryDb _memoryDb;
        IMatchMakingService _matchMakingService;


        public MatchMakingController(IConfiguration configuration, IMemoryDb memoryDb, IMatchMakingService matchMakingService)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _matchMakingService = matchMakingService;
        }

        [HttpPost("matching")]
        public async Task<ResponseDTO> Matching(ReqMatchingDTO request)
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
        public async Task<ResponseDTO> CancelMatching(ReqMatchingDTO request)
        {
            var Id = request.Id;
            var result = await _matchMakingService.RequestCancelMatching(Id);
            return result;
        }
    }
}
