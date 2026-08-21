using System;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public static class SlimPlayerProfileReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static SlimPlayerProfileReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdTbGltUGxheWVyUHJvZmlsZS5wcm90bxIRaXBkLmdhbWUucHJvdG9jb2wi" + "RQodU2xpbVBsYXllckFyZW5hUHJvZmlsZVJlcXVlc3QSEQoJcGxheWVyX2lk" + "GAEgASgJEhEKCWFsbHlfY29kZRgCIAEoCSLFAQoeU2xpbVBsYXllckFyZW5h" + "UHJvZmlsZVJlc3BvbnNlEgwKBG5hbWUYASABKAkSDQoFbGV2ZWwYAiABKAUS" + "EQoJYWxseV9jb2RlGAMgASgDEhEKCXBsYXllcl9pZBgEIAEoCRI4CgtwdnBf" + "cHJvZmlsZRgFIAMoCzIjLmlwZC5nYW1lLnByb3RvY29sLlBsYXllclB2cFBy" + "b2ZpbGUSJgoebG9jYWxfdGltZV96b25lX29mZnNldF9taW51dGVzGAYgASgR" + "ImQKEFBsYXllclB2cFByb2ZpbGUSMAoDdGFiGAEgASgOMiMuaXBkLmdhbWUu" + "cHJvdG9jb2wuUGxheWVyUHJvZmlsZVRhYhIMCgRyYW5rGAIgASgFEhAKCGV2" + "ZW50X2lkGAQgASgJKncKEFBsYXllclByb2ZpbGVUYWISHAoYUGxheWVyUHJv" + "ZmlsZVRhYl9ERUZBVUxUEAASFwoTUFJPRklMRVBWUENIQVJBQ1RFUhABEhIK" + "DlBST0ZJTEVQVlBTSElQEAISGAoUUFJPRklMRVBWUFRPVVJOQU1FTlQQA2IG" + "cHJvdG8z"), new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(PlayerProfileTab) }, new GeneratedClrTypeInfo[3]
		{
			new GeneratedClrTypeInfo(typeof(SlimPlayerArenaProfileRequest), SlimPlayerArenaProfileRequest.Parser, new string[2] { "PlayerId", "AllyCode" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(SlimPlayerArenaProfileResponse), SlimPlayerArenaProfileResponse.Parser, new string[6] { "Name", "Level", "AllyCode", "PlayerId", "PvpProfile", "LocalTimeZoneOffsetMinutes" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(PlayerPvpProfile), PlayerPvpProfile.Parser, new string[3] { "Tab", "Rank", "EventId" }, null, null, null)
		}));
	}
}
