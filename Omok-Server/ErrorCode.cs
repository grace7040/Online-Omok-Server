using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ErrorCode : short
{
    NONE = 0,
    LOGIN_FULL_USER_COUNT = 1,
    ADD_USER_DUPLICATION = 2,
    REMOVE_USER_SEARCH_FAILURE_USER_ID = 3,
    LOGIN_ALREADY_WORKING = 4,
    ROOM_ENTER_INVALID_USER = 5,
    ROOM_ENTER_INVALID_STATE = 6,
    ROOM_ENTER_INVALID_ROOM_NUMBER = 7,
    ROOM_ENTER_FAIL_ADD_USER = 8,
    PUT_STONE_FAIL_NOT_TURN = 9,
}