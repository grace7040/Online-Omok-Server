using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

using Omok_Server;
using SuperSocket.SocketBase.Protocol;

class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureLogging(logging =>
            {
                //로깅 구성
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole();
            })
            .ConfigureServices((hostContext, services) =>
            {
                //서비스 구성
                services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                services.Configure<DbOption>(hostContext.Configuration.GetSection("DbOption"));
                services.Configure<RedisOption>(hostContext.Configuration.GetSection("RedisOption"));
                services.AddHostedService<MainServer>();    
            })
            .Build();

        await host.RunAsync();
    }
}

