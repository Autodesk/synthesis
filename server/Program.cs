using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SynthesisServer
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
                    config.AddCommandLine(args);
                }

                //configure build to use local json file
                //config.AddJsonFile("");
            });

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();

                //Configure commands and config file loading here
                //services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Daemon"));
                services.Configure<DaemonConfig>(hostContext.Configuration);
                services.AddSingleton<IHostedService, SynthesisService>();
            });

            builder.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            var host = builder.Build();
            
            
            await host.RunAsync();
            //await builder.RunConsoleAsync();

        }

    }
}
