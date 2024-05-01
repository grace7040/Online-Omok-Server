using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class UserManager
    {
        int _maxUserCount;

        Dictionary<string, User> _userMap = new();

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;
       
        
        public void Init(int maxUserCount)
        {
            _maxUserCount = maxUserCount;
        }

        public void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        //로그인 시 유저 맵에 추가
        public ErrorCode AddUser(string userID, string sessionID)
        {
            if (IsFullUserCount())
            {
                return ErrorCode.LoginFailFullUserCount;
            }

            if (_userMap.ContainsKey(sessionID))
            {
                return ErrorCode.AddUserDuplication;
            }

            var user = new User();
            user.Set(true, sessionID, userID);
            _userMap.Add(sessionID, user);

            return ErrorCode.None;
        }

        public ErrorCode RemoveUser(string sessionID)
        {
            if (_userMap.Remove(sessionID) == false)
            {
                return ErrorCode.RemoveUserSearchFailUserId;
            }

            return ErrorCode.None;
        }

        public User GetUserBySessionId(string sessionID)
        {
            User user = null;
            _userMap.TryGetValue(sessionID, out user);
            return user;
        }

        bool IsFullUserCount()
        {
            return _maxUserCount <= _userMap.Count();
        }

        public void CheckLoginState(string sessionID, PKTReqLogin reqData)
        {
            //이미 로그인되어 있는 경우
            if (GetUserBySessionId(sessionID) != null)
            {
                ResponseLogin(ErrorCode.LoginFailAlreadyLogined, sessionID);
                return;
            }
            
            
            var errorCode = AddUser(reqData.UserID, sessionID);
            if (errorCode != ErrorCode.None)
            {
                ResponseLogin(errorCode, sessionID );

                if (errorCode == ErrorCode.LoginFailFullUserCount)
                {
                    NotifyMustCloseToClient(ErrorCode.LoginFailFullUserCount, sessionID);
                }

                return;
            }

            //로그인 성공
            ResponseLogin(errorCode, sessionID);

            MainServer.MainLogger.Debug($"로그인 결과. UserID:{reqData.UserID}, PW: {reqData.AuthToken}, ERROR: {errorCode}");
        }
        

        public void ResponseLogin(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKTResLogin()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.ResLogin);

            SendFunc(sessionID, sendData);
        }

        public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKNtfMustClose()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.NtfMustClose);

            SendFunc(sessionID, sendData);
        }
    }
}
