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
    RemoveUserSearchFailUserId = 3,
    LoginFailAlreadyLogined = 4,
    RoomEnterFailInvalidUser = 5,
    RoomEnterFailInvalidState = 6,
    RoomEnterFailInvalidRoomNumber = 7,
    RoomEnterFailAddUser = 8,
    PutStoneFailNotTurn = 9,
    PutStoneFailInvalidPosition = 10,
}