using IdentityServer.Data.Initializer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            new SeedData().EnsureSeedDataAsync(host.Services).Wait();

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) => logging.ClearProviders())
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
