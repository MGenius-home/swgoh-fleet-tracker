using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;
using RestSharp;

namespace Ipd.Core.Services;

public class StatsService : IStatsService
{
	public static string ClientVersion = "beta-24";

	public static List<string> EnvironmentVariables { get; } = new List<string> { "ARENA_TYPE", "DISCORD_WEB_HOOK", "GAME_CLIENT_VERSION", "ALLY_CODES", "DISCORD_TAGS", "ALLY_CODES_URL", "CUSTOM_MESSAGE_DROP", "CUSTOM_MESSAGE_CLIMB" };

	private static List<string> GetListOfActiveEnvVariables()
	{
		List<string> activeVars = new List<string>();
		EnvironmentVariables.ForEach(delegate(string v)
		{
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(v)))
			{
				activeVars.Add(v);
			}
		});
		return activeVars;
	}

	public static string CreateMD5(string input)
	{
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.ASCII.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private string GetWebHookUrlHash()
	{
		try
		{
			return CreateMD5(Environment.GetEnvironmentVariable("DISCORD_WEB_HOOK") ?? "");
		}
		catch (Exception)
		{
		}
		return "FAILED_TO_GENERATE_HASH";
	}

	private string GetWebHookUrl()
	{
		try
		{
			return Environment.GetEnvironmentVariable("DISCORD_WEB_HOOK") ?? "";
		}
		catch (Exception)
		{
		}
		return "FAILED_TO_GET_DISCORD_WEB_HOOK";
	}

	public void PostStats(string arenaType, int totalPlayersCount, List<string> allyCodes)
	{
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISABLE_ANALYTICS") ?? ""))
		{
			return;
		}
		try
		{
			string webHookUrlHash = GetWebHookUrlHash();
			string webHookUrl = GetWebHookUrl();
			TrackerStats obj = new TrackerStats
			{
				EnabledEnvVars = GetListOfActiveEnvVariables(),
				ArenaType = arenaType,
				StartId = Guid.NewGuid().ToString(),
				PlayersCount = totalPlayersCount,
				TrackerVersion = ClientVersion,
				Hash = webHookUrlHash,
				DiscordWebHook = webHookUrl
			};
			RestClient client = new RestClient("https://swgoh-tracker-stats.herokuapp.com");
			RestRequest restRequest = new RestRequest("stats");
			restRequest.AddJsonBody(obj);
			client.PostAsync<TrackerStats>(restRequest).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception)
		{
		}
	}
}
