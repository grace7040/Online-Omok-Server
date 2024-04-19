using Hive_Auth_Server.Repositories;
using Hive_Auth_Server.Servicies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMemoryDb, RedisDb>();
builder.Services.AddScoped<IHiveDb, MySqlDb>();
builder.Services.AddSingleton<IHasher, HasherSHA256>();

var app = builder.Build();

app.MapControllers();

app.Run();
