using System;
using Ipd.Core.Interfaces;

namespace Ipd.Core.Services;

public class DiscordLogger : ILog
{
	private readonly IDiscordMessenger _discordMessenger;

	private readonly ILog _consoleLogger;

	public DiscordLogger(IDiscordMessenger discordMessenger)
	{
		_discordMessenger = discordMessenger;
		_consoleLogger = new ConsoleLogger();
	}

	public void Log(string message)
	{
		try
		{
			_discordMessenger.SendTextMessage(message).Wait();
		}
		catch (Exception ex)
		{
			_consoleLogger.Log("Discord logger fail: " + ex.Message);
		}
		finally
		{
			_consoleLogger.Log(message);
		}
	}
}
