using MemoryPack;
using System;
using System.Collections.Generic;

namespace Omok_Server
{
    public class PKHCommon : PKHandler
    {
        public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PacketId.INNTF_CONNECT_CLIENT, InNotifyConnectClient);
            packetHandlerMap.Add((int)PacketId.INNTF_DISCONNECT_CLIENT, InNotifyDisConnectClient);

            packetHandlerMap.Add((int)PacketId.REQ_LOGIN, RequestLogin);
        }

        public void InNotifyConnectClient(OmokBinaryRequestInfo requestData)
        {
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
                    var internalPacket = _packetMgr.MakeReqRoomLeavePacket(sessionID, user.RoomNumber, user.ID);
                    DIstributePacketAction(internalPacket);
                }

                _userMgr.RemoveUser(sessionID);

                MainServer.MainLogger.Debug($"{requestData.SessionID} 유저의 접속 해제. (IsInRoom: {user.IsInRoom})");
            }

        }


        public void RequestLogin(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                if (_userMgr.GetUserBySessionId(sessionID) != null)
                {
                    ResponseLoginToClient(ErrorCode.LOGIN_ALREADY_WORKING, packetData.SessionID);
                    return;
                }

                var reqData = _packetMgr.GetPacketData<PKTReqLogin>(packetData.Data);
                var errorCode = _userMgr.AddUser(reqData.UserID, sessionID);
                if (errorCode != ErrorCode.NONE)
                {
                    ResponseLoginToClient(errorCode, packetData.SessionID);

                    if (errorCode == ErrorCode.LOGIN_FULL_USER_COUNT)
                    {
                        NotifyMustCloseToClient(ErrorCode.LOGIN_FULL_USER_COUNT, packetData.SessionID);
                    }

                    return;
                }

                ResponseLoginToClient(errorCode, packetData.SessionID);

                MainServer.MainLogger.Debug($"로그인 결과. UserID:{reqData.UserID}, PW: {reqData.AuthToken}, ERROR: {errorCode}");

            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Debug(ex.ToString());
            }
        }

        public void ResponseLoginToClient(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKTResLogin()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.RES_LOGIN);

            SendFunc(sessionID, sendData);
        }

        public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
        {
            var resLogin = new PKNtfMustClose()
            {
                Result = (short)errorCode
            };

            var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.NTF_MUST_CLOSE);

            SendFunc(sessionID, sendData);
        }

    }
}