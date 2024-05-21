namespace HiveAuthServer.Servicies;

public interface ICreateAccountService
{
    public Task<ErrorCode> CreateAccount(string id, string password);
}
