namespace Omok_Server
{
    public class PKHRoom : PKHandler
    {
        List<Room> _roomList = null;
        int _startRoomNumber;

        public void SetRooomList(List<Room> roomList)
        {
            _roomList = roomList;
            _startRoomNumber = roomList[0].Number;
        }

        public void RegistPacketHandler(Dictionary<int, Action<OmokBinaryRequestInfo>> packetHandlerMap)
        {
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_ENTER, RequestRoomEnter);
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_LEAVE, RequestRoomLeave);
            packetHandlerMap.Add((int)PacketId.REQ_ROOM_CHAT, RequestChat);
            packetHandlerMap.Add((int)PacketId.REQ_GAME_READY, RequestReady);
            packetHandlerMap.Add((int)PacketId.REQ_PUT_STONE, RequestPutStone);
        }

        Room GetRoomByRoomNumber(int roomNumber)
        {
            var index = roomNumber - _startRoomNumber;

            if (index < 0 || index >= _roomList.Count())
            {
                return null;
            }

            return _roomList[index];
        }

        public void RequestRoomEnter(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = _userMgr.GetUserBySessionId(sessionID);

                if (user == null || !user.IsLogin)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsInRoom)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = _packetMgr.GetPacketData<PKTReqRoomEnter>(packetData.Data);

                var room = GetRoomByRoomNumber(reqData.RoomNumber);

                if (room == null)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                //유저 추가
                if (room.AddUser(user.ID, sessionID) == false)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                    return;
                }


                user.EnteredRoom(reqData.RoomNumber);

                room.NotifyUserListToClient(sessionID);
                room.NofifyNewUserToClient(sessionID, user.ID);
                ResponseEnterRoomToClient(ErrorCode.NONE, sessionID);

                MainServer.MainLogger.Debug("RequestEnterInternal - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void ResponseEnterRoomToClient(ErrorCode errorCode, string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (short)errorCode
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resRoomEnter, PacketId.RES_ROOM_ENTER);

            SendFunc(sessionID, sendPacket);
        }

        public void RequestRoomLeave(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("방나가기 요청 받음");

            try
            {
                var user = _userMgr.GetUserBySessionId(sessionID);

                //유저가 없거나
                if (user == null)
                {
                    return;
                }

                //룸이 없거나, 해당 룸에 해당 유저가 없거나
                if (LeaveRoomUser(sessionID, user.RoomNumber) == false)
                {
                    return;
                }

                user.LeaveRoom();

                ResponseLeaveRoomToClient(sessionID);

                MainServer.MainLogger.Debug("Room RequestLeave - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(string sessionID, int roomNumber)
        {
            MainServer.MainLogger.Debug($"LeaveRoomUser. SessionID:{sessionID}");

            //해당 룸이 없거나
            var room = GetRoomByRoomNumber(roomNumber);
            if (room == null)
            {
                return false;
            }
            //해당 유저가 해당 룸에 없거나
            var roomUser = room.GetRoomUserBySessionId(sessionID);
            if (roomUser == null)
            {
                return false;
            }
            
            room.RemoveUser(roomUser);
            room.NotifyLeaveUserToClient(roomUser.UserID);

            return true;
        }

        void ResponseLeaveRoomToClient(string sessionID)
        {
            var resRoomLeave = new PKTResRoomLeave()
            {
                Result = (short)ErrorCode.NONE
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resRoomLeave, PacketId.RES_ROOM_LEAVE);

            SendFunc(sessionID, sendPacket);
        }


        public void RequestChat(OmokBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("Room RequestChat");

            try
            {
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
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
        {
            var user = _userMgr.GetUserBySessionId(userNetSessionID);
            if (user == null)
            {
                return (false, null, null);
            }

            var roomNumber = user.RoomNumber;
            var room = GetRoomByRoomNumber(roomNumber);

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

        public void RequestReady(OmokBinaryRequestInfo packetData)
        {
            var sessionId = packetData.SessionID;
            var user = _userMgr.GetUserBySessionId(sessionId);
            var room = GetRoomByRoomNumber(user.RoomNumber);

            room.SetUserStateReadyOrNone(sessionId);
            ResponseReadyToClient(sessionId, room.GetUserStateBySessionId(sessionId));

            if (room.IsAllUserReady())
            {
                if(room.IsGameStarted)
                {
                    return;
                }
                room.NotifyGameStartToClient();
                room.StartGame();
            }
        }

        void ResponseReadyToClient(string sessionId, RoomUserState roomUserState)
        {
            var resGameReady = new PKTResGameReady()
            {
                State = roomUserState
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resGameReady, PacketId.RES_GAME_READY);
            SendFunc(sessionId, sendPacket);
        }

        public void RequestPutStone(OmokBinaryRequestInfo packetData)
        {
            var reqData = _packetMgr.GetPacketData<PKTReqPutStone>(packetData.Data);
            
            var sessionId = packetData.SessionID;
            var user = _userMgr.GetUserBySessionId(sessionId);
            var room = GetRoomByRoomNumber(user.RoomNumber);

            if (room.IsUserTurn(sessionId))
            {
                ResponsePutStone(sessionId, ErrorCode.NONE);
                room.ChangeTurnAndNotifyPutStone(reqData.Position);
            }
            else
            {
                ResponsePutStone(sessionId, ErrorCode.PUT_STONE_FAIL_NOT_TURN);
            }
        }

        void ResponsePutStone(string sessionId, ErrorCode errorCode)
        {
            var resPutStone = new PKTResPutStone()
            {
                Result = (short)errorCode
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resPutStone, PacketId.RES_PUT_STONE);
            SendFunc(sessionId, sendPacket);
        }
    }
}
