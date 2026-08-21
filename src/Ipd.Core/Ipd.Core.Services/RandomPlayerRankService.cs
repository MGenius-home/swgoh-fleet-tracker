using System;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace Ipd.Core.Services;

public class RandomPlayerRankService : IPlayerRankService
{
	private int _min;

	private int _max;

	public RandomPlayerRankService(int min = 1, int max = 51)
	{
		_min = min;
		_max = max;
	}

	public Task<PlayerArenaRank> GetPlayerRank(string allyCode, AuthResponse auth)
	{
		Random random = new Random();
		return Task.FromResult(new PlayerArenaRank
		{
			PlayerName = allyCode,
			SquadArenaRank = random.Next(_min, _max),
			FleetArenaRank = random.Next(_min, _max)
		});
	}

	public Task<AuthResponse> Login()
	{
		return Task.FromResult(new AuthResponse());
	}
}
