using System.Threading.Tasks;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IPlayerRankService
{
	Task<PlayerArenaRank> GetPlayerRank(string allyCode, AuthResponse auth);
}
