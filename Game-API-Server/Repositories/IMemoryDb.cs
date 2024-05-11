
public interface IMemoryDb
{
    public Task<ErrorCode> RegistUserAsync(string id, string authToken, TimeSpan expiry);

    public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);

    public Task<int> TryGetUserRoomNumberAsync(string id);

    public Task<ErrorCode> AddUserToMatchingQueueAsync(string id);

    public Task<bool> IsUserInMatchingQueue(string id);

    public Task<int> GetMatchingQueueSizeAsync();

    public Task<(string, string)> GetMatchedUsers();
    public Task<int> GetRoomNumberAsync();

    public void AddUserRoomNumber(string id, int roomNumber);

    public void RemoveUserFromMatchingQueue(string id);
}

