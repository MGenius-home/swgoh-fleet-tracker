using System;

namespace Ipd.Core.Utils;

public static class ScheduleTimeZone
{
	public static TimeZoneInfo Resolve(string ianaId, Action<string> warn)
	{
		if (string.IsNullOrWhiteSpace(ianaId))
		{
			return TimeZoneInfo.Utc;
		}
		try
		{
			return TimeZoneInfo.FindSystemTimeZoneById(ianaId.Trim());
		}
		catch (Exception ex)
		{
			warn?.Invoke($"[Config]:Unknown SCHEDULE_TIMEZONE '{ianaId}' ({ex.GetType().Name}). Schedules will run in UTC.");
			return TimeZoneInfo.Utc;
		}
	}

	public static DateTime NowInZone(TimeZoneInfo zone)
	{
		return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
	}
}
