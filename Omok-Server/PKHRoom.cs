namespace Omok_Server
{
    public class PKHRoom : PKHandler
    {
        public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PacketId.ReqRoomEnter, RequestRoomEnter);
            packetHandlerMap.Add((int)PacketId.ReqRoomLeave, RequestRoomLeave);
            packetHandlerMap.Add((int)PacketId.ReqRoomChat, RequestChat);
            packetHandlerMap.Add((int)PacketId.ReqGameReady, RequestGameReady);
            packetHandlerMap.Add((int)PacketId.ReqPutStone, RequestPutStone);
        }


        public void RequestRoomEnter(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            _mainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = _userMgr.GetUserBySessionId(sessionID);
                var reqData = _packetMgr.GetPacketData<PKTReqRoomEnter>(packetData.Data);
                _roomMgr.CheckRoom(reqData, sessionID, user);
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
                var room = _roomMgr.GetRoomByRoomNumber(user.RoomNumber);
                room.LeaveRoomUser(sessionID, user);
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

            var roomObject = CheckRoomAndRoomUser(sessionID);
            if (roomObject.Item1 == false)
            {
                _mainLogger.Error("Room RequestChat - CheckRoomAndRoomUserFail");
                return;
            }

            var reqData = _packetMgr.GetPacketData<PKTReqRoomChat>(packetData.Data);
            roomObject.Item2.NotifyRoomChat(roomObject.Item3.UserID, reqData.ChatMessage);

            _mainLogger.Debug("Room RequestChat - Success");

        }

        public void RequestGameReady(OmokBinaryRequestInfo packetData)
        {

            var sessionID = packetData.SessionID;

            var roomObject = CheckRoomAndRoomUser(sessionID);
            if (roomObject.Item1 == false)
            {
                _mainLogger.Error("Room RequestGameReady - CheckRoomAndRoomUserFail");
                return;
            }

            var room = roomObject.Item2;
            room.SetUserStateReadyOrNone(sessionID);
            room.StartGameOnAllUserReady();

        }
 

        public void RequestPutStone(OmokBinaryRequestInfo packetData)
        {
            
            var sessionID = packetData.SessionID;

            var roomObject = CheckRoomAndRoomUser(sessionID);
            if (roomObject.Item1 == false)
            {
                _mainLogger.Error("Room RequestPutStone - CheckRoomAndRoomUserFail");
                return;
            }

            var room = roomObject.Item2;
            var reqData = _packetMgr.GetPacketData<PKTReqPutStone>(packetData.Data);
            room.CheckUserTurnAndPutStone(sessionID, reqData);
            
        }
        
    }
}
