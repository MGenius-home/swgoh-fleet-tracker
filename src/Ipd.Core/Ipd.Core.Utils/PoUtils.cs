using System;
using NodaTime;

namespace Ipd.Core.Utils;

public static class PoUtils
{
	public const int FLEET_PAYOUT_HOUR = 19;

	public static Duration GetPoTime(int offsetMinutes, Instant? utcNow = null)
	{
		Instant instant = (utcNow.HasValue ? utcNow.Value : SystemClock.Instance.GetCurrentInstant());
		DateTime dateTime = instant.ToDateTimeUtc();
		Instant instant2 = Instant.FromUtc(dateTime.Year, dateTime.Month, dateTime.Day, FLEET_PAYOUT_HOUR, 0, 0).Minus(Duration.FromMinutes(offsetMinutes));
		if (instant2 >= instant)
		{
			return instant2.Minus(instant);
		}
		return instant2.Plus(Duration.FromHours(24)).Minus(instant);
	}
}
