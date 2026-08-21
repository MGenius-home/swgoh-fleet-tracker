using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Google.Protobuf;
using Ipd.Game.Protocol;
using Ipd.GameClient.Exeptions;
using Ipd.GameClient.Extensions;
using Ipd.GameClient.Models;

namespace Ipd.GameClient;

public class GameClient : IGameClient
{
	private readonly string _appVersion;

	public string GameClientVersion { get; set; }

	public bool LogPerformance { get; set; }

	public GameClient(string appVrsion)
	{
		_appVersion = appVrsion;
	}

	public PlayerArena GetSlimPlayerArenaRanks(string playerAllyCode)
	{
		PlayerProfileRequest message = new PlayerProfileRequest
		{
			PlayerId = "",
			AllyCode = playerAllyCode.Replace("-", "").Trim()
		};
		byte[] requestEnvelope = GetRequestEnvelope("PlayerRpc", "GetPlayerArenaProfile", message.ToByteString());
		Stopwatch stopwatch = Stopwatch.StartNew();
		using Stream stream = BasicPostRequest(requestEnvelope).GetResponseStream();
		byte[] data = stream.ToByteArray();
		stopwatch.Stop();
		if (LogPerformance)
		{
			Console.WriteLine(string.Format("{0}:get ranks: took time: {1}ms", "GameClient", stopwatch.ElapsedMilliseconds));
		}
		ResponseEnvelope responseEnvelope = ResponseEnvelope.Parser.ParseFrom(data);
		if (responseEnvelope.Code != ResponseCode.Ok)
		{
			throw new GameClientApiException($"errorCode:{responseEnvelope.Code}, allyCode:{playerAllyCode}, {responseEnvelope.Message}")
			{
				ErrorCode = responseEnvelope.Code
			};
		}
		byte[] data2 = responseEnvelope.Payload.ToByteArray().Unzip();
		SlimPlayerArenaProfileResponse slimPlayerArenaProfileResponse = SlimPlayerArenaProfileResponse.Parser.ParseFrom(data2);
		int squadArenaRank = -1;
		PlayerPvpProfile playerPvpProfile = slimPlayerArenaProfileResponse.PvpProfile.FirstOrDefault((PlayerPvpProfile a) => a.Tab == PlayerProfileTab.Profilepvpcharacter);
		if (playerPvpProfile != null)
		{
			squadArenaRank = playerPvpProfile.Rank;
		}
		int fleetArenaRank = -1;
		PlayerPvpProfile playerPvpProfile2 = slimPlayerArenaProfileResponse.PvpProfile.FirstOrDefault((PlayerPvpProfile a) => a.Tab == PlayerProfileTab.Profilepvpship);
		if (playerPvpProfile2 != null)
		{
			fleetArenaRank = playerPvpProfile2.Rank;
		}
		return new PlayerArena
		{
			PlayerName = slimPlayerArenaProfileResponse.Name,
			SquadArenaRank = squadArenaRank,
			FleetArenaRank = fleetArenaRank,
			PayoutOffsetMinutes = slimPlayerArenaProfileResponse.LocalTimeZoneOffsetMinutes
		};
	}

	private byte[] GetRequestEnvelope(string serviceName, string requestMethod, ByteString payload)
	{
		RequestEnvelope requestEnvelope = new RequestEnvelope();
		if (payload != null)
		{
			requestEnvelope.Payload = payload;
		}
		requestEnvelope.CorrelationId = 0;
		requestEnvelope.ServiceName = serviceName;
		requestEnvelope.MethodName = requestMethod;
		requestEnvelope.ClientVersion = 181815;
		long num2 = (requestEnvelope.ClientStartupTimestamp = (long)(Math.Floor((double)DateTime.Now.Ticks / 1000.0) - 10.0));
		requestEnvelope.Platform = "Android";
		requestEnvelope.Region = "NA";
		requestEnvelope.ClientExternalVersion = GameClientVersion ?? "99.99.99";
		requestEnvelope.ClientInternalVersion = GameClientVersion ?? "99.99.99";
		requestEnvelope.RequestId = Guid.NewGuid().ToString().ToLower();
		requestEnvelope.AcceptEncoding = AcceptEncoding.Gzipacceptencoding;
		requestEnvelope.CurrentClientTime = num2 + 8;
		requestEnvelope.NetworkAccess = "W";
		requestEnvelope.Application = _appVersion ?? "";
		using MemoryStream memoryStream = new MemoryStream();
		((IMessage)requestEnvelope).WriteTo((Stream)memoryStream);
		return memoryStream.ToArray();
	}

	private HttpWebResponse BasicPostRequest(byte[] body)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://swprod.capitalgames.com/rpc");
		httpWebRequest.ServicePoint.ConnectionLeaseTimeout = 600000;
		httpWebRequest.Method = "POST";
		httpWebRequest.Headers.Add("Accept-Encoding", "gzip");
		httpWebRequest.ContentType = "application/x-protobuf";
		httpWebRequest.ContentLength = body.Length;
		using (Stream stream = httpWebRequest.GetRequestStream())
		{
			stream.Write(body, 0, body.Length);
		}
		return (HttpWebResponse)httpWebRequest.GetResponse();
	}
}
