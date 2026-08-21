using System;

namespace Ipd.Core.Messages;

public class EnvSettingsService : ISettingsService
{
	public const string CUSTOM_MESSAGE_STATUS = "CUSTOM_MESSAGE_STATUS";

	public const string CUSTOM_MESSAGE_CLIMB = "CUSTOM_MESSAGE_CLIMB";

	public const string CUSTOM_MESSAGE_DROP = "CUSTOM_MESSAGE_DROP";

	public const string DISABLE_STATUS_MESSAGE = "DISABLE_STATUS_MESSAGE";

	public const string TAG_ON_DROP_RANK_LIMIT = "TAG_ON_DROP_RANK_LIMIT";

	public const string TAG_ON_CLIMB_RANK_LIMIT = "TAG_ON_CLIMB_RANK_LIMIT";

	public const string TAG_ON_DROP_PO_LIMIT = "TAG_ON_DROP_PO_LIMIT";

	public const int DEFAULT_TAG_ON_DROP_RANK_LIMIT = 0;

	public const int DEFAULT_TAG_ON_CLIMB_RANK_LIMIT = 1000;

	public const int DEFAULT_TAG_ON_DROP_PO_LIMIT_MINUTES = 1440;

	private readonly Lazy<string> _isStatusMessageDisabled = new Lazy<string>(() => (Environment.GetEnvironmentVariable("DISABLE_STATUS_MESSAGE") ?? "").Trim());

	private readonly Lazy<string> _tagOnDropRankLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_DROP_RANK_LIMIT") ?? "").Trim());

	private readonly Lazy<string> _tagOnClimbRankLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_CLIMB_RANK_LIMIT") ?? "").Trim());

	private readonly Lazy<string> _tagOnDropPayoutLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_DROP_PO_LIMIT") ?? "").Trim());

	public string MessageFormatOnStatus => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_STATUS") ?? "%USER_ICON%`%PLAYER_NAME%` is at %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public string MessageFormatOnClimb => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_CLIMB") ?? "%TAG_ON_CLIMB%%USER_ICON%`%PLAYER_NAME%` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public string MessageFormatOnDrop => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_DROP") ?? "%TAG_ON_DROP%%USER_ICON%`%PLAYER_NAME%` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public bool IsStatusMessageDisabled
	{
		get
		{
			string value = _isStatusMessageDisabled.Value;
			if (value != null)
			{
				return value == "TRUE";
			}
			return false;
		}
	}

	public int TagOnDropRankLimit
	{
		get
		{
			if (int.TryParse(_tagOnDropRankLimit.Value, out var result))
			{
				return result;
			}
			return 0;
		}
	}

	public int TagOnClimbRankLimit
	{
		get
		{
			if (int.TryParse(_tagOnClimbRankLimit.Value, out var result))
			{
				return result;
			}
			return 1000;
		}
	}

	public int TagOnDropPayoutLimitMins
	{
		get
		{
			if (int.TryParse(_tagOnDropPayoutLimit.Value, out var result))
			{
				return result;
			}
			return 1440;
		}
	}
}
