using Matching_Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<MatchingConfig>(configuration.GetSection(nameof(MatchingConfig)));

builder.Services.AddSingleton<IMatchingWorker, MatchingWorker>();

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();

app.Run();


