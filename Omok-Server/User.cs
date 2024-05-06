using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
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
        public bool IsInRoom { get { return RoomNumber != Room.InvalidRoomNumber; } }   //룸입장시 시 RoomNumber 설정. 룸퇴장시  RoomNumber을 -1로 설정
        
        public int HeartBeatCnt { get { return _heartBeatCnt; } set { _heartBeatCnt = value; } }


        //유저 게임 정보
        public int Level { get; set; }
        public int Exp { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int TotalGameCount { get { return WinCount + LoseCount; } }


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
        
    }
}
