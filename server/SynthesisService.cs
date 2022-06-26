using System;
using System.Linq;
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
			Parser.Default.ParseArguments<StartCommand, StopCommand, RestartCommand>(_config.Value.Arguments).MapResult(
				(StartCommand opts) => StartServer(opts),
				(StopCommand opts) => StopServer(opts),
				(RestartCommand opts) => RestartServer(opts),
				errs => (int)Command.INVALID);

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
			_logger.LogInformation("Starting Server");
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
			{
				_logger.LogInformation("An Instance of daemon: " + _config.Value.DaemonName + " is already running");
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			return (int)Command.START;
		}

		private int StopServer(StopCommand cmd)
		{
			_logger.LogInformation("Stopping Server");
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
			{
				foreach (var x in System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)))
				{
					x.Kill();
				}
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			return (int)Command.STOP;
		}

		private int RestartServer(RestartCommand cmd)
		{
			_logger.LogInformation("Restarting Server");
			return (int)Command.RESTART;
		}
	}
}