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
        SortedList<string, int> _sessionList = new();

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;

        Timer _timer;
        int _interval = 250;
        int _index = 0;

        OmokBinaryRequestInfo _innerPacket;

        public void Init(Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> action, ILog logger)
        {
            SendFunc = func;
            DistributeAction = action;
            _mainLogger = logger;
            _innerPacket = _packetMgr.MakeInReqHeartBeatPacket();
            _timer = new System.Threading.Timer(StartTimer, null, 0, _interval);
            
        }
        public void StartTimer(object timerState)
        {
            DistributeAction(_innerPacket);
        }

        public void HeartBeatTask()
        {
            if(_sessionList.Count == 0)
            {
                return;
            }

            for(int i = 0; i < _sessionList.Count/4; i++)
            {
                _index = (_index+1)%_sessionList.Count;
                var sessionID = _sessionList.GetKeyAtIndex(_index);
                CheckHeartBeat(sessionID);
                //_mainLogger.Debug($"{_index}");
            }
        }
        public void CheckHeartBeat(string sessionID)
        {
            if(_sessionList.ContainsKey(sessionID) == false)
            {
                return;
            }

            if(++_sessionList[sessionID] >= 3)
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
            _sessionList[sessionID] = 0;
        }

        public void AddSession(string sessionID)
        {
            _sessionList.Add(sessionID, 0);
        }

        public void RemoveSession(string sessionID)
        {
            _sessionList.Remove(sessionID);
        }
    }

}
