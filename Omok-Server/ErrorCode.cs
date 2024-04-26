using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ErrorCode : UInt16
{
    NONE = 0,
    LOGIN_FULL_USER_COUNT = 1,
    ADD_USER_DUPLICATION = 2,
    REMOVE_USER_SEARCH_FAILURE_USER_ID = 3,
    LOGIN_ALREADY_WORKING = 4,
}