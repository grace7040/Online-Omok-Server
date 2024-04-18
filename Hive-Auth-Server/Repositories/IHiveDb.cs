public interface IHiveDb
{
    public Task<ErrorCode> InsertAccountAsync(string id, string password);
    public Task<string> GetPasswordByEmailAsync(string email);
}