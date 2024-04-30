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

        Dictionary<string, User> _userMap = new Dictionary<string, User>();

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new PacketManager<MemoryPackBinaryPacketDataCreator>();

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
                return ErrorCode.LOGIN_FULL_USER_COUNT;
            }

            if (_userMap.ContainsKey(sessionID))
            {
                return ErrorCode.ADD_USER_DUPLICATION;
            }

            var user = new User();
            user.Set(true, sessionID, userID);
            _userMap.Add(sessionID, user);

            return ErrorCode.NONE;
        }

        public ErrorCode RemoveUser(string sessionID)
        {
            if (_userMap.Remove(sessionID) == false)
            {
                return ErrorCode.REMOVE_USER_SEARCH_FAILURE_USER_ID;
            }

            return ErrorCode.NONE;
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
                ResponseLoginToClient(ErrorCode.LOGIN_ALREADY_WORKING, sessionID);
                return;
            }
            
            
            var errorCode = AddUser(reqData.UserID, sessionID);
            if (errorCode != ErrorCode.NONE)
            {
                ResponseLoginToClient(errorCode, sessionID );

                if (errorCode == ErrorCode.LOGIN_FULL_USER_COUNT)
                {
                    NotifyMustCloseToClient(ErrorCode.LOGIN_FULL_USER_COUNT, sessionID);
                }

                return;
            }

            //로그인 성공
            ResponseLoginToClient(errorCode, sessionID);

            MainServer.MainLogger.Debug($"로그인 결과. UserID:{reqData.UserID}, PW: {reqData.AuthToken}, ERROR: {errorCode}");
        }
        

        public void ResponseLoginToClient(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKTResLogin()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.RES_LOGIN);

            SendFunc(sessionID, sendData);
        }

        public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKNtfMustClose()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.NTF_MUST_CLOSE);

            SendFunc(sessionID, sendData);
        }
    }
}
