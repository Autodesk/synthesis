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
            string filePath = "appsettings.json";
            bool createConfig = false;
            var builder = new HostBuilder();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }

                //configure build to use local json file
                try
                {
                    config.AddJsonFile(filePath);
                } catch (System.IO.FileNotFoundException e)
                {
                    createConfig = true;
                }
                
            });

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();

                foreach (var x in hostContext.Configuration.AsEnumerable())
                {
                    Console.WriteLine(x);
                }
                //Configure commands and config file loading here
                //services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Daemon"));
                services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Settings"));
                services.AddSingleton<IHostedService, SynthesisService>();
            });

            builder.ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            var host = builder.Build();
            
            if (createConfig)
            {
                //create json config using current host configuration
            }
            
            await host.RunAsync();
            //await builder.RunConsoleAsync();

        }

    }
}
