using System;
using System.Threading;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace SimpleTracker.Infrastructure;

public class TrackerJob : BackgroundService
{
	private readonly Tracker _tracker;

	private readonly ILog _logger;

	private readonly int _pollIntervalMilliseconds;

	public TrackerJob(Tracker tracker, ILog logger, int pollIntervalSeconds)
	{
		_tracker = tracker;
		_logger = logger;
		_pollIntervalMilliseconds = pollIntervalSeconds * 1000;
		_logger.Log($"Poll interval: {pollIntervalSeconds} seconds.");
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				_tracker.Track();
			}
			catch (Exception ex)
			{
				_logger.Log("ERROR:" + ex.Message);
				_logger.Log("2 seconds sleep to retry");
			}
			await Task.Delay(_pollIntervalMilliseconds, stoppingToken);
		}
	}
}
