using System;
using System.Collections.Generic;

namespace Ipd.Core.Models;

public class TrackerState
{
	public Dictionary<string, PlayerState> Players { get; set; } = new Dictionary<string, PlayerState>();

	public DateTime? LastWeeklySummaryPost { get; set; }

	public DateTime? LastScheduledStatusPost { get; set; }
}
