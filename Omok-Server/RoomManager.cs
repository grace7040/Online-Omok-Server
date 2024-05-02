﻿using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class RoomManager
    {
        ILog _mainLogger;
        List<Room> _roomList = new List<Room>();
        int _startRoomNumber;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;

        public void Init(Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> action, ILog logger)
        {
            SendFunc = func;
            DistributeAction = action;
            _mainLogger = logger;
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
                room.Init(roomNumber, maxUserCount, SendFunc, DistributeAction, _mainLogger);
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
            ErrorCode errorCode = ErrorCode.None;

            if (user == null || !user.IsLogin)
            {
                errorCode = ErrorCode.RoomEnterFailInvalidUser;
            }
            else if (user.IsInRoom)
            {
                errorCode = ErrorCode.RoomEnterFailInvalidState;
            }
            else if(room == null)
            {
                errorCode = ErrorCode.RoomEnterFailInvalidRoomNumber;
            }
            //유저 추가
            else if (room.AddUser(user.ID, sessionID) == false)
            {
                errorCode = ErrorCode.RoomEnterFailAddUser;
            }

            ResponseEnterRoomToClient(errorCode, sessionID);

            if(errorCode != ErrorCode.None)
            {
                return;
            }

            user.EnterRoom(reqData.RoomNumber);

            room.NotifyUserListToClient(sessionID);
            room.NofifyNewUserToClient(sessionID, user.ID);
            

            _mainLogger.Debug("RequestEnterInternal - Success");
        }

        void ResponseEnterRoomToClient(ErrorCode errorCode, string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (short)errorCode
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resRoomEnter, PacketId.ResRoomEnter);

            SendFunc(sessionID, sendPacket);
        }
    }
}
