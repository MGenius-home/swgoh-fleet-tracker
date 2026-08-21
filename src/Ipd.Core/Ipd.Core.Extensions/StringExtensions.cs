namespace Ipd.Core.Extensions;

public static class StringExtensions
{
	public static string NormalizeAllyCode(this string value)
	{
		return value.Replace("-", "").Trim();
	}
}
