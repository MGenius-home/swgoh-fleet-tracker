using System.Collections.Generic;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IAttackTrackerService
{
	bool ShouldCountAttack(int timezoneOffsetMinutes);

	IList<AttackSummaryEntry> GetWeeklySummary();

	void ResetWeeklyCounters();
}
