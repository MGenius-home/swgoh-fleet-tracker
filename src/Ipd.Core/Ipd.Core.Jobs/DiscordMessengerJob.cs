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

namespace Ipd.Core.Jobs;

public class DiscordMessengerJob : BackgroundService
{
	private readonly ILogger<DiscordMessengerJob> _logger;

	private readonly IServiceProvider _serviceProvider;

	private const int MaxMessagesInChannel = 10;

	private const int DiscordMessageBatchSize = 25;

	private const int MaxBatchCharacterLength = 1800;

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
					foreach (DiscordMessage item2 in item.Where((DiscordMessage m) => m.Embed != null))
					{
						try
						{
							await discordMessenger.SendEmbedMessage(webHookUrl, item2.Embed);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Exception");
						}
					}
					List<DiscordMessage> textMessages = item.Where((DiscordMessage m) => m.Embed == null).ToList();
					List<string> batch = new List<string>();
					int batchLength = 0;
					foreach (DiscordMessage message in textMessages)
					{
						string trimmed = message.Message.Trim();
						if (batch.Count > 0 && (batch.Count >= DiscordMessageBatchSize || batchLength + trimmed.Length > MaxBatchCharacterLength))
						{
							await SendTextBatch(discordMessenger, webHookUrl, batch);
							batch.Clear();
							batchLength = 0;
						}
						batch.Add(trimmed);
						batchLength += trimmed.Length + 1;
					}
					if (batch.Count > 0)
					{
						await SendTextBatch(discordMessenger, webHookUrl, batch);
					}
				}
			}
			catch (Exception ex3)
			{
				_logger.LogError(ex3, "Exception");
			}
			await Task.Delay(1000, stoppingToken);
		}
	}

	private static async Task SendTextBatch(INewDiscordMessenger discordMessenger, string webHookUrl, List<string> batch)
	{
		string textMessage = string.Join('\n', batch);
		try
		{
			await discordMessenger.SendTextMessage(webHookUrl, textMessage);
			Console.WriteLine($"[DiscordMessengerJob]:Sent batch of {batch.Count} message(s) ({textMessage.Length} chars).");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[DiscordMessengerJob]:Failed to send batch of {batch.Count} messages:{ex.Message}");
		}
	}
}
