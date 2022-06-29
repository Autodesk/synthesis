using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SynthesisServer
{
	public enum Command
	{
		INVALID = -1,
		START = 0,
		STOP = 1,
		RESTART = 2
	}
	public class SynthesisService : IHostedService, IDisposable
	{

		private readonly ILogger _logger;
		private readonly IOptions<DaemonConfig> _config;

		public SynthesisService(ILogger<SynthesisService> logger, IOptions<DaemonConfig> config)
		{
			_logger = logger;
			_config = config;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			int status = Parser.Default.ParseArguments<StartCommand, StopCommand, RestartCommand>(_config.Value.Arguments).MapResult(
				(StartCommand opts) => StartServer(opts),
				(StopCommand opts) => StopServer(opts),
				(RestartCommand opts) => RestartServer(opts),
				errs => InvalidCommand());

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping daemon.");
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("Disposing...");
		}

		private int StartServer(StartCommand cmd)
		{
			
			if (!cmd.Force && Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
			{
				_logger.LogInformation("An instance of: " + _config.Value.DaemonName + " is already running");
				Environment.Exit(0);
			} else
            {
				_logger.LogInformation("Starting Server");
			}
			return (int)Command.START;
		}

		private int StopServer(StopCommand cmd)
		{
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);

			foreach (Process x in processes)
			{
				if (x.Id != current.Id)
				{
					x.Kill();
				}
			}

			Environment.Exit(0);
			return (int)Command.STOP;
		}

		private int RestartServer(RestartCommand cmd)
		{
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);

			foreach (Process x in processes)
			{
				if (x.Id != current.Id)
				{
					x.Kill();
				}
			}
			long time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
			while (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1 && System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - time < cmd.Timeout)
            {
				Thread.Sleep(100);
            }
			return StartServer(new StartCommand() { Force = cmd.Force });
		}

		private int InvalidCommand()
        {
			_logger.LogError("Invalid command");
			Environment.Exit(0);
			return (int)Command.INVALID;
		}
	}
}