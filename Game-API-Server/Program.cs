using Game_API_Server.Middleware;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


var app = builder.Build();

//유저 토큰 - redis 토큰 체크
app.UseMiddleware<CheckAuthMiddleware>();

app.MapControllers();

app.Run();
