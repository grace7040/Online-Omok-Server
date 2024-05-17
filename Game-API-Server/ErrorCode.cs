public enum ErrorCode : UInt16
{
    None = 0,

    //6001~
    CreateAccountFailInsertAccount = 6001,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,


    //7001~
    LoginFailOnHive = 7001,
    LoginFailRegistRedis,
    LoginFailInsertDB,

    //8001~
    CheckUserAuthFailNotMatch = 8001,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,

    //9001~
    MatchingWait = 9001,
    AddUserToMatchingQueueFailException = 9002,
    MatchingReqFailException = 9003,
}

