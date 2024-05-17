using Game_API_Server.DTOs;
namespace Game_API_Server.Services
{
    public interface IMatchMakingService
    {
        Task<ResMatchingDTO> TryGetUserMatchingInfo(string id);

        Task<ResponseDTO> RequestCancelMatching(string email);
    }
}
