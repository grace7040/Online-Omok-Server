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

        UserManager _userMgr = new UserManager();
        List<Room> _roomList = new List<Room>();

        Dictionary<int, Action<OmokBinaryRequestInfo>> _packetHandlerMap = new Dictionary<int, Action<OmokBinaryRequestInfo>>();
        PKHCommon _commonPacketHandler = new PKHCommon();
        //PKHRoom _roomPacketHandler = new PKHRoom();

        Func<string, byte[], bool> SendFunc;

        public void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        public void SetRoomList(List<Room> roomList)
        {
            _roomList = roomList;
            var minRoomNum = _roomList[0].Number;
            var maxRoomNum = _roomList[0].Number + _roomList.Count() - 1;
        }


        //roomList는 어따쓸려고? 아..핸들러 등록할때 쓰는구나.
        public void InitAndStartProcssing(ServerOption serverOpt)
        {
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

            _commonPacketHandler.Init(_userMgr);
            _commonPacketHandler.RegistPacketHandler(_packetHandlerMap);
        }

        public void InsertPakcet(OmokBinaryRequestInfo requstPacket)
        {
            _pktBuffer.Post(requstPacket);
        }

        void Process()
        {
            while (_isThreadRunning)
            {
                //System.Threading.Thread.Sleep(64); //테스트 용
                try
                {
                    var packet = _pktBuffer.Receive();

                    var header = new MemoryPackPacketHeadInfo();
                    header.Read(packet.Data);

                    if (_packetHandlerMap.ContainsKey(header.Id))
                    {
                        _packetHandlerMap[header.Id](packet);
                    }
                    /*else
                    {
                        System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                    }*/
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
