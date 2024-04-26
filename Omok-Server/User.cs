using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class User
    {
        bool _isLogin = false;
        string _sessionId = "";
        string _userID = "";

        public string ID { get { return _userID; } }
        public int RoomNumber { get; private set; } = -1;
        public bool IsLogin { get { return _isLogin; } }
        public bool IsInRoom { get { return RoomNumber != Room.InvalidRoomNumber; } }   //룸입장시 시 RoomNumber 설정. 룸퇴장시  RoomNumber을 -1로 설정
        

        public void Set(bool isLogin, string sessionID, string userID)
        {
            _isLogin = isLogin;
            _sessionId = sessionID;
            _userID = userID;
        }

        //public bool IsConfirm(string netSessionID)
        //{
        //    return _sessionId == netSessionID;
        //}

        public void EnteredRoom(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public void LeaveRoom()
        {
            RoomNumber = -1;
        }

        //public bool IsStateLogin() { return SequenceNumber != 0; }

        //public bool IsStateRoom() { return RoomNumber != -1; }
    }
}
