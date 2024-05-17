using System;

// 1000 ~ 19999
public enum ErrorCode : UInt16
{
    None = 0,

    //9001~
    MatchingWait = 9001,
    AddUserToMatchingQueueFailException = 9002,
}