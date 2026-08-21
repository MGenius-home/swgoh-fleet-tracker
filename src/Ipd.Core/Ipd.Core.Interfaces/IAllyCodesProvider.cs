using System.Collections.Generic;
using System.Threading.Tasks;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IAllyCodesProvider
{
	IList<PlayerSettings> GetAllyCodes();

	Task<IList<PlayerSettings>> GetAllyCodesAsync();
}
