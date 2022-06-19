using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Synthesis_Server
{
    class Program
    {
        
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();

                if (args != null)
                {
                    foreach (string x in args)
                    {
                        Console.WriteLine(x);
                    }
                    config.AddCommandLine(args);
                }
            });

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Daemon"));
                services.AddSingleton<IHostedService, DaemonService>();
            });

            builder.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            await builder.RunConsoleAsync();

            Console.WriteLine("Hello World!");
        }

    }
}
