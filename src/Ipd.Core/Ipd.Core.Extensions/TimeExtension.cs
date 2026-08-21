using NodaTime;

namespace Ipd.Core.Extensions;

public static class TimeExtension
{
	public static string ToPayoutString(this Duration value)
	{
		return $"{value.Hours:D2}:{value.Minutes:D2}";
	}
}
