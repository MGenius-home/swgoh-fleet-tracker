using System;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;

namespace Ipd.Core.Services;

public class ConsoleDiscordMessenger : IDiscordMessenger
{
	public string DiscordWebHook
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Task SendTextMessage(string textMessage)
	{
		Console.WriteLine(textMessage);
		return Task.CompletedTask;
	}

	public Task SendTextTaggedMessage(string userDiscordId, string textMessage)
	{
		throw new NotImplementedException();
	}
}
