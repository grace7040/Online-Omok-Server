using Game_API_Server.DTOs;

namespace Game_API_Server.Services
{
    //redis increment를 사용한 간단한 매칭 방법
    //매칭 요청 인원이 2명이 이상이 되면 redis에서 룸 넘버(1씩 증가)를 받아와 매칭을 처리한다.
    public class SimpleMatchService : IMatchMakingService
    {
        IMemoryDb _memoryDb;
        IConfiguration _configuration;
        string _omokServerIP;
        string _omokServerPort;
        public SimpleMatchService(IMemoryDb memoryDb, IConfiguration configuration)
        {
            _configuration = configuration;
            _memoryDb = memoryDb;
            _omokServerIP = _configuration.GetConnectionString("OmokServerIP");
            _omokServerPort = _configuration.GetConnectionString("OmokServerPort");
        }
        public async Task<ResMatchingDTO> TryGetUserMatchingInfo(string email)
        {
            //redis에 유저:룸넘버 키페어 존재하는지 확인
            var roomNumber = await _memoryDb.TryGetUserRoomNumberAsync(email);

            var userMatchingInfo = new ResMatchingDTO()
            {
                RoomNumber = roomNumber,
                OmokServerIP = _omokServerIP,
                OmokServerPort = _omokServerPort
            };

            return userMatchingInfo;
        }

        public async Task<ErrorCode> StartUserMatching(string email)
        {
            //매칭 큐에 유저가 존재하는지 확인
            var IsMatchingStarted = await _memoryDb.IsUserInMatchingQueue(email);
            if (!IsMatchingStarted)
            {
                //없다면, 매칭 큐에 유저 정보 추가
                var result = await _memoryDb.AddUserToMatchingQueueAsync(email);
                return result;
            }
            return ErrorCode.None;
        }


        public async void UpdateMatchingState()
        {
            //매칭 여부 체크
            var matchingQueCount = await _memoryDb.GetMatchingQueueSizeAsync();
            if (matchingQueCount >= 2)
            {
                MakeMatch();
            }

            return;
        }

        async void MakeMatch()
        {
            while(await _memoryDb.GetMatchingQueueSizeAsync() > 1)
            {
                //redis 매칭큐에서 두 명 빼와서 룸넘버 부여하고 유저정보:룸넘버 키페어 추가
                var users = await _memoryDb.GetMatchedUsers();
                if (users.Item1 == null || users.Item2 == null)
                {
                    return;
                }

                //유저:룸넘버 키페어 추가
                var roomNumber = await _memoryDb.GetRoomNumberAsync();
                _memoryDb.AddUserRoomNumber(users.Item1, roomNumber);
                _memoryDb.AddUserRoomNumber(users.Item2, roomNumber);

                //매칭 큐에서 제거
                _memoryDb.RemoveUserFromMatchingQueue(users.Item1);
                _memoryDb.RemoveUserFromMatchingQueue(users.Item2);
            }
        }

        public async Task<ResponseDTO> RequestCancelMatching(string email)
        {
            _memoryDb.RemoveUserFromMatchingQueue(email);
            return new ResponseDTO { Result = ErrorCode.None };
        }
    }
}
