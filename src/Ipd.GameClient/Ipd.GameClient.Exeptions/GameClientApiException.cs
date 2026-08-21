using System;
using Ipd.Game.Protocol;

namespace Ipd.GameClient.Exeptions;

public class GameClientApiException : Exception
{
	public ResponseCode ErrorCode { get; set; }

	public GameClientApiException()
	{
	}

	public GameClientApiException(string message)
		: base(message)
	{
	}

	public GameClientApiException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
