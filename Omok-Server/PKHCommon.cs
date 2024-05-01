﻿using MemoryPack;
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
            packetHandlerMap.Add((int)PacketId.NtfInDisconnectClient, InNotifyDisConnectClient);
            packetHandlerMap.Add((int)PacketId.ReqLogin, RequestLogin);
            packetHandlerMap.Add((int)PacketId.ResHeartBeat, InResponseHeartBeat);
        }

        public void InNotifyConnectClient(OmokBinaryRequestInfo requestData)
        {
            _heartBeatMgr.AddSession(requestData.SessionID);
            MainServer.MainLogger.Debug($"{requestData.SessionID} 유저의 접속 성공");
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
                    var internalPacket = _packetMgr.MakeInNTFRoomLeavePacket(sessionID, user.RoomNumber, user.ID);
                    DIstributePacketAction(internalPacket);
                }

                _userMgr.RemoveUser(sessionID);
                

                MainServer.MainLogger.Debug($"{requestData.SessionID} 유저의 접속 해제. (IsInRoom: {user.IsInRoom})");
            }

            _heartBeatMgr.RemoveSession(sessionID);
        }


        public void RequestLogin(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            var reqData = _packetMgr.GetPacketData<PKTReqLogin>(packetData.Data);
            _userMgr.CheckLoginState(sessionID, reqData);
        }

        public void InResponseHeartBeat(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            _heartBeatMgr.ClearSessionHeartBeat(sessionID);
        }
    }
}