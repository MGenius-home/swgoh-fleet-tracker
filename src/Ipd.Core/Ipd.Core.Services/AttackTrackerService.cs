using System;
using System.Collections.Generic;
using System.Linq;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace Ipd.Core.Services;

public class AttackTrackerService : IAttackTrackerService
{
	public const int PAYOUT_RESET_WINDOW_MINUTES = 60;

	private readonly IPersistentStorageService _storage;

	private readonly IPayoutService _payoutService;

	public AttackTrackerService(IPersistentStorageService storage, IPayoutService payoutService)
	{
		_storage = storage;
		_payoutService = payoutService;
	}

	public bool ShouldCountAttack(int timezoneOffsetMinutes)
	{
		return IsOutsidePayoutResetWindow(timezoneOffsetMinutes, DateTime.UtcNow);
	}

	public IList<AttackSummaryEntry> GetWeeklySummary()
	{
		TrackerState trackerState = _storage.Load();
		return (from kv in trackerState.Players
			where kv.Value != null
			select new AttackSummaryEntry
			{
				AllyCode = kv.Key,
				PlayerName = kv.Value.PlayerName,
				Attacks = kv.Value.WeeklyAttacks
			} into e
			orderby e.Attacks descending, e.AllyCode
			select e).ToList();
	}

	public void ResetWeeklyCounters()
	{
		TrackerState trackerState = _storage.Load();
		foreach (PlayerState value in trackerState.Players.Values)
		{
			if (value != null)
			{
				value.WeeklyAttacks = 0;
			}
		}
		_storage.Save(trackerState);
	}

	private bool IsOutsidePayoutResetWindow(int timezoneOffsetMinutes, DateTime utcNow)
	{
		int payoutMinuteOfDay = PayoutService.ParseMinuteOfDay(_payoutService.GetUtcPayoutTime(timezoneOffsetMinutes));
		int nowMinuteOfDay = utcNow.Hour * 60 + utcNow.Minute;
		int delta = ((nowMinuteOfDay - payoutMinuteOfDay) % 1440 + 1440) % 1440;
		return delta >= PAYOUT_RESET_WINDOW_MINUTES;
	}
}
