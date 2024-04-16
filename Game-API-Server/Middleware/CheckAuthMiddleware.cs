using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Game_API_Server.Middleware
{
    public class CheckAuthMiddleware
    {
        IConfiguration _configuration;
        RequestDelegate _next;
        ConnectionMultiplexer _redisConnection;
        IDatabase _redisDb;

        //유저 요청시, 유저가 보낸 토큰값과 redis에 저장된 토큰값 비교.
        //다른 경우 short-cut
        public CheckAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;

            var redisConnectString = _configuration.GetConnectionString("GameRedis");
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectString);
            _redisDb = _redisConnection.GetDatabase();
        }

        // 정상적인 유저가 게임 서버에 접속할 때는, 항상 하이브로부터 받은 토큰을 가지고 있는 상태일 것임. 
        // 따라서 미들웨어로 일괄 적용 가능
        // :: TODO :: 내용들 분리하고, 비동기 처리하기
        public async Task Invoke(HttpContext context)
        {
            //email, token값 파싱. 
            // :: TODO :: 널(string.Empty)인 경우 바로 반환 처리
            string headerEmail = context.Request.Headers["email"].ToString();
            string headerToken = context.Request.Headers["token"].ToString();

            //토큰이 유효한지 검사(to redis)
            // :: TODO :: 이건 널인 경우 어떻게 처리하지?
            string token = _redisDb.StringGet(headerEmail);

            //유효하지 않은 경우, 400 반환
            if (token == null || !token.Equals(headerToken))
            {
                context.Response.StatusCode = 400;
                return;
            }

            await _next(context);
        }
    }
}
