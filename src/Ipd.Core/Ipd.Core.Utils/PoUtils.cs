using System;
using Ipd.Core.Models;
using NodaTime;

namespace Ipd.Core.Utils;

public static class PoUtils
{
	public static Duration GetPoTime(int offsetMinutes, ArenaType arenaType, Instant? utcNow = null)
	{
		Instant instant = (utcNow.HasValue ? utcNow.Value : SystemClock.Instance.GetCurrentInstant());
		DateTime dateTime = instant.ToDateTimeUtc();
		int hourOfDay = ((arenaType == ArenaType.Squad) ? 18 : 19);
		Instant instant2 = Instant.FromUtc(dateTime.Year, dateTime.Month, dateTime.Day, hourOfDay, 0, 0).Minus(Duration.FromMinutes(offsetMinutes));
		if (instant2 >= instant)
		{
			return instant2.Minus(instant);
		}
		return instant2.Plus(Duration.FromHours(24)).Minus(instant);
	}
}
