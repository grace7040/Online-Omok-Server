using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Game_API_Server.Middleware
{
    public class CheckAuthMiddleware
    {
        IConfiguration _configuration;
        RequestDelegate _next;
        RedisConnection _redisConnection;

        //유저 요청시, 유저가 보낸 토큰값과 redis에 저장된 토큰값 비교.
        //다른 경우 short-cut
        public CheckAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;

            var redisConnectString = configuration.GetConnectionString("GameRedis");
            var redisConfig = new RedisConfig("GameRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);
        }


        // :: TODO :: 서비스 단위로 분리하고, 비동기 처리하기
        public async Task Invoke(HttpContext context)
        {
            if(context.Request.Path.Value == "/login")
            {
                await _next(context);
            }
            else
            {
                //email, token값 파싱. 
                // :: TODO :: 널(string.Empty)인 경우 바로 반환 처리
                string headerEmail = context.Request.Headers["email"].ToString();
                string headerToken = context.Request.Headers["token"].ToString();

                //토큰이 유효한지 검사(to redis)
                // :: TODO :: 이건 널인 경우 어떻게 처리하지?
                var query = new RedisString<string>(_redisConnection, headerEmail, null);
                var token = query.GetAsync().Result.Value;

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
}
