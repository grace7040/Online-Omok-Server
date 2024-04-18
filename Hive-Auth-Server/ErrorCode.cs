public enum ErrorCode : UInt16
{
    None = 0,

    LoginFailRegistRedis,
    LoginFailWrongPassword,

    CheckUserAuthFailNotMatch,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,
    

}

