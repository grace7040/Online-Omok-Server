namespace Game_API_Server.Services
{
    public class SimpleMatchService : IMatchMakingService
    {
        IMemoryDb _memoryDb;
        public SimpleMatchService(IMemoryDb memoryDb)
        {
            _memoryDb = memoryDb;
        }
        public async Task<int> TryGetUserRoomNumber(string email)
        {
            //redis에 유저:룸넘버 키페어 존재하는지 확인

            var roomNumber = await _memoryDb.TryGetUserRoomNumberAsync(email);
            return roomNumber;
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
    }
}
