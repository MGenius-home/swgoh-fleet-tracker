using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public enum ResponseCode
{
	[OriginalName("NONE")]
	None = 0,
	[OriginalName("OK")]
	Ok = 1,
	[OriginalName("ERROR")]
	Error = 2,
	[OriginalName("SERVERERROR")]
	Servererror = 3,
	[OriginalName("SESSIONEXPIRED")]
	Sessionexpired = 4,
	[OriginalName("AUTHFAILED")]
	Authfailed = 5,
	[OriginalName("RATEEXCEEDED")]
	Rateexceeded = 6,
	[OriginalName("SERVERUNAVAILABLE")]
	Serverunavailable = 7,
	[OriginalName("INVALIDREQUEST")]
	Invalidrequest = 8,
	[OriginalName("INVALIDDATA")]
	Invaliddata = 9,
	[OriginalName("LEADERBOARDMATCHMAKINGERROR")]
	Leaderboardmatchmakingerror = 10,
	[OriginalName("UNAUTHORIZED")]
	Unauthorized = 11,
	[OriginalName("SUSPENDED")]
	Suspended = 12,
	[OriginalName("SERVEROUTAGE")]
	Serveroutage = 13,
	[OriginalName("NETWORKUNAVAILABLE")]
	Networkunavailable = 20,
	[OriginalName("SEQUENCEHIGH")]
	Sequencehigh = 30,
	[OriginalName("SEQUENCELOW")]
	Sequencelow = 31,
	[OriginalName("RECORDNOTFOUND")]
	Recordnotfound = 32,
	[OriginalName("EVENTNOTFOUND")]
	Eventnotfound = 33,
	[OriginalName("INSUFFICIENTRESOURCES")]
	Insufficientresources = 40,
	[OriginalName("INVALIDCLIENTVERSION")]
	Invalidclientversion = 50,
	[OriginalName("FORCECLIENTRESTART")]
	Forceclientrestart = 51,
	[OriginalName("INCOMPATIBLEDEVICE")]
	Incompatibledevice = 52,
	[OriginalName("ACCOUNTUPDATED")]
	Accountupdated = 53,
	[OriginalName("INVALIDRECEIPT")]
	Invalidreceipt = 60,
	[OriginalName("PAYMENTPENDING")]
	Paymentpending = 61,
	[OriginalName("OPPONENTINBATTLE")]
	Opponentinbattle = 71,
	[OriginalName("UNDERATTACK")]
	Underattack = 72,
	[OriginalName("OPPONENTDATASTALE")]
	Opponentdatastale = 73,
	[OriginalName("BATTLETIMEDOUT")]
	Battletimedout = 74,
	[OriginalName("PLAYERRANKSTALE")]
	Playerrankstale = 75
}
