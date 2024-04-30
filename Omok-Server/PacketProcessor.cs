using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Omok_Server
{
    public class PacketProcessor
    {
        bool _isThreadRunning = false;
        System.Threading.Thread _processThread = null;

        BufferBlock<OmokBinaryRequestInfo> _pktBuffer = new BufferBlock<OmokBinaryRequestInfo>();

        UserManager _userMgr;
        RoomManager _roomMgr;

        Dictionary<int, Action<OmokBinaryRequestInfo>> _packetHandlerMap = new Dictionary<int, Action<OmokBinaryRequestInfo>>();
        PKHCommon _commonPacketHandler = new PKHCommon();
        PKHRoom _roomPacketHandler = new PKHRoom();

        Func<string, byte[], bool> SendFunc;

        public void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        public void InitAndStartProcssing(ServerOption serverOpt, UserManager userMgr, RoomManager roomMgr)
        {
            _userMgr = userMgr;
            _roomMgr = roomMgr;
            var maxUserCount = serverOpt.RoomMaxCount * serverOpt.RoomMaxUserCount;
            _userMgr.Init(maxUserCount);

            RegistPacketHandler();

            _isThreadRunning = true;
            _processThread = new System.Threading.Thread(this.Process);
            _processThread.Start();
        }

        void RegistPacketHandler()
        {
            PKHandler.SendFunc = SendFunc;
            PKHandler.DIstributePacketAction = InsertPakcet;

            _commonPacketHandler.Init(_userMgr, _roomMgr);
            _commonPacketHandler.RegistPacketHandler(_packetHandlerMap);

            _roomPacketHandler.Init(_userMgr, _roomMgr);
            _roomPacketHandler.RegistPacketHandler(_packetHandlerMap);
        }

        public void InsertPakcet(OmokBinaryRequestInfo requstPacket)
        {
            _pktBuffer.Post(requstPacket);
        }

        void Process()
        {
            while (_isThreadRunning)
            {
                try
                {
                    var packet = _pktBuffer.Receive();

                    var header = new MemoryPackPacketHeadInfo();
                    header.Read(packet.Data);

                    if (_packetHandlerMap.ContainsKey(header.Id))
                    {
                        _packetHandlerMap[header.Id](packet);
                    }
                }
                catch (Exception ex)
                {
                    if (_isThreadRunning)
                    {
                        MainServer.MainLogger.Error(ex.ToString());
                    }
                }
            }
        }
    }
}
