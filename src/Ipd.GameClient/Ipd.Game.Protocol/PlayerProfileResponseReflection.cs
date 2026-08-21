using System;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public static class PlayerProfileResponseReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PlayerProfileResponseReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtQbGF5ZXJQcm9maWxlUmVzcG9uc2UucHJvdG8SEWlwZC5nYW1lLnByb3Rv" + "Y29sImMKFVBsYXllclByb2ZpbGVSZXNwb25zZRIMCgRuYW1lGAEgASgJEjwK" + "DmFyZW5hX3N0YXR1c2VzGBEgAygLMiQuaXBkLmdhbWUucHJvdG9jb2wuUGxh" + "eWVyQXJlbmFTdGF0dXMiWgoRUGxheWVyQXJlbmFTdGF0dXMSNgoKYXJlbmFf" + "dHlwZRgBIAEoDjIiLmlwZC5nYW1lLnByb3RvY29sLlBsYXllckFyZW5hVHlw" + "ZRINCgVwbGFjZRgCIAEoBSpHCg9QbGF5ZXJBcmVuYVR5cGUSFAoQUGxheWVy" + "QXJlbmFfTm9uZRAAEg4KClNxdWFkQXJlbmEQARIOCgpGbGVldEFyZW5hEAJi" + "BnByb3RvMw=="), new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(PlayerArenaType) }, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(PlayerProfileResponse), PlayerProfileResponse.Parser, new string[2] { "Name", "ArenaStatuses" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(PlayerArenaStatus), PlayerArenaStatus.Parser, new string[2] { "ArenaType", "Place" }, null, null, null)
		}));
	}
}
