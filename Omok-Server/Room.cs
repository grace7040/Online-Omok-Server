using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Omok_Server
{
    public class Room
    {
        public const int InvalidRoomNumber = -1;
        public int Number { get; private set; }

        int _maxUserCount = 0;

        List<RoomUser> _userList = new List<RoomUser>();

        protected PacketManager<MemoryPackBinaryPacketDataCreater> _packetMgr = new();

        static Func<string, byte[], bool> SendFunc;
        public void Init(int number, int maxUserCount)
        {
            Number = number;
            _maxUserCount = maxUserCount;
        }
        public static void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        public bool AddUser(string userID, string netSessionID)
        {
            if (GetUser(userID) != null)
            {
                return false;
            }

            var roomUser = new RoomUser();
            roomUser.Set(userID, netSessionID);
            _userList.Add(roomUser);

            return true;
        }

        public void RemoveUser(string netSessionID)
        {
            var index = _userList.FindIndex(x => x.NetSessionID == netSessionID);
            _userList.RemoveAt(index);
        }

        public bool RemoveUser(RoomUser user)
        {
            return _userList.Remove(user);
        }

        public RoomUser GetUser(string userID)
        {
            return _userList.Find(x => x.UserID == userID);
        }

        public RoomUser GetUserByNetSessionId(string netSessionID)
        {
            return _userList.Find(x => x.NetSessionID == netSessionID);
        }

        public int CurrentUserCount()
        {
            return _userList.Count();
        }

        public void NotifyPacketUserListToClient(string userNetSessionID)
        {
            var ntfRoomUserList = new PKTNtfRoomUserList();
            foreach (var user in _userList)
            {
                ntfRoomUserList.UserIDList.Add(user.UserID);
            }

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomUserList, PacketId.NTF_ROOM_USER_LIST);
            SendFunc(userNetSessionID, sendPacket);
        }

        public void NofifyPacketNewUserToClient(string newUserNetSessionID, string newUserID)
        {
            var ntfRoomNewUser = new PKTNtfRoomNewUser();
            ntfRoomNewUser.UserID = newUserID;

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomNewUser, PacketId.NTF_ROOM_NEW_USER);

            Broadcast(newUserNetSessionID, sendPacket);
        }
        public void NotifyPacketLeaveUserToClient(string userID)
        {
            if (CurrentUserCount() == 0)
            {
                return;
            }

            var ntfRoomLeaveUser = new PKTNtfRoomLeaveUser();
            ntfRoomLeaveUser.UserID = userID;

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomLeaveUser, PacketId.NTF_ROOM_LEAVE_USER);

            Broadcast("", sendPacket);
        }

        public void NotifyPacketRoomChat(string userId, string chatMessage)
        {
            var notifyPacket = new PKTNtfRoomChat()
            {
                UserID = userId,
                ChatMessage = chatMessage
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyPacket, PacketId.NTF_ROOM_CHAT);

            Broadcast("", sendPacket);
        }

        public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
        {
            foreach (var user in _userList)
            {
                if (user.NetSessionID == excludeNetSessionID)
                {
                    continue;
                }

                SendFunc(user.NetSessionID, sendPacket);
            }
        }
    }

    public class RoomUser
    {
        public string UserID { get; private set; }
        public string NetSessionID { get; private set; }

        public void Set(string userID, string netSessionID)
        {
            UserID = userID;
            NetSessionID = netSessionID;
        }
    }
}
