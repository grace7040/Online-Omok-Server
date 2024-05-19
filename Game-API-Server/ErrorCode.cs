
// 3001 ~ 4000
public enum ErrorCode : UInt16
{
    None = 0,

    CreateAccountFailInsertAccount = 3001,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,

    LoginFailOnHive = 3101,
    LoginFailRegistRedis,
    LoginFailInsertDB,
    RemoveUserFailException,

    CheckUserAuthFailNotMatch = 3201,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,

    MatchingWait = 3301,
    AddUserToMatchingQueueFailException,
    MatchingReqFailException,
    
}

