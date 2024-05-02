using MemoryPack;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Omok_Server
{
    public enum RoomState { None, GameStart, GameEnd }



    public class Room
    {
        ILog _mainLogger;

        public const int InvalidRoomNumber = -1;
        public int Number { get; private set; }

        int _maxUserCount = 0;

        RoomState _state;

        public bool IsGameStarted { get { return _state == RoomState.GameStart; } }
        public bool IsGameEnded { get { return _state == RoomState.GameEnd; } }

        List<RoomUser> _userList = new List<RoomUser>();

        OmokGame _game;

        protected PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;
        public void Init(int number, int maxUserCount, Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> action, ILog logger)
        {
            Number = number;
            _maxUserCount = maxUserCount;
            SendFunc = func;
            DistributeAction = action;
            _mainLogger = logger;
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

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomUserList, PacketId.NtfRoomUserList);
            SendFunc(sessionID, sendPacket);
        }

        public void NofifyNewUserToClient(string newUserSessionID, string newUserID)
        {
            var ntfRoomNewUser = new PKTNtfRoomNewUser()
            {
                UserID = newUserID
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomNewUser, PacketId.NtfRoomNewUser);

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

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfRoomLeaveUser, PacketId.NtfRoomLeaveUser);

            Broadcast("", sendPacket);
        }

        public void NotifyRoomChat(string userId, string chatMessage)
        {
            var notifyRoomChat = new PKTNtfRoomChat()
            {
                UserID = userId,
                ChatMessage = chatMessage
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyRoomChat, PacketId.NtfRoomChat);

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

            var sendPacket = _packetMgr.GetBinaryPacketData(resGameReady, PacketId.ResGameReady);
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

        public void NotifyGameStartToClient(string sessionID, StoneColor stoneColor)
        {
            var notifyGameStart = new PKTNtfGameStart() { MyStoneColor = stoneColor};

            var sendPacket = _packetMgr.GetBinaryPacketData(notifyGameStart, PacketId.NtfGameStart);
            SendFunc(sessionID, sendPacket);
        }

        //Set Random Start Turn / Stone Color According to Turn
        public void SetRandomTurnAndStart()
        {
            Dictionary<string, StoneColor> userStoneColorDict = new Dictionary<string, StoneColor>();
            Random random = new Random();
            var turnIndex = random.Next(0, _userList.Count);

            userStoneColorDict.Add(_userList[turnIndex].SessionID, StoneColor.Black);
            NotifyGameStartToClient(_userList[turnIndex].SessionID, StoneColor.Black);

            for (int i = 0; i < _userList.Count; i++)
            {
                if(i == turnIndex)
                {
                    continue;
                }
                userStoneColorDict.Add(_userList[i].SessionID, StoneColor.White);
                NotifyGameStartToClient(_userList[i].SessionID, StoneColor.White);
            }
            _game = new OmokGame(userStoneColorDict, SendFunc, GameEnd);

            _mainLogger.Debug($"[Room {Number}] SetRandomTurnAndStart. UserID: {_userList[turnIndex].UserID}");
        }

        public void CheckUserTurnAndPutStone(string sessionID, PKTReqPutStone reqData)
        {
            if (!_game.IsUserTurn(sessionID))
            {
                _game.ResponsePutStone(sessionID, ErrorCode.PutStoneFailNotTurn);
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

            _mainLogger.Debug("Room RequestLeave - Success");
        }


        void ResponseLeaveRoomToClient(string sessionID)
        {
            var resRoomLeave = new PKTResRoomLeave()
            {
                Result = (short)ErrorCode.None
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(resRoomLeave, PacketId.ResRoomLeave);

            SendFunc(sessionID, sendPacket);
        }

        void GameEnd(StoneColor winnerColor)
        {
            _mainLogger.Debug($"[GameEnd] WINNER: {winnerColor}");
            _state = RoomState.GameEnd;

            var ntfGameEnd = new PKTNtfGameEnd()
            {
                WinnerColor = winnerColor
            };

            var sendPacket = _packetMgr.GetBinaryPacketData(ntfGameEnd, PacketId.NtfGameENd);
            Broadcast("", sendPacket);

            /* ::TODO:: 게임 데이터 저장 및 n초 대기 */

            LeaveRoomAllUsers();
        }

        void LeaveRoomAllUsers()
        {
            for (int i = 0; i < _userList.Count; i++)
            {
                var innerPacket = _packetMgr.MakeInNTFRoomLeavePacket(_userList[i].SessionID, Number, _userList[i].UserID);
                
                DistributeAction(innerPacket);
            }
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
