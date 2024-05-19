public interface IHiveDb : IDisposable
{
    public Task<ErrorCode> InsertAccountAsync(string id, string password);
    public Task<string> GetPasswordByIdAsync(string id);
}