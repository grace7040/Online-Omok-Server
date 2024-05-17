using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Matching_Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<MatchingConfig>(configuration.GetSection(nameof(MatchingConfig)));


builder.Services.AddSingleton<IMatchWorker, MatchWorker>();

builder.Services.AddControllers();
WebApplication app = builder.Build();

app.MapDefaultControllerRoute();


app.Run(configuration["ServerAddress"]);


