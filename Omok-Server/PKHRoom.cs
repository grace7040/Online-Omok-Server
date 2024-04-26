using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //packetHandlerMap.Add((int)PacketId.REQ_ROOM_CHAT, RequestChat);
        }

        Room GetRoom(int roomNumber)
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
                var user = _userMgr.GetUser(sessionID);

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

                var room = GetRoom(reqData.RoomNumber);

                if (room == null)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                if (room.AddUser(user.ID, sessionID) == false)
                {
                    ResponseEnterRoomToClient(ErrorCode.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                    return;
                }


                user.EnteredRoom(reqData.RoomNumber);

                room.NotifyPacketUserListToClient(sessionID);
                room.NofifyPacketNewUserToClient(sessionID, user.ID);
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
                var user = _userMgr.GetUser(sessionID);

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
            var room = GetRoom(roomNumber);
            if (room == null)
            {
                return false;
            }
            //해당 유저가 해당 룸에 없거나
            var roomUser = room.GetUserByNetSessionId(sessionID);
            if (roomUser == null)
            {
                return false;
            }
            
            room.RemoveUser(roomUser);
            room.NotifyPacketLeaveUserToClient(roomUser.UserID);

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
    }
}
