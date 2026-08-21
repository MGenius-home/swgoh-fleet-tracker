using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public enum ContentEncoding
{
	[OriginalName("DEFAULTCONTENTENCODING")]
	Defaultcontentencoding,
	[OriginalName("GZIPCONTENTENCODING")]
	Gzipcontentencoding
}
