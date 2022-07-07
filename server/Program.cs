using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;

namespace SynthesisServer
{

    class Program
    {
        
        public static async Task Main(string[] args)
        {

            string filePath = "appsettings.json";
            var builder = new HostBuilder();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
                config.AddJsonFile(filePath, true);

            });

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();

                services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Settings"));
                services.Configure<DaemonConfig>(config => { config.Arguments = args; });
                services.AddSingleton<IHostedService, SynthesisService>();
            });

            builder.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            var host = builder.Build();
           
            
            await host.RunAsync();
            
        }

    }
}
