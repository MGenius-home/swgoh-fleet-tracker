using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ipd.Core.Interfaces;

public interface ITagsProvider
{
	Task<Dictionary<string, string>> GetTagsAsync();
}
