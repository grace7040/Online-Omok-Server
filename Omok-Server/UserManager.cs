using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class UserManager
    {
        ILog _mainLogger;

        int _maxUserCount;

        Dictionary<string, User> _userMap = new();

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;

        Action<OmokBinaryRequestInfo> DistributeDBWorkAction;
        
       
        
        public void Init(int maxUserCount, Func<string, byte[], bool> sendFunc, Action<OmokBinaryRequestInfo> distributeDBaction, ILog logger)
        {
            _maxUserCount = maxUserCount;
            SendFunc = sendFunc;
            DistributeDBWorkAction = distributeDBaction;
            _mainLogger = logger;
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

            //Redis에 확인
            //Redis 스레드로 sessionID, userID, Auth를 보낸다. (DbReqLogin)
            //Redis 스레드에서 검증 완료하면 DbResLogin 패킷을 DistributeDB한다
            //DbResLogin 패킷을 받으면 Login처리를 한다.
            Login(reqData.UserID, sessionID);
        }

        public void Login(string userID, string sessionID)
        {
            var errorCode = AddUser(userID, sessionID);
            if (errorCode != ErrorCode.None)
            {
                ResponseLogin(errorCode, sessionID);

                if (errorCode == ErrorCode.LoginFailFullUserCount)
                {
                    NotifyMustCloseToClient(ErrorCode.LoginFailFullUserCount, sessionID);
                }

                return;
            }

            //로그인 성공
            ResponseLogin(errorCode, sessionID);

            _mainLogger.Debug($"로그인 결과. UserID:{userID}, ERROR: {errorCode}");
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
