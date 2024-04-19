public interface IHiveDb
{
    public Task<ErrorCode> InsertAccountAsync(string email, string password);
    public Task<string> GetPasswordByEmailAsync(string email);
}