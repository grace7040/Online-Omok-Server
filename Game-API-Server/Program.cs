using Game_API_Server.Middleware;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseMiddleware<CheckAuthMiddleware>();   //유저 토큰 - redis 토큰 체크

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
