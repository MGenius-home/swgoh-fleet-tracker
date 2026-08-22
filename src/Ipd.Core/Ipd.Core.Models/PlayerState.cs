using System;

namespace Ipd.Core.Models;

public class PlayerState
{
	public string PlayerName { get; set; }

	public int CurrentRank { get; set; }

	public int PreviousRank { get; set; }

	public string UtcPayoutTime { get; set; }

	public string PendingUtcPayoutTime { get; set; }

	public int TimezoneOffsetMinutes { get; set; }

	public int WeeklyAttacks { get; set; }

	public DateTime? LastAttackTimestamp { get; set; }
}
