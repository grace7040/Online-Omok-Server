﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SuperSocket.SocketBase.Logging;

namespace OmokServer;

public class PacketProcessor
{
    ILog _mainLogger;

    bool _isThreadRunning = false;
    System.Threading.Thread _processThread = null;

    BufferBlock<OmokBinaryRequestInfo> _pktBuffer = new();

    UserManager _userMgr;
    RoomManager _roomMgr;
    HeartBeatManager _heartBeatMgr;

    Dictionary<int, Action<OmokBinaryRequestInfo>> _packetHandlerMap = new();
    PKHCommon _commonPacketHandler = new();
    PKHRoom _roomPacketHandler = new();

    Func<string, byte[], bool> SendFunc;
    Action<string> CloseSessionAction;

    public void InitAndStartProcessing(ILog logger, ServerOption serverOpt, UserManager userMgr, RoomManager roomMgr, HeartBeatManager heartBeatMgr, Func<string, byte[], bool> sendFunc, Action<string> closeSessionAction)
    {
        _mainLogger = logger;

        _userMgr = userMgr;
        _roomMgr = roomMgr;
        _heartBeatMgr = heartBeatMgr;

        SendFunc = sendFunc;
        CloseSessionAction = closeSessionAction;

        _isThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();

        RegistPacketHandler();
    }

    void RegistPacketHandler()
    {
        PKHandler.SendFunc = SendFunc;
        PKHandler.DIstributePacketAction = InsertPakcet;
        PKHandler.CloseSessionAction = CloseSessionAction;

        _commonPacketHandler.Init(_mainLogger, _userMgr, _roomMgr);
        _commonPacketHandler.RegistPacketHandler(_packetHandlerMap);
        _commonPacketHandler._heartBeatMgr = _heartBeatMgr;

        _roomPacketHandler.Init(_mainLogger, _userMgr, _roomMgr);
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
                    _mainLogger.Error(ex.ToString());
                }
            }
        }
    }
}
