
// 1001 ~ 2000
public enum ErrorCode : UInt16
{
    None = 0,

    CreateAccountFailInsertAccount = 1001,
    CreateAccountFailAlreadyExist,
    CreateAccountFailException,

    LoginFailRegistRedis=1101,
    LoginFailWrongPassword,
    LoginFailNotExistId,

    CheckUserAuthFailNotMatch= 1201,
    CheckUserAuthFailNotExist,
    CheckUserAuthFailException,

}

