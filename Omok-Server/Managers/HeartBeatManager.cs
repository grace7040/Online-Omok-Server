using SuperSocket.SocketBase.Logging;

namespace Omok_Server
{
    public class HeartBeatManager
    {
        ILog _mainLogger;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        UserManager _userMgr;

        Timer _timer;
        int _interval;
        int _checkStartIndex = 0;
        int _checkUserCount;
        int _maxUserCount;
        OmokBinaryRequestInfo _innerPacket;

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;

        public void Init(ILog logger, UserManager userManager, int checkUserCount, int maxUserCount, int hartBeatInterval, Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> action)
        {
            _mainLogger = logger;
            _userMgr = userManager;
            _checkUserCount = checkUserCount;
            _maxUserCount = maxUserCount;
            _interval = hartBeatInterval;

            SendFunc = func;
            DistributeAction = action;
        }
        public void StartTimer()
        {
            _innerPacket = _packetMgr.MakeInReqHeartBeatPacket();
            _timer = new System.Threading.Timer(SendHeartBeatPkt, null, 0, _interval);
        }

        void SendHeartBeatPkt(object timerState)
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
                var innerPacket = _packetMgr.MakeInReqDisConnectUserPacket(sessionID);
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
