public interface IGameDb
{
    public Task<ErrorCode> InsertAccountAsync(string id);

    public Task<bool> IsUserEmailExistAsync(string email);
}