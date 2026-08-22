using System.Collections.Generic;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IAttackTrackerService
{
	bool RecordAttack(string allyCode, int timezoneOffsetMinutes, ArenaType arenaType);

	IList<AttackSummaryEntry> GetWeeklySummary();

	void ResetWeeklyCounters();
}
