using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class RoomManager
    {
        List<Room> _roomList = new List<Room>();
        int _startRoomNumber;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new PacketManager<MemoryPackBinaryPacketDataCreator>();

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;

        public void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        public void SetDistributeAction(Action<OmokBinaryRequestInfo> action)
        {
            DistributeAction = action;
        }
        public void CreateRooms(ServerOption serverOpt)
        {
            var maxRoomCount = serverOpt.RoomMaxCount;
            var startNumber = serverOpt.RoomStartNumber;
            var maxUserCount = serverOpt.RoomMaxUserCount;

            for (int i = 0; i < maxRoomCount; ++i)
            {
                var roomNumber = (startNumber + i);
                var room = new Room();
                room.Init(roomNumber, maxUserCount);
                room.SetSendFunc(SendFunc);
                room.SetDistributeAction(DistributeAction);
                _roomList.Add(room);
            }

            _startRoomNumber = _roomList[0].Number;
        }


        public List<Room> GetRoomsList()
        {
            return _roomList;
        }

        public Room GetRoomByRoomNumber(int roomNumber)
        {
            var index = roomNumber - _startRoomNumber;

            if (index < 0 || index >= _roomList.Count())
            {
                return null;
            }

            return _roomList[index];
        }

        public void CheckRoom(PKTReqRoomEnter reqData, string sessionID, User user)
        {
            var room = GetRoomByRoomNumber(reqData.RoomNumber);
            ErrorCode errorCode = ErrorCode.NONE;

            if (user == null || !user.IsLogin)
            {
                errorCode = ErrorCode.ROOM_ENTER_INVALID_USER;
            }
            else if (user.IsInRoom)
            {
                errorCode = ErrorCode.ROOM_ENTER_INVALID_STATE;
            }
            else if(room == null)
            {
                errorCode = ErrorCode.ROOM_ENTER_INVALID_ROOM_NUMBER;
            }
            //유저 추가
            else if (room.AddUser(user.ID, sessionID) == false)
            {
                errorCode = ErrorCode.ROOM_ENTER_FAIL_ADD_USER;
            }

            ResponseEnterRoomToClient(errorCode, sessionID);

            if(errorCode != ErrorCode.NONE)
            {
                return;
            }

            user.EnteredRoom(reqData.RoomNumber);

            room.NotifyUserListToClient(sessionID);
            room.NofifyNewUserToClient(sessionID, user.ID);
            

            MainServer.MainLogger.Debug("RequestEnterInternal - Success");
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
    }
}
