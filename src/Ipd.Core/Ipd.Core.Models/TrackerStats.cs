using System.Collections.Generic;

namespace Ipd.Core.Models;

public class TrackerStats
{
	public string TrackerVersion { get; set; }

	public string HerokuAppId { get; set; }

	public string StartId { get; set; }

	public string ArenaType { get; set; }

	public int PlayersCount { get; set; }

	public List<string> EnabledEnvVars { get; set; }

	public string Hash { get; set; }

	public string DiscordWebHook { get; set; }

	public List<string> AllyCodes { get; set; }
}
