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
    public enum RoomState { None, GameStart, GameEnd }
    public enum RoomUserState { None, Ready, Turn, NotTurn, GameEnd }
    public enum StoneColor { None, Black, White }


    public class Room
    {
        public const int InvalidRoomNumber = -1;
        public int Number { get; private set; }

        int _maxUserCount = 0;

        RoomState _state;

        public bool IsGameStarted { get { return _state != RoomState.None; } }
        public bool IsGameEnded { get { return _state == RoomState.GameEnd; } }

        List<RoomUser> _userList = new List<RoomUser>();

        StoneColor _curStoneColor;

        protected PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

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
            if (GetRoomUserByUserId(userID) != null)
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

        public RoomUser GetRoomUserByUserId(string userID)
        {
            return _userList.Find(x => x.UserID == userID);
        }

        public RoomUser GetRoomUserBySessionId(string netSessionID)
        {
            return _userList.Find(x => x.NetSessionID == netSessionID);
        }

        public int CurrentUserCount()
        {
            return _userList.Count();
        }

        public void NotifyUserListToClient(string userNetSessionID)
        {
            var ntfRoomUserList = new PKTNtfRoomUserList();
            foreach (var user in _userList)
            {
                ntfRoomUserList.UserIDList.Add(user.UserID);
            }

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomUserList, PacketId.NTF_ROOM_USER_LIST);
            SendFunc(userNetSessionID, sendPacket);
        }

        public void NofifyNewUserToClient(string newUserNetSessionID, string newUserID)
        {
            var ntfRoomNewUser = new PKTNtfRoomNewUser()
            {
                UserID = newUserID
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomNewUser, PacketId.NTF_ROOM_NEW_USER);

            Broadcast(newUserNetSessionID, sendPacket);
        }
        public void NotifyLeaveUserToClient(string userID)
        {
            if (CurrentUserCount() == 0)
            {
                return;
            }

            var ntfRoomLeaveUser = new PKTNtfRoomLeaveUser()
            {
                UserID = userID
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomLeaveUser, PacketId.NTF_ROOM_LEAVE_USER);

            Broadcast("", sendPacket);
        }

        public void NotifyRoomChat(string userId, string chatMessage)
        {
            var notifyRoomChat = new PKTNtfRoomChat()
            {
                UserID = userId,
                ChatMessage = chatMessage
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyRoomChat, PacketId.NTF_ROOM_CHAT);

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

        public void SetUserStateReadyOrNone(string netSessionId)
        {
            var roomUser = GetRoomUserBySessionId(netSessionId);

            if(roomUser.State == RoomUserState.None)
            {
                roomUser.State = RoomUserState.Ready;
            }
            else if(roomUser.State == RoomUserState.Ready)
            {
                roomUser.State = RoomUserState.None;
            }

        }

        public RoomUserState GetUserStateBySessionId(string netSessionId)
        {
            var roomUser = GetRoomUserBySessionId(netSessionId);
            return roomUser.State;
        }

        public bool IsAllUserReady()
        {
            if(_userList.Count < _maxUserCount)
            {
                return false;
            }

            for(int i = 0; i < _userList.Count; i++)
            {
                if (_userList[i].State != RoomUserState.Ready)
                {
                    return false;
                }
            }
            return true;
        }

        public void StartGame()
        {
            _state = RoomState.GameStart;
            SetRandomTurnOnStart();
        }

        public void NotifyGameStartToClient()
        {
            var notifyGameStart = new PKTNtfGameStart();

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyGameStart, PacketId.NTF_GAME_START);

            Broadcast("", sendPacket);
        }

        //Set Random Start Turn / Stone Color According to Turn
        public void SetRandomTurnOnStart()
        {
            Random random = new Random();
            var turnIndex = random.Next(0, _userList.Count);
            _userList[turnIndex].State = RoomUserState.Turn;

            NotifyTurnToClient(_userList[turnIndex].NetSessionID, StoneColor.Black, null);
            MainServer.MainLogger.Debug($"[Room {Number}] SetRandomTurnOnStart. UserID: {_userList[turnIndex].UserID}");
            
            for (int i = 0; i < _userList.Count; i++)
            {
                if(i == turnIndex)
                {
                    continue;
                }
                _userList[i].State = RoomUserState.NotTurn;
            }

            _curStoneColor = StoneColor.Black;
        }

        //Notify Turn & Put Other's Stone
        void NotifyTurnToClient(string netSessionID, StoneColor stoneColor, Tuple<int,int>? position)
        {
            var notifyTurn = new PKTNtfGameTurn() { 
                Color = stoneColor,
                Position = position
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyTurn, PacketId.NTF_GAME_TURN);

            SendFunc(netSessionID, sendPacket);
        }

        public bool IsUserTurn(string netSessionID)
        {
            var roomUser = GetRoomUserBySessionId(netSessionID);
            return roomUser.State == RoomUserState.Turn;
        }

        public void ChangeTurnAndNotifyPutStone(Tuple<int, int>? position)
        {
            for (int i = 0; i < _userList.Count; i++)
            {
                if (_userList[i].State != RoomUserState.Turn)
                {
                    _userList[i].State = RoomUserState.Turn;
                    NotifyTurnToClient(_userList[i].NetSessionID, _curStoneColor, position);
                    continue;
                }
                _userList[i].State = RoomUserState.NotTurn;
            }

            _curStoneColor = _curStoneColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
        }
    }
    
    
    public class RoomUser
    {
        public string UserID { get; private set; }
        public string NetSessionID { get; private set; }

        public RoomUserState State { get; set; }
        public void Set(string userID, string netSessionID)
        {
            UserID = userID;
            NetSessionID = netSessionID;
        }
    }

    
}
