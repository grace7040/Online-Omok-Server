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



    public class Room
    {
        public const int InvalidRoomNumber = -1;
        public int Number { get; private set; }

        int _maxUserCount = 0;

        RoomState _state;

        public bool IsGameStarted { get { return _state != RoomState.None; } }
        public bool IsGameEnded { get { return _state == RoomState.GameEnd; } }

        List<RoomUser> _userList = new List<RoomUser>();

        //StoneColor _curStoneColor;

        OmokGame _game;

        protected PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;
        public void Init(int number, int maxUserCount)
        {
            Number = number;
            _maxUserCount = maxUserCount;
        }
        public void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }

        public bool AddUser(string userID, string sessionID)
        {
            if (GetRoomUserByUserId(userID) != null)
            {
                return false;
            }

            var roomUser = new RoomUser();
            roomUser.Set(userID, sessionID);
            _userList.Add(roomUser);

            return true;
        }

        public void RemoveUser(string sessionID)
        {
            var index = _userList.FindIndex(x => x.SessionID == sessionID);
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

        public RoomUser GetRoomUserBySessionId(string sessionID)
        {
            return _userList.Find(x => x.SessionID == sessionID);
        }

        public int CurrentUserCount()
        {
            return _userList.Count();
        }

        public void NotifyUserListToClient(string sessionID)
        {
            var ntfRoomUserList = new PKTNtfRoomUserList();
            foreach (var user in _userList)
            {
                ntfRoomUserList.UserIDList.Add(user.UserID);
            }

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomUserList, PacketId.NTF_ROOM_USER_LIST);
            SendFunc(sessionID, sendPacket);
        }

        public void NofifyNewUserToClient(string newUserSessionID, string newUserID)
        {
            var ntfRoomNewUser = new PKTNtfRoomNewUser()
            {
                UserID = newUserID
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomNewUser, PacketId.NTF_ROOM_NEW_USER);

            Broadcast(newUserSessionID, sendPacket);
        }

        public void NotifyLeaveRoomUserToClient(string userID)
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

        public void Broadcast(string excludeSessionID, byte[] sendPacket)
        {
            foreach (var user in _userList)
            {
                if (user.SessionID == excludeSessionID)
                {
                    continue;
                }

                SendFunc(user.SessionID, sendPacket);
            }
        }

        public void SetUserStateReadyOrNone(string sessionID)
        {
            var roomUser = GetRoomUserBySessionId(sessionID);

            if(roomUser.State == RoomUserState.None)
            {
                roomUser.State = RoomUserState.Ready;
            }
            else if(roomUser.State == RoomUserState.Ready)
            {
                roomUser.State = RoomUserState.None;
            }
            ResponseReadyToClient(sessionID, GetUserStateBySessionId(sessionID));
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

        public void StartGameOnAllUserReady()
        {
            if (IsAllUserReady())
            {
                if (IsGameStarted)
                {
                    return;
                }
                NotifyGameStartToClient();
                StartGame();
            }
        }

        public RoomUserState GetUserStateBySessionId(string sessionId)
        {
            var roomUser = GetRoomUserBySessionId(sessionId);
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
            SetRandomTurnAndStart();
        }

        public void NotifyGameStartToClient()
        {
            var notifyGameStart = new PKTNtfGameStart();

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyGameStart, PacketId.NTF_GAME_START);

            Broadcast("", sendPacket);
        }

        //Set Random Start Turn / Stone Color According to Turn
        public void SetRandomTurnAndStart()
        {
            Dictionary<string, StoneColor> userStoneColorDict = new Dictionary<string, StoneColor>();
            Random random = new Random();
            var turnIndex = random.Next(0, _userList.Count);

            userStoneColorDict.Add(_userList[turnIndex].SessionID, StoneColor.Black);
            for (int i = 0; i < _userList.Count; i++)
            {
                if(i == turnIndex)
                {
                    continue;
                }
                userStoneColorDict.Add(_userList[i].SessionID, StoneColor.White);
            }
            _game = new OmokGame(userStoneColorDict, SendFunc, GameEnd);

            MainServer.MainLogger.Debug($"[Room {Number}] SetRandomTurnAndStart. UserID: {_userList[turnIndex].UserID}");
        }
        

        public void CheckUserTurnAndPutStone(string sessionID, PKTReqPutStone reqData)
        {
            if (!_game.IsUserTurn(sessionID))
            {
                _game.ResponsePutStone(sessionID, ErrorCode.PUT_STONE_FAIL_NOT_TURN);
                return;
            }

            _game.PutStone(reqData.Position.Item1, reqData.Position.Item2);
        }

        

        public void LeaveRoomUser(string sessionID, User user)
        {
            //유저가 없거나
            if (user == null)
            {
                return;
            }

            //해당 유저가 해당 룸에 없거나
            var roomUser = GetRoomUserBySessionId(sessionID);
            if (roomUser == null)
            {
                return;
            }

            user.LeaveRoom();
            RemoveUser(roomUser);
            NotifyLeaveRoomUserToClient(roomUser.UserID);
            ResponseLeaveRoomToClient(sessionID);

            MainServer.MainLogger.Debug("Room RequestLeave - Success");
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

        void GameEnd(StoneColor winnerColor)
        {
            MainServer.MainLogger.Debug($"[GameEnd] WINNER: {winnerColor}");

            var ntfGameEnd = new PKTNtfGameEnd()
            {
                WinnerColor = winnerColor
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfGameEnd, PacketId.NTF_GAME_END);
            Broadcast("", sendPacket);
            // ::TODO:: 결과 보여주고, 방 나가기 처리
        }
    }
    
    
    public class RoomUser
    {
        public string UserID { get; private set; }
        public string SessionID { get; private set; }

        public RoomUserState State { get; set; }
        public void Set(string userID, string sessionID)
        {
            UserID = userID;
            SessionID = sessionID;
        }
    }

    
}
