using HiveAuthServer.Controllers;
using System.Security.Principal;

namespace HiveAuthServer.Servicies;

public class CreateAccountService : ICreateAccountService
{
    IHiveDb _hiveDb;
    IHasher _hasher;

    public CreateAccountService(IHiveDb hiveDb, IHasher hasher)
    {
        _hiveDb = hiveDb;
        _hasher = hasher;
    }

    public async Task<ErrorCode> CreateAccount(string id, string password)
    {
        var hashedPassword = _hasher.GetHashedString(password);
        var result = await _hiveDb.InsertAccountAsync(id, hashedPassword);
        return result;
    }
}
