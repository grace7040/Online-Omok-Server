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
            packetHandlerMap.Add((int)PacketId.ResDbLogin, InResponseDbLogin);
            packetHandlerMap.Add((int)PacketId.ResDbLoadUserGameData, InResponseDbLoadUserGameData);
        }

        public void InNotifyConnectClient(OmokBinaryRequestInfo packetData)
        {
            _userMgr.AddUser(packetData.SessionID);
            _mainLogger.Debug($"{packetData.SessionID} 유저의 접속 성공");
        }


        public void InNotifyDisConnectClient(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
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
                _mainLogger.Debug($"{packetData.SessionID} 유저의 접속 해제. (IsInRoom: {user.IsInRoom})");
            }
        }

        public void InReqDisConnectUser(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
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

        public void InResponseDbLogin(OmokBinaryRequestInfo packetData)
        {
            var resData = _packetMgr.GetPacketData<PKTResInLogin>(packetData.Data);

            if(resData.Result != (short)ErrorCode.None)
            {
                _mainLogger.Debug($"로그인 실패: {resData.Result}");
                _userMgr.ResponseLogin((ErrorCode)resData.Result, packetData.SessionID);
                return;
            }
            _userMgr.Login(resData.UserID, packetData.SessionID);
        }
        
        public void InResponseDbLoadUserGameData(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            var resData = _packetMgr.GetPacketData<PKTResDbLoadUserGameData>(packetData.Data);
            var user = _userMgr.GetUserBySessionId(sessionID);
            
            if(user == null)
            {
                _mainLogger.Error($"[InResponseDbLoadUserGameData Fail] {resData.UserID} 유저가 없습니다.");
                return;
            }
            else if (resData.Result != (short)ErrorCode.None)
            {
                _mainLogger.Error($"유저 데이터 로드 실패: {resData.Result}");
                _userMgr.ResponseLoadUserGameData(sessionID, resData.Result, 0, 0, 0, 0);
                _userMgr.LogoutUser(sessionID);
                return;
            }

            user.SetGameData(new UserGameData() { Level = resData.Level,
                                                Exp = resData.Exp,
                                                Win_Count = resData.WinCount,
                                                Lose_Count = resData.LoseCount, });
            
            _userMgr.ResponseLoadUserGameData(sessionID, resData.Result, resData.WinCount, resData.LoseCount, resData.Level, resData.Exp); ;
        }
    }
}