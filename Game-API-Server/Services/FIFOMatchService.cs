using Game_API_Server.DTOs;

namespace Game_API_Server.Services
{
    //Matching API Server를 사용한 FIFO 방식의 매칭
    public class FIFOMatchService : IMatchMakingService
    {
        IConfiguration _config;
        string _matchingServerUrl;
        public FIFOMatchService(IConfiguration config)
        {
            _config = config;
            _matchingServerUrl = _config.GetConnectionString("MatchingServer");
        }
        public async Task<ResMatchingDTO> TryGetUserMatchingInfo(string email)
        {
            var matchingUrl = _matchingServerUrl + "/matching"; 
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(matchingUrl, new { UserID = email });
            var responseContent = response.Content.ReadFromJsonAsync<ResMatchingDTO>();
            var userMatchingInfo = responseContent.Result;

            return userMatchingInfo;
        }

        public async Task<ResponseDTO> RequestCancelMatching(string email)
        {
            var cancelMatchingUrl = _matchingServerUrl + "/cancelmatching"; 
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(cancelMatchingUrl, new { UserID = email });
            var responseContent = response.Content.ReadFromJsonAsync<ResponseDTO>();
            var userMatchingInfo = responseContent.Result;

            return userMatchingInfo;
        }
    }
}
