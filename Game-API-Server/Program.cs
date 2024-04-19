using Game_API_Server.Middleware;
using Game_API_Server.Services;
using Game_API_Server.Repositories;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMemoryDb, RedisDb>();
builder.Services.AddScoped<IGameDb, MySqlDb>();
builder.Services.AddSingleton<ICheckAuthService, CheckAuthService>();


SettingLogger();

var app = builder.Build();

//유저 토큰 - redis 토큰 체크
app.UseMiddleware<CheckAuthMiddleware>();

app.MapControllers();

app.Run();


void SettingLogger()
{
    ILoggingBuilder logging = builder.Logging;
    _ = logging.ClearProviders();

    string fileDir = builder.Configuration["logdir"];

    bool exists = Directory.Exists(fileDir);

    if (!exists)
    {
        _ = Directory.CreateDirectory(fileDir);
    }

    _ = logging.AddZLoggerRollingFile(
        options =>
        {
            options.UseJsonFormatter();
            options.FilePathSelector = (timestamp, sequenceNumber) => $"{fileDir}{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";
            options.RollingInterval = ZLogger.Providers.RollingInterval.Day;
            options.RollingSizeKB = 1024;
        });

    _ = logging.AddZLoggerConsole(options =>
    {
        options.UseJsonFormatter();
    });

}