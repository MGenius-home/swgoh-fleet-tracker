using System;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public static class PlayerProfileRequestReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PlayerProfileRequestReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChpQbGF5ZXJQcm9maWxlUmVxdWVzdC5wcm90bxIRaXBkLmdhbWUucHJvdG9j" + "b2wiPAoUUGxheWVyUHJvZmlsZVJlcXVlc3QSEQoJcGxheWVyX2lkGAEgASgJ" + "EhEKCWFsbHlfY29kZRgCIAEoCWIGcHJvdG8z"), new FileDescriptor[0], new GeneratedClrTypeInfo(null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerProfileRequest), PlayerProfileRequest.Parser, new string[2] { "PlayerId", "AllyCode" }, null, null, null)
		}));
	}
}
