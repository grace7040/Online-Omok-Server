using Hive_Auth_Server.Repositories;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMemoryDb, RedisDb>();
builder.Services.AddScoped<IHiveDb, MySqlDb>();

var app = builder.Build();

app.MapControllers();

app.Run();
