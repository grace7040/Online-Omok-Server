namespace HiveAuthServer.Servicies;

public interface ILoginService
{
    public Task<(ErrorCode,string)> Login(string username, string password);
}
