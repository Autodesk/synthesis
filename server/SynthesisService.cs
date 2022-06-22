using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SynthesisServer
{
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
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
			{
				_logger.LogInformation("An Instance of daemon: " + _config.Value.DaemonName + " is already running");
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			_logger.LogInformation("Starting daemon: " + _config.Value.DaemonName);
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

		public void StartServer()
		{
			throw new NotImplementedException();
		}

		public void StopServer()
		{
			throw new NotImplementedException();
		}

		public void RestartServer()
		{
			throw new NotImplementedException();
		}
	}
}