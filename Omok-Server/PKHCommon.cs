using MemoryPack;
using System;
using System.Collections.Generic;

namespace Omok_Server
{
    public class PKHCommon : PKHandler
    {
        public HeartBeatManager _heartBeatMgr;
        public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PacketId.NtfInConnectClient, InNotifyConnectClient);
            packetHandlerMap.Add((int)PacketId.NtfInDisConnectClient, InNotifyDisConnectClient);
            packetHandlerMap.Add((int)PacketId.ReqLogin, RequestLogin);
            packetHandlerMap.Add((int)PacketId.ResHeartBeat, ResponseHeartBeat);
            packetHandlerMap.Add((int)PacketId.ReqInHeartBeat, InRequestHeartBeat);
            packetHandlerMap.Add((int)PacketId.ReqInDisConnectUser, InReqDisConnectUser);
        }

        public void InNotifyConnectClient(OmokBinaryRequestInfo requestData)
        {
            _userMgr.AddUser(requestData.SessionID);
            _mainLogger.Debug($"{requestData.SessionID} 유저의 접속 성공");
        }


        public void InNotifyDisConnectClient(OmokBinaryRequestInfo requestData)
        {
            var sessionID = requestData.SessionID;
            var user = _userMgr.GetUserBySessionId(sessionID);

            if (user != null)
            {
                // 방에 들어가 있는 상태에서 연결이 끊어진 경우 방에서 나가게 한다.
                if (user.IsInRoom)
                {
                    var internalPacketRoomLeave = _packetMgr.MakeInNTFRoomLeavePacket(sessionID, user.RoomNumber, user.ID);
                    DIstributePacketAction(internalPacketRoomLeave);
                }

                var internalPacketDisconnect = _packetMgr.MakeInReqDisConnectUserPacket();
                DIstributePacketAction(internalPacketDisconnect);
                //_userMgr.DisConnectUser(sessionID);
                _mainLogger.Debug($"{requestData.SessionID} 유저의 접속 해제. (IsInRoom: {user.IsInRoom})");
            }
        }

        public void InReqDisConnectUser(OmokBinaryRequestInfo requestData)
        {
            var sessionID = requestData.SessionID;
            _userMgr.DisConnectUser(sessionID);
        }


        public void RequestLogin(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            _mainLogger.Debug("로그인 요청 받음");

            var reqData = _packetMgr.GetPacketData<PKTReqLogin>(packetData.Data);
            _userMgr.CheckLoginState(sessionID, reqData);
        }

        public void ResponseHeartBeat(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            _heartBeatMgr.ClearSessionHeartBeat(sessionID);
        }

        public void InRequestHeartBeat(OmokBinaryRequestInfo packetData)
        {
            _heartBeatMgr.HeartBeatTask();
        }
    }
}