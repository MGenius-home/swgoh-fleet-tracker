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
			services.AddSingleton((Func<IServiceProvider, IHostedService>)delegate(IServiceProvider serviceProvider)
			{
				Tracker tracker = InitTracker(serviceProvider.GetRequiredService<Channel<DiscordMessage>>());
				tracker.PostStats();
				return new TrackerJob(tracker, Logger);
			});
		});
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

	private static Tracker InitTracker(Channel<DiscordMessage> channel)
	{
		string gameClientVersion = Environment.GetEnvironmentVariable("GAME_CLIENT_VERSION") ?? "99.99.99";
		string text = (Environment.GetEnvironmentVariable("ALLY_CODES_URL") ?? "").Trim();
		ArenaType arenaType = ((!(Environment.GetEnvironmentVariable("ARENA_TYPE") ?? "SQUAD").Trim().Equals("FLEET")) ? ArenaType.Squad : ArenaType.Fleet);
		Logger.Log($"Arena type: {arenaType}");
		string text2 = (Environment.GetEnvironmentVariable("DISCORD_WEB_HOOK") ?? "").Trim();
		if (string.IsNullOrEmpty(text2))
		{
			Logger.Log("ENV variable DISCORD_WEB_HOOK not found");
		}
		IPlayerSettingsProvider playerSettingsProvider = null;
		if (!string.IsNullOrEmpty(text))
		{
			Logger.Log("Ally codes and tags will be loaded from the provided url");
			playerSettingsProvider = new PlayerSettingsUrlProvider(text, Logger);
		}
		else
		{
			playerSettingsProvider = new PlayerSettingsEnvProvider(Logger);
			List<string> list = playerSettingsProvider.GetPlayerSettingAsync().Result.Select((PlayerSettings ac) => ac.AllyCode).ToList();
			Logger.Log($"Provided ally codes from environment: #{list.Count}");
			Logger.Log(string.Join(',', list));
		}
		EnvSettingsService envSettingsService = new EnvSettingsService();
		Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_STATUS");
		Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_CLIMB");
		Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_DROP");
		Logger.Log("Message formats:");
		Logger.Log("MESSAGE_STATUS:" + envSettingsService.MessageFormatOnStatus);
		Logger.Log("MESSAGE_CLIMB:" + envSettingsService.MessageFormatOnClimb);
		Logger.Log("MESSAGE_DROP:" + envSettingsService.MessageFormatOnDrop);
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_CLIMB_RANK_LIMIT", envSettingsService.TagOnClimbRankLimit));
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_DROP_RANK_LIMIT", envSettingsService.TagOnDropRankLimit));
		Logger.Log(string.Format("{0}:{1}", "TAG_ON_DROP_PO_LIMIT", envSettingsService.TagOnDropPayoutLimitMins));
		Logger.Log(string.Format("{0}:{1}", "DISABLE_STATUS_MESSAGE", envSettingsService.IsStatusMessageDisabled));
		return new Tracker(new DiscordMessenger(text2), playerSettingsProvider, new StaticArenaRankStorage(), new PlayerRankService(new GameClient("ipd-arena-tracker:" + StatsService.ClientVersion)
		{
			GameClientVersion = gameClientVersion,
			LogPerformance = false
		}), Logger, new EnvTagsProvider(Logger), arenaType, new StatsService(), channel, new EnvSettingsService());
	}
}
