namespace HiveAuthServer.Servicies;

public class LoginService: ILoginService
{
    IMemoryDb _memoryDb;
    IHiveDb _hiveDb;
    IHasher _hasher;
    ITokenCreator _tokenCreator;

    public LoginService(IMemoryDb memoryDb, IHiveDb hiveDb, IHasher hasher, ITokenCreator tokenCreater)
    {
        _memoryDb = memoryDb;
        _hiveDb = hiveDb;
        _hasher = hasher;
        _tokenCreator = tokenCreater;
    }

    public async Task<(ErrorCode, string)> Login(string id, string password)
    {
        //비밀번호 검증
        var isRightPassword = await VerifyPassword(id, password);
        if (!isRightPassword)
        {
            return (ErrorCode.LoginFailWrongPassword, null);
        }

        //Token 랜덤 생성 및 Redis에 등록
        var token = _tokenCreator.CreateAuthToken();
        var result = await _memoryDb.RegistUserAsync(id, token, Expiries.LoginToken);
        if(result != ErrorCode.None)
        {
            return (result, null);
        }

        return (ErrorCode.None, token);
    }

    async Task<bool> VerifyPassword(string id, string password)
    {
        var passwordFromRequest = _hasher.GetHashedString(password);
        var passwordFromDb = await _hiveDb.GetPasswordByIdAsync(id);

        return passwordFromRequest == passwordFromDb;
    }
}
