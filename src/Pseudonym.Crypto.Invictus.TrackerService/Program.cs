using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Pseudonym.Crypto.Invictus.TrackerService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((c, builder) => builder
                    .AddUserSecrets<Program>(true)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{c.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables())
                .ConfigureWebHostDefaults(webHost => webHost
                    .UseStartup<Startup>()
                    .ConfigureKestrel(options => options.AddServerHeader = false)
                    .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .SuppressStatusMessages(true))
                .Build()
                .RunAsync();
        }
    }
}
