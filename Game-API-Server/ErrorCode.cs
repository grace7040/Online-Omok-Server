public enum ErrorCode : UInt16
{
    None = 0,

    CreateAccountFailInsertAccount,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,

    LoginFailOnHive,
    LoginFailRegistRedis,
    LoginFailInsertDB,

    CheckUserAuthFailNotMatch,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException

}

