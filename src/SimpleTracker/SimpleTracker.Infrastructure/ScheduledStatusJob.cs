using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ipd.Core.Extensions;
using Ipd.Core.Interfaces;
using Ipd.Core.Messages;
using Ipd.Core.Models;
using Ipd.Core.Models.Discord;
using Ipd.Core.Services;
using Ipd.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using SimpleTracker.Services;

namespace SimpleTracker.Infrastructure;

public class ScheduledStatusJob : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;

	private readonly ILog _logger;

	private readonly ISettingsService _settings;

	private readonly IPersistentStorageService _storage;

	private readonly ArenaType _arenaType;

	private readonly IPlayerSettingsProvider _playerSettingsProvider;

	public ScheduledStatusJob(IServiceProvider serviceProvider, ILog logger, ISettingsService settings, ArenaType arenaType, IPlayerSettingsProvider playerSettingsProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_settings = settings;
		_arenaType = arenaType;
		_playerSettingsProvider = playerSettingsProvider;
		_storage = new FileStorageService(settings.StorageFilePath, logger);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (string.IsNullOrEmpty(_settings.StatusMessageCron))
		{
			_logger.Log("[ScheduledStatusJob]:STATUS_MESSAGE_CRON not set. Scheduled roster posts are off.");
			return;
		}
		CronExpression cronExpression = CronExpression.ParseSchedule(_settings.StatusMessageCron);
		_logger.Log("[ScheduledStatusJob]:Scheduled with schedule:" + _settings.StatusMessageCron);
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await RunOnce(DateTime.UtcNow, cronExpression, stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.Log("ERROR:[ScheduledStatusJob]:" + ex.Message);
			}
			await Task.Delay(60000, stoppingToken);
		}
	}

	private async Task RunOnce(DateTime utcNow, CronExpression cronExpression, CancellationToken stoppingToken)
	{
		if (!cronExpression.IsMatch(utcNow))
		{
			return;
		}
		DateTime minuteStart = utcNow.AddSeconds(-utcNow.Second).AddMilliseconds(-utcNow.Millisecond);
		TrackerState trackerState = _storage.Load();
		if (trackerState.LastScheduledStatusPost.HasValue && trackerState.LastScheduledStatusPost.Value >= minuteStart)
		{
			return;
		}
		string payoutWebHook = GetPayoutWebHook();
		IList<PlayerSettings> playerSettings = await _playerSettingsProvider.GetPlayerSettingAsync();
		Dictionary<string, PlayerSettings> settingsByAllyCode = playerSettings.GroupBy((PlayerSettings p) => p.AllyCode.NormalizeAllyCode()).ToDictionary((IGrouping<string, PlayerSettings> g) => g.Key, (IGrouping<string, PlayerSettings> g) => g.First());
		Channel<DiscordMessage> channel = _serviceProvider.GetRequiredService<Channel<DiscordMessage>>();
		int num = 0;
		foreach (KeyValuePair<string, PlayerState> item in trackerState.Players)
		{
			PlayerState value = item.Value;
			if (value == null || value.CurrentRank <= 0)
			{
				continue;
			}
			settingsByAllyCode.TryGetValue(item.Key, out PlayerSettings value2);
			PlayerSettings playerSettings2 = value2 ?? new PlayerSettings
			{
				AllyCode = item.Key
			};
			Duration poTime = PoUtils.GetPoTime(value.TimezoneOffsetMinutes, _arenaType, Instant.FromDateTimeUtc(utcNow));
			MessageMap messageMap = Tracker.PopulateMessageMap(playerSettings2, value.PlayerName, value.PreviousRank, value.CurrentRank, poTime, _settings);
			string textMessage = MessageGenerator.GenerateStatusMessage(messageMap, _settings.MessageFormatOnStatus);
			DiscordMessage discordMessage = new DiscordMessage
			{
				DiscrodHookUrl = payoutWebHook,
				Message = textMessage
			};
			if (!channel.Writer.TryWrite(discordMessage))
			{
				_logger.Log("Error: failed to enqueue discord message");
			}
			num++;
		}
		trackerState.LastScheduledStatusPost = utcNow;
		_storage.Save(trackerState);
		_logger.Log($"[ScheduledStatusJob]:Enqueued {num} roster status messages.");
		await Task.CompletedTask;
	}

	private string GetPayoutWebHook()
	{
		string payoutWebHookUrl = _settings.PayoutWebHookUrl;
		if (!string.IsNullOrWhiteSpace(payoutWebHookUrl))
		{
			return payoutWebHookUrl.Trim();
		}
		return (Environment.GetEnvironmentVariable(EnvSettingsService.DISCORD_WEB_HOOK) ?? "").Trim();
	}
}
