using System.Collections.Generic;
using Ipd.Core.Models;

namespace Ipd.Core.Interfaces;

public interface IPayoutService
{
	string GetUtcPayoutTime(int timezoneOffsetMinutes, ArenaType arenaType);

	PayoutShiftInfo BuildShiftInfo(string allyCode, string playerName, string previousUtcPayoutTime, string newUtcPayoutTime);

	IList<string> GetSharedPayoutGroup(TrackerState state, string utcPayoutTime, string excludeAllyCode);

	IList<PayoutRosterEntry> GetFullPayoutRoster(TrackerState state);
}
