using GameAPIServer.DTOs;
namespace GameAPIServer.Services
{
    public interface IMatchMakingService
    {
        Task<ResMatchingDTO> TryGetUserMatchingInfo(string id);

        Task<ResponseDTO> RequestCancelMatching(string id);
    }
}
