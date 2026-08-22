using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models.Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Ipd.Core.Services;

public class NewDiscordMessenger : INewDiscordMessenger
{
	private readonly HttpClient _httpClient;

	public NewDiscordMessenger(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public Task<bool> SendTextMessage(string webHookUrl, string textMessage)
	{
		return SendMessage(webHookUrl, new
		{
			content = textMessage
		});
	}

	public Task<bool> SendEmbedMessage(string webHookUrl, DiscordEmbed embed)
	{
		return SendMessage(webHookUrl, new
		{
			embeds = new[] { embed }
		});
	}

	private async Task<bool> SendMessage(string discordWebHook, object payload)
	{
		StringContent content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
		try
		{
			HttpResponseMessage httpResponseMessage = await Policy.HandleResult((HttpResponseMessage r) => !r.IsSuccessStatusCode).RetryAsync(3, async delegate(DelegateResult<HttpResponseMessage> result, int retryCount, Context context)
			{
				if (result.Result.StatusCode == HttpStatusCode.TooManyRequests)
				{
					DiscrodRateLimitResponse discrodRateLimitResponse = JsonConvert.DeserializeObject<DiscrodRateLimitResponse>(await result.Result.Content.ReadAsStringAsync(), new JsonSerializerSettings
					{
						ContractResolver = new DefaultContractResolver
						{
							NamingStrategy = new SnakeCaseNamingStrategy()
						}
					});
					if (discrodRateLimitResponse.RetryAfter != 0)
					{
						TimeSpan timeSpan = TimeSpan.FromMilliseconds(discrodRateLimitResponse.RetryAfter);
						if (retryCount >= 2)
						{
							Console.WriteLine($"[DiscordMessenger]:Request failed with StatusCode({result.Result.StatusCode}). Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
						}
						await Task.Delay(timeSpan);
					}
					else
					{
						TimeSpan timeSpan2 = TimeSpan.FromMilliseconds(1000.0);
						if (retryCount >= 2)
						{
							Console.WriteLine($"[DiscordMessenger]:Request failed with StatusCode({result.Result.StatusCode}). Waiting {timeSpan2} before next retry. Retry attempt {retryCount}");
						}
						await Task.Delay(timeSpan2);
					}
				}
			}).ExecuteAsync(() => _httpClient.PostAsync(discordWebHook, content));
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				Console.WriteLine($"[DiscordMessenger]:Request failed with StatusCode({httpResponseMessage.StatusCode}).");
				return false;
			}
			return true;
		}
		finally
		{
			if (content != null)
			{
				((IDisposable)content).Dispose();
			}
		}
	}
}
