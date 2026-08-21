using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;
using Newtonsoft.Json;

namespace Ipd.Core.Services;

public class PlayerSettingsUrlProvider : IPlayerSettingsProvider
{
	private ILog _logger;

	private string _url;

	public PlayerSettingsUrlProvider(string url, ILog logger)
	{
		_url = url;
		_logger = logger;
	}

	public async Task<IList<PlayerSettings>> GetPlayerSettingAsync()
	{
		using HttpClient httpClient = new HttpClient();
		HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(_url);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			_logger.Log($"[PlayerSettingsProvider]:Failed to load player settings. Status code ({httpResponseMessage.StatusCode}).");
			return new List<PlayerSettings>();
		}
		try
		{
			List<PlayerSettings> list = JsonConvert.DeserializeObject<List<PlayerSettings>>(await httpResponseMessage.Content.ReadAsStringAsync());
			list.ForEach(delegate(PlayerSettings s)
			{
				s.DiscordId = (s.DiscordId ?? "").Trim();
			});
			return list;
		}
		catch (Exception ex)
		{
			_logger.Log("[PlayerSettingsProvider]:Failed to deserialize player settings: " + ex.Message);
			return new List<PlayerSettings>();
		}
	}
}
