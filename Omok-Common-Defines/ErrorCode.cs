using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ErrorCode : short
{
    None = 0,
    LoginFailFullUserCount = 1,
    AddUserDuplication = 2,
    RemoveUserFailInvalidSessionID = 3,
    LoginFailAlreadyLogined = 4,
    RoomEnterFailInvalidUser = 5,
    RoomEnterFailInvalidState = 6,
    RoomEnterFailInvalidRoomNumber = 7,
    RoomEnterFailAddUser = 8,
    PutStoneFailNotTurn = 9,
    PutStoneFailInvalidPosition = 10,
    AddUserFailFullUserCount = 11,
    LoginFailInvalidUser = 12,
    CheckUserAuthFailNotExist = 13,
    CheckUserAuthFailNotMatch = 14,
    CheckUserAuthFailException = 15,
    LoadUserGameDataFailNotExist = 16,
    UpdateUserGameDataFail = 17,
}