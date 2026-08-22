using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IPersistentStorageService
{
	TrackerState Load();

	void Save(TrackerState state);
}
