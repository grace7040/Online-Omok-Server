public enum ErrorCode : UInt16
{
    None = 0,

    //1001~
    CreateAccountFailInsertAccount = 1001,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,

    //2001~
    LoginFailRegistRedis=2001,
    LoginFailWrongPassword,
    LoginFailNotExistEmail,

    //3001~
    CheckUserAuthFailNotMatch=3001,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,

}

