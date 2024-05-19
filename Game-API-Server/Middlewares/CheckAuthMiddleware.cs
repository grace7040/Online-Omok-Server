using Game_API_Server.Services;
using System.Net;

namespace Game_API_Server.Middleware
{
    public class CheckAuthMiddleware
    {
        ICheckAuthService _checkAuthService;
        RequestDelegate _next;
        

        public CheckAuthMiddleware(RequestDelegate next, ICheckAuthService checkAuthService)
        {
            _next = next;
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
                var id = context.Request.Headers["Id"].ToString();
                var token = context.Request.Headers["token"].ToString();

                var isAuthed = await _checkAuthService.CheckAuthToMemoryDbAsync(id, token);
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
