namespace OmokServer
{
    public class User
    {
        bool _isUsing = false;
        bool _isLogin = false;
        string _sessionID = "";
        string _userID = "";
        int _heartBeatCnt = 0;

        public string ID { get { return _userID; } }
        public string SessionID { get { return _sessionID; } }
        public int RoomNumber { get; private set; } = -1;

        public bool IsUsing { get { return _isUsing; } }
        public bool IsLogin { get { return _isLogin; } }
        public bool IsInRoom { get { return RoomNumber != Room.InvalidRoomNumber; } }   //룸 입장시 RoomNumber 설정. 룸퇴장시  RoomNumber을 -1로 설정
        
        public int HeartBeatCnt { get { return _heartBeatCnt; } set { _heartBeatCnt = value; } }
        public UserGameData GameData { get; private set; }  //유저의 게임 정보

        public void SetGameData(UserGameData userGameData)
        {
            GameData = userGameData;
        }

        public void Use(string sessionID)
        {
            _sessionID = sessionID;
            _isUsing = true;
            _isLogin = false;
            _heartBeatCnt = 0;
        }

        public void Free()
        {
            _isUsing = false;
            _heartBeatCnt = 0;
        }

        public void Login(string userID)
        {
            _isLogin = true;
            _userID = userID;
        }

        public void Logout()
        {
            _isLogin = false;
            LeaveRoom();
        }

        public void EnterRoom(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public void LeaveRoom()
        {
            RoomNumber = -1;
        }
        
        public void Win()
        {
            GameData.Win_Count++;
            UpdateExp(10);
        }

        public void Lose()
        {
            GameData.Lose_Count++;
            UpdateExp(5);
        }

        void UpdateExp(int exp)
        {
            GameData.Exp += exp;
            if (GameData.Exp >= 100)
            {
                GameData.Level++;
                GameData.Exp = 0;
            }
        }
    }
}
