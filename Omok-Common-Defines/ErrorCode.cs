// 7001 ~ 8000
public enum ErrorCode : short
{
    None = 0,

    LoginFailFullUserCount = 7001,
    LoginFailAlreadyLogined,
    LoginFailInvalidUser,

    AddUserDuplication = 7011,
    AddUserFailFullUserCount,

    RemoveUserFailInvalidSessionID = 7021,
    RemoveUserRoomNumberFail,
    RemoveUserFromMatchingQueueFail,

    RoomEnterFailInvalidUser = 7031,
    RoomEnterFailInvalidState,
    RoomEnterFailInvalidRoomNumber,
    RoomEnterFailAddUser,

    CheckUserAuthFailNotExist = 7041,
    CheckUserAuthFailNotMatch,
    CheckUserAuthFailException,

    LoadUserGameDataFailNotExist = 7051,
    UpdateUserGameDataFail,
    
    PopMatchingRequestFromRedisFail = 7061,

    PutStoneFailNotTurn = 7101,
    PutStoneFailInvalidPosition,
    PutStoneFailNotGameStart,
}