using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ipd.Core.Extensions;
using Ipd.Core.Interfaces;

namespace Ipd.Core.Services;

public class EnvTagsProvider : ITagsProvider
{
	private readonly ILog _logger;

	public const string TAGS_ENV_NAME = "DISCORD_TAGS";

	public EnvTagsProvider(ILog logger)
	{
		_logger = logger;
	}

	public Task<Dictionary<string, string>> GetTagsAsync()
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
