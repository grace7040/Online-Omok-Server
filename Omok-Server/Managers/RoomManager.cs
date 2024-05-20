using SuperSocket.SocketBase.Logging;

namespace OmokServer
{
    public class RoomManager
    {
        ILog _mainLogger;
        List<Room> _roomList = new List<Room>();
        int _startRoomNumber;

        Timer _timer;
        int _interval;
        int _checkStartIndex = 0;
        int _checkRoomCount;
        OmokBinaryRequestInfo _innerPacket;
        ServerOption _serverOption;

        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

        Func<string, byte[], bool> SendFunc;
        Action<OmokBinaryRequestInfo> DistributeAction;
        Action<OmokBinaryRequestInfo> DistributeMySqlDbAction;
        Action<string, string> UpdateUsersGameDataAction;
        Action<int> AddRoomToEmptyRoomQueueAction;

        public void Init(ILog logger, ServerOption serverOption, Func<string, byte[], bool> func, Action<OmokBinaryRequestInfo> distributeAction, Action<OmokBinaryRequestInfo> distributeMySqlAction, Action<string, string>  updateUserGameDataAction, Action<int> addEmptyRoomAction)
        {
            SendFunc = func;
            DistributeAction = distributeAction;
            DistributeMySqlDbAction = distributeMySqlAction;
            UpdateUsersGameDataAction = updateUserGameDataAction;
            _mainLogger = logger;
            _serverOption = serverOption;
            _interval = serverOption.RoomCheckInterval;
            _checkRoomCount = serverOption.CheckRoomCount;
            AddRoomToEmptyRoomQueueAction = addEmptyRoomAction;
            StartTimer();
        }

        public void StartTimer()
        {
            _innerPacket = _packetMgr.MakeInReqRoomCheckPacket();
            _timer = new System.Threading.Timer(SendRoomCheckPkt, null, 0, _interval);
        }
        void SendRoomCheckPkt(object timerState)
        {
            DistributeAction(_innerPacket);
        }
        public void RoomCheckTask()
        {
            //_mainLogger.Debug($"{_checkStartIndex}");
            for (int i = _checkStartIndex; i < _checkStartIndex+_checkRoomCount; i++)
            {
                if (_roomList[i].GetCurrentUserCount() == 0)
                {
                    continue;
                }
                _roomList[i].CheckRoomState();
            }

            _checkStartIndex = (_checkStartIndex + _checkRoomCount) % _roomList.Count;
        }


        public void CreateRooms()
        {
            var maxRoomCount = _serverOption.RoomMaxCount;
            var startNumber = _serverOption.RoomStartNumber;
            var maxUserCount = _serverOption.RoomMaxUserCount;

            for (int i = 0; i < maxRoomCount; ++i)
            {
                var roomNumber = (startNumber + i);
                var room = new Room();
                room.Init(_mainLogger, roomNumber, maxUserCount, _serverOption.MaxGameTime, _serverOption.TurnTimeOut, _serverOption.MaxTurnOverCnt, SendFunc, DistributeAction, DistributeMySqlDbAction, UpdateUsersGameDataAction, AddRoomToEmptyRoomQueueAction);
                _roomList.Add(room);
                AddRoomToEmptyRoomQueueAction(roomNumber);
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

        public void EnterRoomUser(PKTReqRoomEnter reqData, string sessionID, User user)
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
            if (room.AddUser(user.ID, sessionID) == false)
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

        public void LeaveRoom(int roomNumber, string sessionID)
        {
            var room = GetRoomByRoomNumber(roomNumber);
            room.LeaveRoomUser(sessionID);
            
        }
    }
}
