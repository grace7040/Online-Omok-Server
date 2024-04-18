public enum ErrorCode : UInt16
{
    None = 0,

    CreateAccountFailInsertAccount,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,

    LoginFailRegistRedis,
    LoginFailWrongPassword,
    LoginFailNotExistEmail,

    CheckUserAuthFailNotMatch,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,
    

}

