
public interface IMemoryDb
{
    public Task<ErrorCode> RegistUserAsync(string email, string authToken, TimeSpan expiry);

    public Task<ErrorCode> CheckUserAuthAsync(string email, string authToken);
}

