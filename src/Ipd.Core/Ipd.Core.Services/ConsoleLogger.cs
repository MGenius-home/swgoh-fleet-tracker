using System;
using Ipd.Core.Interfaces;

namespace Ipd.Core.Services;

public class ConsoleLogger : ILog
{
	public void Log(string message)
	{
		Console.WriteLine(message);
	}
}
