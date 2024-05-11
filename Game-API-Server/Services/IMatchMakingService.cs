namespace Game_API_Server.Services
{
    public interface IMatchMakingService
    {
        Task<int> TryGetUserRoomNumber(string id);
        Task<ErrorCode> StartUserMatching(string id);

        void UpdateMatchingState();
    }
}
