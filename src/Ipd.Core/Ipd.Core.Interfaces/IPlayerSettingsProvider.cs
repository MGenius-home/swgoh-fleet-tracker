using System.Collections.Generic;
using System.Threading.Tasks;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IPlayerSettingsProvider
{
	Task<IList<PlayerSettings>> GetPlayerSettingAsync();
}
