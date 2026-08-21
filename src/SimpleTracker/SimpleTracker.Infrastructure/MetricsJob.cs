using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SimpleTracker.Infrastructure;

public class MetricsJob : BackgroundService
{
	private readonly string _name;

	public MetricsJob(string name)
	{
		_name = name;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			TcpConnectionInformation[] activeTcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
			int workerThreads = 0;
			int completionPortThreads = 0;
			ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
			Console.WriteLine("---------------------");
			foreach (IGrouping<IPEndPoint, TcpConnectionInformation> group in from x in activeTcpConnections.ToList()
				group x by x.RemoteEndPoint)
			{
				(from c in @group.ToList()
					group c by c.State).ToList().ForEach(delegate(IGrouping<TcpState, TcpConnectionInformation> g)
				{
					Console.WriteLine($"{group.Key}:{g.Key}:{g.Count()}");
				});
			}
			await Task.Delay(3000, stoppingToken);
		}
	}
}
