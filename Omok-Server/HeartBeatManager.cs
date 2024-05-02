using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Omok_Server
{
    public class HeartBeatManager
    {
        ILog _mainLogger;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        UserManager _userMgr;

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;

        Timer _timer;
        int _interval = 250;
        int _checkStartIndex = 0;
        int _checkUserCount;
        int _maxUserCount;
        
        OmokBinaryRequestInfo _innerPacket;

        public void Init(Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> action, ILog logger, UserManager userManager, int checkUserCount, int maxUserCount)
        {
            SendFunc = func;
            DistributeAction = action;
            _mainLogger = logger;
            _userMgr = userManager;
            _checkUserCount = checkUserCount;
            _maxUserCount = maxUserCount;
        }
        public void StartTimer()
        {
            _innerPacket = _packetMgr.MakeInReqHeartBeatPacket();
            _timer = new System.Threading.Timer(SendHeartBeat, null, 0, _interval);
        }

        void SendHeartBeat(object timerState)
        {
            DistributeAction(_innerPacket);
        }

        public void HeartBeatTask()
        {
            //_mainLogger.Debug($"{_checkStartIndex}");
            for (int i = _checkStartIndex; i < _checkStartIndex+_checkUserCount; i++)
            {
                if (!_userMgr.UserList[i].IsUsing)
                {
                    continue;
                }

                CheckHeartBeat(i);
            }

            _checkStartIndex = (_checkStartIndex + _checkUserCount) % _maxUserCount;
        }
        public void CheckHeartBeat(int userIdx)
        {
            var sessionID = _userMgr.UserList[userIdx].SessionID;

            if (++_userMgr.UserList[userIdx].HeartBeatCnt >= 3)
            {
                var innerPacket = _packetMgr.MakeInNTFConnectOrDisConnectClientPacket(false, sessionID);
                DistributeAction(innerPacket);
                _mainLogger.Debug("::HeartBeat 응답 없음 - 연결 해제 - sessionID : " + sessionID);
                return;
            }

            RequestHeartBeat(sessionID);
        }

        void RequestHeartBeat(string sessionID)
        {
            var reqHeartBeat = new PKTReqHeartBeat();
            var sendData = _packetMgr.GetBinaryPacketData(reqHeartBeat, PacketId.ReqHeartBeat);
            SendFunc(sessionID, sendData);
        }

        public void ClearSessionHeartBeat(string sessionID)
        {
            var user = _userMgr.GetUserBySessionId(sessionID);
            if(user == null)
            {
                return;
            }

            user.HeartBeatCnt = 0;
        }
    }

}
