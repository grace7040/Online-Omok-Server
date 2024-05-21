namespace OmokServer;

public class PKHRoom : PKHandler
{
    public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
    {
        packetHandlerMap.Add((int)PacketId.ReqRoomEnter, RequestRoomEnter);
        packetHandlerMap.Add((int)PacketId.ReqRoomLeave, RequestRoomLeave);
        packetHandlerMap.Add((int)PacketId.ReqRoomChat, RequestChat);
        packetHandlerMap.Add((int)PacketId.ReqGameReady, RequestGameReady);
        packetHandlerMap.Add((int)PacketId.ReqPutStone, RequestPutStone);
        packetHandlerMap.Add((int)PacketId.ReqInRoomCheck, RequestInRoomCheck);
        packetHandlerMap.Add((int)PacketId.ResDbSaveUserGameData, ResponseInDbSaveUserGameData);
    }


    public void RequestRoomEnter(OmokBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        _mainLogger.Debug("RequestRoomEnter");

        try
        {
            var user = _userMgr.GetUserBySessionId(sessionID);
            if (!user.IsLogin) {
                // ::TODO:: 방 입장 전 로그인 해달라는 응답
                _mainLogger.Debug("방 입장 전 로그인 해야함");
                return;
            }
            var reqData = _packetMgr.GetPacketData<PKTReqRoomEnter>(packetData.Data);
            _roomMgr.EnterRoomUser(reqData, sessionID, user);
        }
        catch (Exception ex)
        {
            _mainLogger.Error(ex.ToString());
        }
    }

    
    public void RequestRoomLeave(OmokBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        _mainLogger.Debug("방나가기 요청 받음");

        try
        {
            var user = _userMgr.GetUserBySessionId(sessionID);
            if(user == null)
            {
                _mainLogger.Error("RequestRoomLeave - UserIsNotExit");
                return;
            }
            if(user.IsInRoom == false)
            {
                _mainLogger.Error("RequestRoomLeave - UserIsNotInRoom");
                return;
            }
            _roomMgr.LeaveRoom(user.RoomNumber, sessionID);
            _userMgr.GetUserBySessionId(sessionID).LeaveRoom();
        }
        catch (Exception ex)
        {
            _mainLogger.Error(ex.ToString());
        }
    }

    (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
    {
        try
        {
            var user = _userMgr.GetUserBySessionId(userNetSessionID);
            if (user == null)
            {
                return (false, null, null);
            }

            var roomNumber = user.RoomNumber;
            var room = _roomMgr.GetRoomByRoomNumber(roomNumber);

            if (room == null)
            {
                return (false, null, null);
            }

            var roomUser = room.GetRoomUserBySessionId(userNetSessionID);

            if (roomUser == null)
            {
                return (false, room, null);
            }

            return (true, room, roomUser);
        }
        catch (Exception ex)
        {
            _mainLogger.Error(ex.ToString());
        }


        return(false, null, null);
    }

    public void RequestChat(OmokBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        _mainLogger.Debug("Room RequestChat");

        var (isSucceed, room, roomUser) = CheckRoomAndRoomUser(sessionID);
        if (!isSucceed)
        {
            _mainLogger.Error("Room RequestChat - CheckRoomAndRoomUserFail");
            return;
        }

        var reqData = _packetMgr.GetPacketData<PKTReqRoomChat>(packetData.Data);
        room.NotifyRoomChat(roomUser.UserID, reqData.ChatMessage);

        _mainLogger.Debug("Room RequestChat - Success");

    }

    public void RequestGameReady(OmokBinaryRequestInfo packetData)
    {

        var sessionID = packetData.SessionID;

        var (isSucceed, room, roomUser) = CheckRoomAndRoomUser(sessionID);
        if (!isSucceed)
        {
            _mainLogger.Error("Room RequestGameReady - CheckRoomAndRoomUserFail");
            return;
        }

        room.SetUserStateReadyOrNone(sessionID);
        room.StartGameOnAllUserReady();
    }


    public void RequestPutStone(OmokBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;

        var (isSucceed, room, roomUser) = CheckRoomAndRoomUser(sessionID);
        if (!isSucceed)
        {
            _mainLogger.Error("Room RequestPutStone - CheckRoomAndRoomUserFail");
            return;
        }

        var reqData = _packetMgr.GetPacketData<PKTReqPutStone>(packetData.Data);
        room.CheckUserTurnAndPutStone(sessionID, reqData);
        
    }

    public void RequestInRoomCheck(OmokBinaryRequestInfo packetData)
    {
        _roomMgr.RoomCheckTask();
    }   

    public void ResponseInDbSaveUserGameData(OmokBinaryRequestInfo packetData)
    {
        var resData = _packetMgr.GetPacketData<PKTResDbSaveUserGameData>(packetData.Data);
        if(resData.Result != (short)ErrorCode.None)
        {
            // ::TODO:: 클라이언트에게 저장 실패 응답 전송
            return;
        }

        var sessionID = packetData.SessionID;
        var (isSucceed, room, roomUser) = CheckRoomAndRoomUser(sessionID);
        if (!isSucceed)
        {
            _mainLogger.Error("Room RequestPutStone - CheckRoomAndRoomUserFail");
            return;
        }

        //var innerPacket = _packetMgr.MakeInNTFRoomLeavePacket(sessionID, room.Number, resData.UserID);
        var innerPacket = _packetMgr.MakeInReqDisConnectUserPacket(sessionID);
        DIstributePacketAction(innerPacket);

    }
    
}
