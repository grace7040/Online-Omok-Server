namespace Omok_Server
{
    public class PKHRoom : PKHandler
    {
        public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_ENTER, RequestRoomEnter);
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_LEAVE, RequestRoomLeave);
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_CHAT, RequestChat);
            packetHandlerMap.Add((int)PacketId.REQ_GAME_READY, RequestGameReady);
            packetHandlerMap.Add((int)PacketId.REQ_PUT_STONE, RequestPutStone);
        }


        public void RequestRoomEnter(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = _userMgr.GetUserBySessionId(sessionID);
                var reqData = _packetMgr.GetPacketData<PKTReqRoomEnter>(packetData.Data);
                _roomMgr.CheckRoom(reqData, sessionID, user);
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        
        public void RequestRoomLeave(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("방나가기 요청 받음");

            try
            {
                var user = _userMgr.GetUserBySessionId(sessionID);
                var room = _roomMgr.GetRoomByRoomNumber(user.RoomNumber);
                room.LeaveRoomUser(sessionID, user);
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
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
                MainServer.MainLogger.Error(ex.ToString());
            }


            return(false, null, null);
        }

        public void RequestChat(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("Room RequestChat");

            var roomObject = CheckRoomAndRoomUser(sessionID);
            if (roomObject.Item1 == false)
            {
                MainServer.MainLogger.Error("Room RequestChat - CheckRoomAndRoomUserFail");
                return;
            }

            var reqData = _packetMgr.GetPacketData<PKTReqRoomChat>(packetData.Data);
            roomObject.Item2.NotifyRoomChat(roomObject.Item3.UserID, reqData.ChatMessage);

            MainServer.MainLogger.Debug("Room RequestChat - Success");

        }

        public void RequestGameReady(OmokBinaryRequestInfo packetData)
        {

            var sessionID = packetData.SessionID;

            var roomObject = CheckRoomAndRoomUser(sessionID);
            if (roomObject.Item1 == false)
            {
                MainServer.MainLogger.Error("Room RequestGameReady - CheckRoomAndRoomUserFail");
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
                MainServer.MainLogger.Error("Room RequestPutStone - CheckRoomAndRoomUserFail");
                return;
            }

            var room = roomObject.Item2;
            var reqData = _packetMgr.GetPacketData<PKTReqPutStone>(packetData.Data);
            room.CheckUserTurnAndPutStone(sessionID, reqData);
            
        }
        
    }
}
