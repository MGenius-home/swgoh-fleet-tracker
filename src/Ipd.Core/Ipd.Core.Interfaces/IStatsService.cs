using System.Collections.Generic;

namespace Ipd.Core.Interfaces;

public interface IStatsService
{
	void PostStats(string arenaType, int totalPlayers, List<string> allyCodes);
}
