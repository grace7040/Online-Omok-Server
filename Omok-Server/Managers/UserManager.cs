using SuperSocket.SocketBase.Logging;

namespace OmokServer;

public class UserManager
{
    ILog _mainLogger;

    int _maxUserCount;

    Dictionary<string, int> _sessionIndexDict = new();
    List<User> _userList = new();

    PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

    Func<string, byte[], bool> SendFunc;
    Action<OmokBinaryRequestInfo> DistributeRedisDBWorkAction;
    Action<OmokBinaryRequestInfo> DistributeMySqlDBWorkAction;

    public List<User> UserList { get { return _userList; } }

    public void Init(ILog logger,int maxUserCount, Func<string, byte[], bool> sendFunc, Action<OmokBinaryRequestInfo> distributeRedisDBaction, Action<OmokBinaryRequestInfo> distributeMySqlDBaction)
    {
        _maxUserCount = maxUserCount;
        SendFunc = sendFunc;
        DistributeRedisDBWorkAction = distributeRedisDBaction;
        DistributeMySqlDBWorkAction = distributeMySqlDBaction;
        _mainLogger = logger;
    }

    public void CreateUsers()
    {
        for (int i = 0; i < _maxUserCount; ++i)
        {
            var user = new User();
            _userList.Add(user);
        }
    }

    //연결 시
    public ErrorCode AddUser(string sessionID)
    {
        var emptyUserObject = GetEmptyUser();
        var user = emptyUserObject.Item1;
        if(user == null)
        {
            return ErrorCode.AddUserFailFullUserCount;
        }

        user.Use(sessionID);
        _sessionIndexDict.Add(sessionID, emptyUserObject.Item2);
        return ErrorCode.None;
    }

    public ErrorCode RemoveUserFromSessionIndexDict(string sessionID)
    {
        var user = GetUserBySessionId(sessionID);
        if (user == null)
        {
            return ErrorCode.RemoveUserFailInvalidSessionID;
        }
        _sessionIndexDict.Remove(sessionID);
        return ErrorCode.None;
    }

    (User, int) GetEmptyUser()
    {
        for (int i = 0; i < _userList.Count; i++)
        {
            if (_userList[i].IsUsing)
                continue;

            return (_userList[i], i);
        }

        return (null, -1);
    }

    public User GetUserBySessionId(string sessionID)
    {
        int index = -1;
         _sessionIndexDict.TryGetValue(sessionID, out index);

        if (index == -1 || _userList[index].IsUsing == false)
            return null;

        var user = _userList[index];
        return user;
    }

    //로그인 시 
    public void Login(string userID, string sessionID)
    {
        var errorCode = LoginUser(userID, sessionID);
        if (errorCode != ErrorCode.None)
        {
            ResponseLogin(errorCode, sessionID);

            if (errorCode == ErrorCode.LoginFailFullUserCount)
            {
                NotifyMustCloseToClient(ErrorCode.LoginFailFullUserCount, sessionID);
            }

            _mainLogger.Debug($"로그인 결과. UserID:{userID}, ERROR: {errorCode}");
            return;
        }

        //로그인 성공
        ResponseLogin(errorCode, sessionID);
        _mainLogger.Debug($"로그인 결과. UserID:{userID}, ERROR: {errorCode}");

        //게임 데이터 로드
        var innerPacket = _packetMgr.MakeInReqDbLoadUserGameDataPacket(sessionID, userID);
        DistributeMySqlDBWorkAction(innerPacket);
    }
    

    public void ResponseLogin(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.ResLogin);

        SendFunc(sessionID, sendData);
    }

    public ErrorCode LoginUser(string userID, string sessionID)
    {
        var user = GetUserBySessionId(sessionID);
        if (user == null)
        {
            return ErrorCode.LoginFailInvalidUser;
        }

        user.Login(userID);

        return ErrorCode.None;
    }

    

    public void DisConnectUser(string sessionID)
    {
        var user = GetUserBySessionId(sessionID);
        if (user == null)
        {
            return;
        }

        user.Free();
        RemoveUserFromSessionIndexDict(sessionID);
        LogoutUser(sessionID);
        
    }

    public void LogoutUser(string sessionID)
    {
        var user = GetUserBySessionId(sessionID);
        if (user == null)
        {
            return;
        }

        user.Logout();
    }

    public void CheckLoginState(string sessionID, PKTReqLogin reqData)
    {
        var user = GetUserBySessionId(sessionID);
        if (user == null)
        {
            return;
        }

        //이미 로그인되어 있는 경우
        if (user.IsLogin)
        {
            ResponseLogin(ErrorCode.LoginFailAlreadyLogined, sessionID);
            return;
        }

        //Redis에 로그인 정보 확인 요청
        var innerPacket = _packetMgr.MakeInReqDbLoginPacket(sessionID, reqData.UserID, reqData.AuthToken);
        DistributeRedisDBWorkAction(innerPacket);
    }


    public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var sendData = _packetMgr.GetBinaryPacketData(resLogin, PacketId.NtfMustClose);

        SendFunc(sessionID, sendData);
    }

    public void ResponseLoadUserGameData(string sessionID, short error, int winCount, int loseCount, int level, int exp)
    {
        var resLoadUserGameData = new PKTResLoadUserGameData()
        {
            Result = error,
            WinCount = winCount,
            LoseCount = loseCount,
            Level = level,
            Exp = exp
        };

        var sendData = _packetMgr.GetBinaryPacketData(resLoadUserGameData, PacketId.ResLoadUserGameData);
        SendFunc(sessionID, sendData);
    }

    public void UpdateUsersGameData(string winUserSessionId, string loseUserSessionId)
    {
        if(winUserSessionId == null || loseUserSessionId == null)
        {
            _mainLogger.Error("WinUser or LoseUser is NULL");
        }
        var winUser = GetUserBySessionId(winUserSessionId);
        var loseUser = GetUserBySessionId(loseUserSessionId);

        winUser.Win();
        loseUser.Lose();

        SaveUserGameData(winUserSessionId, winUser.ID, winUser.GameData.Win_Count, winUser.GameData.Lose_Count, winUser.GameData.Level, winUser.GameData.Exp);
        SaveUserGameData(loseUserSessionId, loseUser.ID, loseUser.GameData.Win_Count, loseUser.GameData.Lose_Count, loseUser.GameData.Level, loseUser.GameData.Exp);
        
    }

    void SaveUserGameData(string sessionId, string userId, int winCount, int loseCount, int level, int exp)
    {
        if (sessionId == null) return;

        var innerPacket = _packetMgr.MakeInReqDbSaveUserGameDataPacket(sessionId, userId, winCount, loseCount, level, exp);
        DistributeMySqlDBWorkAction(innerPacket);
    }

    public void RequestDbLeaveRoom(string sessionID, string userId, int roomNumber)
    {
        var innerPacket = _packetMgr.MakeInReqDbLeaveRoom(sessionID, userId, roomNumber);
        DistributeRedisDBWorkAction(innerPacket);
    }

}
