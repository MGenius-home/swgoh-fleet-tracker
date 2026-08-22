using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Channels;
using Ipd.Core.Interfaces;
using Ipd.Core.Jobs;
using Ipd.Core.Messages;
using Ipd.Core.Models;
using Ipd.Core.Models.Discord;
using Ipd.Core.Services;
using Ipd.GameClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTracker.Infrastructure;
using SimpleTracker.Services;

namespace SimpleTracker;

internal class Program
{
	private static readonly ILog Logger;

	static Program()
	{
		string text = (Environment.GetEnvironmentVariable("LOGGER_TYPE") ?? "CONSOLE").Trim();
		string text2 = (Environment.GetEnvironmentVariable("LOGGER_HOOK") ?? "").Trim();
		ILog logger;
		if (!text.Equals("DISCORD", StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(text2))
		{
			ILog log = new ConsoleLogger();
			logger = log;
		}
		else
		{
			ILog log = new DiscordLogger(new DiscordMessenger(text2));
			logger = log;
		}
		Logger = logger;
		Logger.Log("Logger type: " + text);
		Logger.Log("Tracker version:" + StatsService.ClientVersion);
	}

	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args).ConfigureServices(delegate(HostBuilderContext hostContext, IServiceCollection services)
		{
			services.AddSingleton((IServiceProvider serviceProvider) => Channel.CreateUnbounded<DiscordMessage>());
			services.AddTransient<INewDiscordMessenger, NewDiscordMessenger>();
			services.AddTransient<HttpClient>();
			services.AddHostedService<DiscordMessengerJob>();
			IPlayerSettingsProvider playerSettingsProvider = CreatePlayerSettingsProvider();
			services.AddSingleton((Func<IServiceProvider, IHostedService>)delegate(IServiceProvider serviceProvider)
			{
				Tracker tracker = InitTracker(serviceProvider.GetRequiredService<Channel<DiscordMessage>>(), playerSettingsProvider);
				tracker.PostStats();
				return new TrackerJob(tracker, Logger);
			});
			services.AddSingleton((Func<IServiceProvider, IHostedService>)delegate(IServiceProvider serviceProvider)
			{
				return new WeeklyAttackSummaryJob(serviceProvider, Logger, new EnvSettingsService());
			});
			services.AddSingleton((Func<IServiceProvider, IHostedService>)delegate(IServiceProvider serviceProvider)
			{
				return new ScheduledStatusJob(serviceProvider, Logger, new EnvSettingsService(), playerSettingsProvider);
			});
		});
	}

	private static IPlayerSettingsProvider CreatePlayerSettingsProvider()
	{
		string allyCodesUrl = (Environment.GetEnvironmentVariable("ALLY_CODES_URL") ?? "").Trim();
		if (!string.IsNullOrEmpty(allyCodesUrl))
		{
			Logger.Log("Ally codes and tags will be loaded from the provided url");
			return new PlayerSettingsUrlProvider(allyCodesUrl, Logger);
		}
		IPlayerSettingsProvider playerSettingsProvider = new PlayerSettingsEnvProvider(Logger);
		List<string> list = playerSettingsProvider.GetPlayerSettingAsync().Result.Select((PlayerSettings ac) => ac.AllyCode).ToList();
		Logger.Log($"Provided ally codes from environment: #{list.Count}");
		Logger.Log(string.Join(',', list));
		return playerSettingsProvider;
	}

	private static void Main(string[] args)
	{
		if (string.IsNullOrEmpty((Environment.GetEnvironmentVariable("DISCORD_WEB_HOOK") ?? "").Trim()))
		{
			Logger.Log("env variable DISCORD_WEB_HOOK not found");
		}
		else
		{
			CreateHostBuilder(args).Build().Run();
		}
	}

	private static Tracker InitTracker(Channel<DiscordMessage> channel, IPlayerSettingsProvider playerSettingsProvider)
	{
		string gameClientVersion = Environment.GetEnvironmentVariable("GAME_CLIENT_VERSION") ?? "99.99.99";
		if (!string.IsNullOrEmpty((Environment.GetEnvironmentVariable("ARENA_TYPE") ?? "").Trim()))
		{
			Logger.Log("ARENA_TYPE is no longer used - fleet arena is the only tracked type.");
		}
		Logger.Log("Tracking fleet arena payouts (19:00 local time).");
		string text2 = (Environment.GetEnvironmentVariable("DISCORD_WEB_HOOK") ?? "").Trim();
		if (string.IsNullOrEmpty(text2))
		{
			Logger.Log("ENV variable DISCORD_WEB_HOOK not found");
		}
		EnvSettingsService envSettingsService = new EnvSettingsService();
		Logger.Log("Message formats:");
		Logger.Log("MESSAGE_STATUS:" + ((string.IsNullOrEmpty(envSettingsService.StatusMessageCron) ? "(scheduled roster off) " : "") + envSettingsService.MessageFormatOnStatus));
		Logger.Log("MESSAGE_CLIMB:" + envSettingsService.MessageFormatOnClimb);
		Logger.Log("MESSAGE_DROP:" + envSettingsService.MessageFormatOnDrop);
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_CLIMB_RANK_LIMIT", envSettingsService.TagOnClimbRankLimit));
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_DROP_RANK_LIMIT", envSettingsService.TagOnDropRankLimit));
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_DROP_PO_LIMIT", envSettingsService.TagOnDropPayoutLimitMins));
		Logger.Log(string.Format("{0}:{1}", "POST_FULL_PAYOUT_LIST_ON_CHANGE", envSettingsService.PostFullPayoutListOnChange));
		Logger.Log(string.Format("{0}:{1}", "WEEKLY_ATTACK_SUMMARY_CRON", envSettingsService.WeeklyAttackSummaryCron));
		Logger.Log(string.Format("{0}:{1}", "ENABLE_WEEKLY_ATTACK_SUMMARY", envSettingsService.IsWeeklyAttackSummaryEnabled));
		Logger.Log(string.Format("{0}:{1}", "ENABLE_PAYOUT_TRACKING", envSettingsService.IsPayoutTrackingEnabled));
		Logger.Log(string.Format("{0}:{1}", "STATUS_MESSAGE_CRON", (string.IsNullOrEmpty(envSettingsService.StatusMessageCron) ? "not set" : envSettingsService.StatusMessageCron)));
		Logger.Log(string.Format("{0}:{1}", "STORAGE_FILE_PATH", envSettingsService.StorageFilePath));
		Logger.Log(string.Format("{0}:{1}", "PAYOUT_WEBHOOK_URL", (string.IsNullOrEmpty(envSettingsService.PayoutWebHookUrl) ? "not set, falling back to DISCORD_WEB_HOOK" : "set")));
		FileStorageService fileStorageService = new FileStorageService(envSettingsService.StorageFilePath, Logger);
		PayoutService payoutService = new PayoutService();
		AttackTrackerService attackTrackerService = new AttackTrackerService(fileStorageService, payoutService);
		return new Tracker(new DiscordMessenger(text2), playerSettingsProvider, fileStorageService, new PlayerRankService(new GameClient("ipd-arena-tracker:" + StatsService.ClientVersion)
		{
			GameClientVersion = gameClientVersion,
			LogPerformance = false
		}), Logger, new EnvTagsProvider(Logger), new StatsService(), channel, envSettingsService, payoutService, attackTrackerService);
	}
}
