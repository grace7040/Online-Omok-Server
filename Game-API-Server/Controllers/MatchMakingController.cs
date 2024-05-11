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
        string _omokServerIP;
        string _omokServerPort;

        public MatchMakingController(IConfiguration configuration, IMemoryDb memoryDb, IMatchMakingService matchMakingService)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _matchMakingService = matchMakingService;

            Init();
        }

        void Init()
        {
            _omokServerIP = _configuration.GetConnectionString("OmokServerIP");
            _omokServerPort = _configuration.GetConnectionString("OmokServerPort");
        }

        [HttpPost("matching")]
        public async Task<ResponseDTO> Matching(ReqMatchingDTO request)
        {
            var email = request.Email;
            var roomNumber = await _matchMakingService.TryGetUserRoomNumber(email);
            
            //redis에 유저정보:룸넘버 키페어가 존재하는 경우
            if(roomNumber != -1)
            {
                //매칭 성공 시ErrorCode.None + 소켓서버 주소 + 룸 넘버 응답
                return new ResMatchingDTO
                {
                    Result = ErrorCode.None,
                    OmokServerIP = _omokServerIP,
                    OmokServerPort = _omokServerPort,
                    RoomNumber = roomNumber
                };
            }


            //매칭 시작
            var matchStartResult = await _matchMakingService.StartUserMatching(email);
            if (matchStartResult != ErrorCode.None)
            {
                return new ResponseDTO { Result = matchStartResult };
            }

            //매칭 상태 업데이트
            _matchMakingService.UpdateMatchingState();

            return new ResponseDTO { Result = ErrorCode.MatchingWait };
        }
    }
}
