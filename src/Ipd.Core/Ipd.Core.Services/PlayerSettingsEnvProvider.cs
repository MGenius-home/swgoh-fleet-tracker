using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ipd.Core.Extensions;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace Ipd.Core.Services;

public class PlayerSettingsEnvProvider : IPlayerSettingsProvider
{
	private readonly ILog _logger;

	public const string TAGS_ENV_NAME = "DISCORD_TAGS";

	public const string ALLY_CODES_ENV_NAME = "ALLY_CODES";

	public PlayerSettingsEnvProvider(ILog logger)
	{
		_logger = logger;
	}

	public async Task<IList<PlayerSettings>> GetPlayerSettingAsync()
	{
		string text = (Environment.GetEnvironmentVariable("ALLY_CODES") ?? "").Trim();
		IEnumerable<string> codes = (from ac in text.Split(',')
			select ac.Trim().Replace("-", "")).Distinct();
		Dictionary<string, string> tags = await GetTagsAsync();
		return codes.Select((string ac) => new PlayerSettings
		{
			AllyCode = ac,
			Name = "",
			DiscordId = (tags.ContainsKey(ac) ? tags[ac] : "")
		}).ToList();
	}

	private Task<Dictionary<string, string>> GetTagsAsync()
	{
		try
		{
			List<string> list = new List<string>();
			foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
			{
				if (environmentVariable.Key.ToString().StartsWith("DISCORD_TAGS"))
				{
					string text = environmentVariable.Value.ToString().Trim();
					if (!string.IsNullOrEmpty(text))
					{
						list.AddRange(text.Split(','));
					}
				}
			}
			Dictionary<string, string> result = new Dictionary<string, string>();
			list.ForEach(delegate(string t)
			{
				string[] array = t.Trim().Split('|');
				if (array.Length != 2)
				{
					_logger.Log("Invalid tag format in " + t);
				}
				else
				{
					string text2 = array[0].NormalizeAllyCode();
					long result2 = 0L;
					if (!long.TryParse(text2, out result2) || text2.Length != 9)
					{
						_logger.Log("Error: ally code `" + array[0] + "` should consist of 9 digits.");
					}
					else
					{
						result[text2] = array[1];
					}
				}
			});
			return Task.FromResult(result);
		}
		catch (Exception)
		{
			_logger.Log("Failed to process `TAGS_ENV_NAME` environment variable");
		}
		return Task.FromResult(new Dictionary<string, string>());
	}
}
