using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public enum PlayerArenaType
{
	[OriginalName("PlayerArena_None")]
	PlayerArenaNone,
	[OriginalName("SquadArena")]
	SquadArena,
	[OriginalName("FleetArena")]
	FleetArena
}
