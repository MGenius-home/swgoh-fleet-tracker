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

	public TrackerJob(Tracker tracker, ILog logger)
	{
		_tracker = tracker;
		_logger = logger;
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
			await Task.Delay(2000, stoppingToken);
		}
	}
}
