public interface IGameDb : IDisposable
{
    public Task<ErrorCode> InsertAccountAsync(string id);

    public Task<bool> IsUserIdExistAsync(string id);
}