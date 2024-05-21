namespace GameAPIServer.Services;

public interface ILoginService
{
    public Task<ErrorCode> Login(string id, string token);

    public Task<ErrorCode> Logout(string id);
}
