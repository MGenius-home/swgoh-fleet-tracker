using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ipd.Core.Interfaces;

public interface IArenaRankStorage
{
	int? GetRank(string allyCode);

	void SaveRank(string allyCode, int rank);

	IDictionary<string, int> GetRanks();

	Task<IDictionary<string, int>> GetRanksAsync();

	Task SaveRanksAsync(IDictionary<string, int> ranks);
}
