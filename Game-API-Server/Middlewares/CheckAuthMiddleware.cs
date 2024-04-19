using Game_API_Server.Services;
using System.Net;

namespace Game_API_Server.Middleware
{
    public class CheckAuthMiddleware
    {
        IConfiguration _configuration;
        ICheckAuthService _checkAuthService;
        RequestDelegate _next;
        

        //유저 요청시, 유저가 보낸 토큰값과 redis에 저장된 토큰값 비교.
        //다른 경우 short-cut
        public CheckAuthMiddleware(RequestDelegate next, IConfiguration configuration, ICheckAuthService checkAuthService)
        {
            _next = next;
            _configuration = configuration;
            _checkAuthService = checkAuthService;
        }

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

                bool isAuthed = await _checkAuthService.CheckAuthToMemoryDbAsync(headerEmail, headerToken);
                if (!isAuthed)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                await _next(context);
            }
            return;


        }
    }
}
