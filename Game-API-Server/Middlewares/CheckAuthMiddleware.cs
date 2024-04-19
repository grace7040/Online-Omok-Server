namespace Game_API_Server.Middleware
{
    public class CheckAuthMiddleware
    {
        IConfiguration _configuration;
        RequestDelegate _next;
        IMemoryDb _memoryDb;

        //유저 요청시, 유저가 보낸 토큰값과 redis에 저장된 토큰값 비교.
        //다른 경우 short-cut
        public CheckAuthMiddleware(RequestDelegate next, IConfiguration configuration, IMemoryDb memoryDb)
        {
            _next = next;
            _configuration = configuration;
            _memoryDb = memoryDb;
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
                string headerEmail = context.Request.Headers["email"].ToString();
                string headerToken = context.Request.Headers["token"].ToString();

                //토큰이 유효한지 검사(to redis)
                ErrorCode redisResult = await _memoryDb.CheckUserAuthAsync(headerEmail, headerToken);
                if(redisResult != ErrorCode.None)
                {
                    // :: TODO :: 로깅
                    context.Response.StatusCode = 400;
                    return;
                }

                await _next(context);
            }
            return;


        }
    }
}
