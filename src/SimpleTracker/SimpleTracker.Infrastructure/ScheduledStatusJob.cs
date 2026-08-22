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

	private readonly IPlayerSettingsProvider _playerSettingsProvider;

	public ScheduledStatusJob(IServiceProvider serviceProvider, ILog logger, ISettingsService settings, IPlayerSettingsProvider playerSettingsProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_settings = settings;
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
		TimeZoneInfo zone = ScheduleTimeZone.Resolve(_settings.ScheduleTimeZoneId, message => _logger.Log(message));
		_logger.Log($"[ScheduledStatusJob]:Scheduled with schedule:{_settings.StatusMessageCron} (timezone:{zone.Id})");
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await RunOnce(DateTime.UtcNow, cronExpression, zone, stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.Log("ERROR:[ScheduledStatusJob]:" + ex.Message);
			}
			await Task.Delay(60000, stoppingToken);
		}
	}

	private async Task RunOnce(DateTime utcNow, CronExpression cronExpression, TimeZoneInfo zone, CancellationToken stoppingToken)
	{
		DateTime localNow = ScheduleTimeZone.NowInZone(zone);
		if (!cronExpression.IsMatch(localNow))
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
		List<KeyValuePair<string, PlayerState>> roster = SortRosterByPayout(trackerState, utcNow);
		List<string> lines = new List<string>
		{
			"**Fleet payout order** (soonest first):"
		};
		foreach (KeyValuePair<string, PlayerState> item in roster)
		{
			PlayerState value = item.Value;
			settingsByAllyCode.TryGetValue(item.Key, out PlayerSettings value2);
			PlayerSettings playerSettings2 = value2 ?? new PlayerSettings
			{
				AllyCode = item.Key
			};
			Duration poTime = PoUtils.GetPoTime(value.TimezoneOffsetMinutes, Instant.FromDateTimeUtc(utcNow));
			MessageMap messageMap = Tracker.PopulateMessageMap(playerSettings2, value.PlayerName, value.PreviousRank, value.CurrentRank, poTime, _settings);
			lines.Add(MessageGenerator.GenerateStatusMessage(messageMap, _settings.MessageFormatOnStatus));
		}
		int enqueued = 0;
		foreach (string postBody in ChunkPost(lines))
		{
			DiscordMessage discordMessage = new DiscordMessage
			{
				DiscrodHookUrl = payoutWebHook,
				Message = postBody
			};
			if (!channel.Writer.TryWrite(discordMessage))
			{
				_logger.Log("Error: failed to enqueue discord message");
			}
			enqueued++;
		}
		trackerState.LastScheduledStatusPost = utcNow;
		_storage.Save(trackerState);
		_logger.Log($"[ScheduledStatusJob]:Enqueued roster post ({roster.Count} players, sorted by time to payout) as {enqueued} message(s).");
		await Task.CompletedTask;
	}

	public static List<KeyValuePair<string, PlayerState>> SortRosterByPayout(TrackerState state, DateTime utcNow)
	{
		return (from item in state.Players
			where item.Value != null && item.Value.CurrentRank > 0
			let poTime = PoUtils.GetPoTime(item.Value.TimezoneOffsetMinutes, Instant.FromDateTimeUtc(utcNow))
			orderby poTime.TotalMinutes, item.Value.CurrentRank
			select item).ToList();
	}

	private static IEnumerable<string> ChunkPost(IList<string> lines)
	{
		List<string> current = new List<string>();
		int length = 0;
		foreach (string line in lines)
		{
			if (current.Count > 0 && (current.Count >= 25 || length + line.Length + 1 > 1800))
			{
				yield return string.Join("\n", current);
				current.Clear();
				length = 0;
			}
			current.Add(line);
			length += line.Length + 1;
		}
		if (current.Count > 0)
		{
			yield return string.Join("\n", current);
		}
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
