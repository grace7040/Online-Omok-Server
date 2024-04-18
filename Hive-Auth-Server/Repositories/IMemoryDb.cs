
public interface IMemoryDb
{
    public Task<ErrorCode> RegistUserAsync(string id, string authToken, TimeSpan expiry);

    public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);
}

