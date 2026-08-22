using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Messages;
using Ipd.Core.Models;
using Ipd.Core.Models.Discord;
using Ipd.Core.Services;
using Ipd.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTracker.Services;

namespace SimpleTracker.Infrastructure;

public class WeeklyAttackSummaryJob : BackgroundService
{
	private const int SUMMARY_EMBED_COLOR = 0x5865F2;

	private const int MAX_EMBED_DESCRIPTION_LENGTH = 4096;

	private readonly IServiceProvider _serviceProvider;

	private readonly ILog _logger;

	private readonly ISettingsService _settings;

	private readonly IPersistentStorageService _storage;

	private readonly IAttackTrackerService _attackTracker;

	public WeeklyAttackSummaryJob(IServiceProvider serviceProvider, ILog logger, ISettingsService settings)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_settings = settings;
		_storage = new FileStorageService(settings.StorageFilePath, logger);
		_attackTracker = new AttackTrackerService(_storage, new PayoutService());
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_settings.IsWeeklyAttackSummaryEnabled)
		{
			_logger.Log("[WeeklyAttackSummaryJob]:ENABLE_WEEKLY_ATTACK_SUMMARY not set to TRUE. Weekly summary is off.");
			return;
		}
		CronExpression cronExpression;
		try
		{
			cronExpression = CronExpression.ParseSchedule(_settings.WeeklyAttackSummaryCron);
		}
		catch (Exception ex)
		{
			_logger.Log($"[WeeklyAttackSummaryJob]:Invalid schedule '{_settings.WeeklyAttackSummaryCron}':{ex.Message} Weekly summary is disabled.");
			return;
		}
		TimeZoneInfo zone = ScheduleTimeZone.Resolve(_settings.ScheduleTimeZoneId, message => _logger.Log(message));
		_logger.Log($"[WeeklyAttackSummaryJob]:Scheduled with schedule:{_settings.WeeklyAttackSummaryCron} (timezone:{zone.Id})");
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await RunOnce(DateTime.UtcNow, cronExpression, zone);
			}
			catch (Exception ex)
			{
				_logger.Log("ERROR:[WeeklyAttackSummaryJob]:" + ex.Message);
			}
			await Task.Delay(60000, stoppingToken);
		}
	}

	private async Task RunOnce(DateTime utcNow, CronExpression cronExpression, TimeZoneInfo zone)
	{
		DateTime localNow = ScheduleTimeZone.NowInZone(zone);
		if (!cronExpression.IsMatch(localNow))
		{
			return;
		}
		DateTime value = utcNow.AddSeconds(-utcNow.Second).AddMilliseconds(-utcNow.Millisecond);
		TrackerState trackerState = _storage.Load();
		if (trackerState.LastWeeklySummaryPost.HasValue && trackerState.LastWeeklySummaryPost.Value >= value)
		{
			return;
		}
		DiscordEmbed discordEmbed = BuildSummaryEmbed(_attackTracker.GetWeeklySummary());
		string payoutWebHook = GetPayoutWebHook();
		INewDiscordMessenger requiredService = _serviceProvider.GetRequiredService<INewDiscordMessenger>();
		bool flag = await requiredService.SendEmbedMessage(payoutWebHook, discordEmbed);
		if (flag)
		{
			_attackTracker.ResetWeeklyCounters();
			trackerState = _storage.Load();
			trackerState.LastWeeklySummaryPost = utcNow;
			_storage.Save(trackerState);
			_logger.Log("[WeeklyAttackSummaryJob]:Posted weekly attack summary and reset counters.");
		}
		else
		{
			_logger.Log("[WeeklyAttackSummaryJob]:Failed to post weekly attack summary. Counters were not reset.");
		}
	}

	private DiscordEmbed BuildSummaryEmbed(System.Collections.Generic.IList<AttackSummaryEntry> summary)
	{
		string text = string.Join("\n", summary.Select((AttackSummaryEntry e, int i) => $"{i + 1}. {FormatPlayer(e.PlayerName, e.AllyCode)} - {e.Attacks} attack{(e.Attacks == 1 ? "" : "s")}"));
		if (string.IsNullOrEmpty(text))
		{
			text = "No tracked players.";
		}
		if (text.Length > MAX_EMBED_DESCRIPTION_LENGTH)
		{
			text = text.Substring(0, MAX_EMBED_DESCRIPTION_LENGTH - 3) + "...";
		}
		return new DiscordEmbed
		{
			Title = "Weekly Attack Summary",
			Description = text,
			Color = SUMMARY_EMBED_COLOR,
			Timestamp = DateTime.UtcNow
		};
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

	private static string FormatPlayer(string playerName, string allyCode)
	{
		if (!string.IsNullOrWhiteSpace(playerName))
		{
			return $"{playerName} ({allyCode})";
		}
		return allyCode;
	}
}
