using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ipd.Core.Extensions;
using Ipd.Core.Interfaces;
using Ipd.Core.Models.Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace Ipd.Core.Jobs;

public class DiscordMessengerJob : BackgroundService
{
	private readonly ILogger<DiscordMessengerJob> _logger;

	private readonly IServiceProvider _serviceProvider;

	private const int MaxMessagesInChannel = 10;

	private const int DiscordMessageBatchSize = 10;

	private readonly Channel<DiscordMessage> _channel;

	public DiscordMessengerJob(ILogger<DiscordMessengerJob> logger, Channel<DiscordMessage> channel, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_channel = channel;
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				INewDiscordMessenger discordMessenger = _serviceProvider.GetRequiredService<INewDiscordMessenger>();
				IEnumerable<IGrouping<string, DiscordMessage>> enumerable = from m in await _channel.Reader.ToListAsync(stoppingToken)
					group m by m.DiscrodHookUrl;
				foreach (IGrouping<string, DiscordMessage> item in enumerable)
				{
					string webHookUrl = item.First().DiscrodHookUrl;
					IEnumerable<IEnumerable<DiscordMessage>> enumerable2 = item.Batch(10);
					foreach (IEnumerable<DiscordMessage> item2 in enumerable2)
					{
						string textMessage = string.Join('\n', item2.Select((DiscordMessage m) => m.Message.Trim()));
						try
						{
							await discordMessenger.SendTextMessage(webHookUrl, textMessage);
						}
						catch (Exception ex)
						{
							_logger.LogError("Exception", ex);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				_logger.LogError("Exception", ex2);
			}
			await Task.Delay(1000, stoppingToken);
		}
	}
}
