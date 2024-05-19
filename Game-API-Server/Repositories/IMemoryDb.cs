
public interface IMemoryDb
{
    public Task<ErrorCode> RegistUserAuthAsync(string id, string authToken, TimeSpan expiry);
    public Task<ErrorCode> RemoveUserAuthAsync(string id);

    public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);

    public Task<int> TryGetUserRoomNumberAsync(string id);


    //for SimpleMatchingService...
    public Task<ErrorCode> AddUserToMatchingQueueAsync(string id);

    public Task<bool> IsUserInMatchingQueue(string id);

    public Task<int> GetMatchingQueueSizeAsync();

    public Task<(string, string)> GetMatchedUsers();
    public Task<int> GetRoomNumberAsync();

    public void AddUserRoomNumber(string id, int roomNumber);

    public void RemoveUserFromMatchingQueue(string id);
}

