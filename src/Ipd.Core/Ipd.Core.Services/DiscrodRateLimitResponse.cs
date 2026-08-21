namespace Ipd.Core.Services;

public class DiscrodRateLimitResponse
{
	public string Message { get; set; } = "";

	public int RetryAfter { get; set; }

	public bool Global { get; set; }
}
