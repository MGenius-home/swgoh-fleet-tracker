using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public enum AcceptEncoding
{
	[OriginalName("DEFAULTACCEPTENCODING")]
	Defaultacceptencoding,
	[OriginalName("GZIPACCEPTENCODING")]
	Gzipacceptencoding
}
