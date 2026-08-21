using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;
using Ipd.GameClient;
using Ipd.GameClient.Models;

namespace Ipd.Core.Services;

public class PlayerRankService : IPlayerRankService
{
	private IGameClient Client { get; set; }

	public PlayerRankService(IGameClient client)
	{
		Client = client;
	}

	public Task<PlayerArenaRank> GetPlayerRank(string allyCode, AuthResponse auth)
	{
		PlayerArena slimPlayerArenaRanks = Client.GetSlimPlayerArenaRanks(allyCode);
		return Task.FromResult(new PlayerArenaRank
		{
			PlayerName = slimPlayerArenaRanks.PlayerName,
			FleetArenaRank = slimPlayerArenaRanks.FleetArenaRank,
			SquadArenaRank = slimPlayerArenaRanks.SquadArenaRank,
			PayoutOffsetMinutes = slimPlayerArenaRanks.PayoutOffsetMinutes
		});
	}
}
