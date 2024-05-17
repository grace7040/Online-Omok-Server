using Game_API_Server.DTOs;

namespace Game_API_Server.Services
{
    //Matching API Server를 사용한 FIFO 방식의 매칭
    public class FIFOMatchService : IMatchMakingService
    {
        public async Task<ResMatchingDTO> TryGetUserMatchingInfo(string email)
        {
            var matchingServerUrl = "http://127.0.0.1:11502/matching"; // ::TODO:: config에서 받아오게 수정
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(matchingServerUrl, new { UserID = email });
            var responseContent = response.Content.ReadFromJsonAsync<ResMatchingDTO>();
            var userMatchingInfo = responseContent.Result;

            return userMatchingInfo;
        }
    }
}
