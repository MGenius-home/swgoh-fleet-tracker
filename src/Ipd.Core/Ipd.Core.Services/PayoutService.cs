using System;
using System.Collections.Generic;
using System.Linq;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace Ipd.Core.Services;

public class PayoutService : IPayoutService
{
	public const int FLEET_PAYOUT_HOUR = 19;

	private const int MinutesPerDay = 1440;

	public string GetUtcPayoutTime(int timezoneOffsetMinutes)
	{
		int minuteOfDay = Mod(FLEET_PAYOUT_HOUR * 60 - timezoneOffsetMinutes, MinutesPerDay);
		return $"{minuteOfDay / 60:D2}:{minuteOfDay % 60:D2}";
	}

	public PayoutShiftInfo BuildShiftInfo(string allyCode, string playerName, string previousUtcPayoutTime, string newUtcPayoutTime)
	{
		int previousMinuteOfDay = ParseMinuteOfDay(previousUtcPayoutTime);
		int newMinuteOfDay = ParseMinuteOfDay(newUtcPayoutTime);
		int deltaMinutes = Mod(newMinuteOfDay - previousMinuteOfDay, MinutesPerDay);
		if (deltaMinutes > MinutesPerDay / 2)
		{
			deltaMinutes -= MinutesPerDay;
		}
		return new PayoutShiftInfo
		{
			AllyCode = allyCode,
			PlayerName = playerName,
			PreviousUtcPayoutTime = previousUtcPayoutTime,
			NewUtcPayoutTime = newUtcPayoutTime,
			ShiftDeltaHours = Math.Round((double)deltaMinutes / 60.0, 2)
		};
	}

	public IList<string> GetSharedPayoutGroup(TrackerState state, string utcPayoutTime, string excludeAllyCode)
	{
		return (from kv in state.Players
			where !(kv.Key == excludeAllyCode) && kv.Value != null && kv.Value.UtcPayoutTime == utcPayoutTime
			select FormatPlayer(kv.Value, kv.Key)).ToList();
	}

	public IList<PayoutRosterEntry> GetFullPayoutRoster(TrackerState state)
	{
		return (from kv in state.Players
			where kv.Value != null && !string.IsNullOrEmpty(kv.Value.UtcPayoutTime)
			orderby ParseMinuteOfDay(kv.Value.UtcPayoutTime), kv.Key
			select new PayoutRosterEntry
			{
				AllyCode = kv.Key,
				PlayerName = kv.Value.PlayerName,
				UtcPayoutTime = kv.Value.UtcPayoutTime
			}).ToList();
	}

	public static int ParseMinuteOfDay(string utcPayoutTime)
	{
		if (string.IsNullOrWhiteSpace(utcPayoutTime))
		{
			return 0;
		}
		string[] parts = utcPayoutTime.Trim().Split(':');
		if (parts.Length != 2 || !int.TryParse(parts[0], out var hour) || !int.TryParse(parts[1], out var minute))
		{
			return 0;
		}
		return Mod(hour * 60 + minute, MinutesPerDay);
	}

	private static int Mod(int value, int modulus)
	{
		return ((value % modulus) + modulus) % modulus;
	}

	private static string FormatPlayer(PlayerState player, string allyCode)
	{
		string name = string.IsNullOrWhiteSpace(player.PlayerName) ? allyCode : player.PlayerName;
		return $"{name} ({allyCode})";
	}
}
