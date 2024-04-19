using Game_API_Server.Middleware;
using Game_API_Server.Services;
using Hive_Auth_Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMemoryDb, RedisDb>();
builder.Services.AddScoped<IGameDb, MySqlDb>();
builder.Services.AddSingleton<ICheckAuthService, CheckAuthService>();


var app = builder.Build();

//유저 토큰 - redis 토큰 체크
app.UseMiddleware<CheckAuthMiddleware>();

app.MapControllers();

app.Run();
