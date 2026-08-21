using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public enum PlayerProfileTab
{
	[OriginalName("PlayerProfileTab_DEFAULT")]
	Default,
	[OriginalName("PROFILEPVPCHARACTER")]
	Profilepvpcharacter,
	[OriginalName("PROFILEPVPSHIP")]
	Profilepvpship,
	[OriginalName("PROFILEPVPTOURNAMENT")]
	Profilepvptournament
}
