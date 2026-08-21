using Ipd.GameClient.Models;

namespace Ipd.GameClient;

public interface IGameClient
{
	PlayerArena GetSlimPlayerArenaRanks(string playerAllyCode);
}
