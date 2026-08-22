using System;

namespace Ipd.Core.Messages;

public class EnvSettingsService : ISettingsService
{
	public const string CUSTOM_MESSAGE_STATUS = "CUSTOM_MESSAGE_STATUS";

	public const string CUSTOM_MESSAGE_CLIMB = "CUSTOM_MESSAGE_CLIMB";

	public const string CUSTOM_MESSAGE_DROP = "CUSTOM_MESSAGE_DROP";

	public const string TAG_ON_DROP_RANK_LIMIT = "TAG_ON_DROP_RANK_LIMIT";

	public const string TAG_ON_CLIMB_RANK_LIMIT = "TAG_ON_CLIMB_RANK_LIMIT";

	public const string TAG_ON_DROP_PO_LIMIT = "TAG_ON_DROP_PO_LIMIT";

	public const string DISCORD_WEB_HOOK = "DISCORD_WEB_HOOK";

	public const string PAYOUT_WEBHOOK_URL = "PAYOUT_WEBHOOK_URL";

	public const string POST_FULL_PAYOUT_LIST_ON_CHANGE = "POST_FULL_PAYOUT_LIST_ON_CHANGE";

	public const string WEEKLY_ATTACK_SUMMARY_CRON = "WEEKLY_ATTACK_SUMMARY_CRON";

	public const string STATUS_MESSAGE_CRON = "STATUS_MESSAGE_CRON";

	public const string POLL_INTERVAL_SECONDS = "POLL_INTERVAL_SECONDS";

	public const string ENABLE_WEEKLY_ATTACK_SUMMARY = "ENABLE_WEEKLY_ATTACK_SUMMARY";

	public const string ENABLE_PAYOUT_TRACKING = "ENABLE_PAYOUT_TRACKING";

	public const string STORAGE_FILE_PATH = "STORAGE_FILE_PATH";

	public const int DEFAULT_TAG_ON_DROP_RANK_LIMIT = 0;

	public const int DEFAULT_TAG_ON_CLIMB_RANK_LIMIT = 1000;

	public const int DEFAULT_TAG_ON_DROP_PO_LIMIT_MINUTES = 1440;

	public const string DEFAULT_WEEKLY_ATTACK_SUMMARY_CRON = "0 0 * * 0";

	private readonly Lazy<string> _tagOnDropRankLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_DROP_RANK_LIMIT") ?? "").Trim());

	private readonly Lazy<string> _tagOnClimbRankLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_CLIMB_RANK_LIMIT") ?? "").Trim());

	private readonly Lazy<string> _tagOnDropPayoutLimit = new Lazy<string>(() => (Environment.GetEnvironmentVariable("TAG_ON_DROP_PO_LIMIT") ?? "").Trim());

	private readonly Lazy<string> _isWeeklyAttackSummaryEnabled = new Lazy<string>(() => (Environment.GetEnvironmentVariable("ENABLE_WEEKLY_ATTACK_SUMMARY") ?? "").Trim());

	private readonly Lazy<string> _isPayoutTrackingEnabled = new Lazy<string>(() => (Environment.GetEnvironmentVariable("ENABLE_PAYOUT_TRACKING") ?? "").Trim());

	public string MessageFormatOnStatus => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_STATUS") ?? "%USER_ICON%`%PLAYER_NAME%` is at %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public string MessageFormatOnClimb => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_CLIMB") ?? "%TAG_ON_CLIMB%%USER_ICON%`%PLAYER_NAME%` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public string MessageFormatOnDrop => Environment.GetEnvironmentVariable("CUSTOM_MESSAGE_DROP") ?? "%TAG_ON_DROP%%USER_ICON%`%PLAYER_NAME%` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

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

	public string PayoutWebHookUrl => (Environment.GetEnvironmentVariable("PAYOUT_WEBHOOK_URL") ?? "").Trim();

	public bool PostFullPayoutListOnChange
	{
		get
		{
			string value = (Environment.GetEnvironmentVariable("POST_FULL_PAYOUT_LIST_ON_CHANGE") ?? "").Trim();
			if (value != null)
			{
				return value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}
	}

	public string WeeklyAttackSummaryCron
	{
		get
		{
			string value = (Environment.GetEnvironmentVariable("WEEKLY_ATTACK_SUMMARY_CRON") ?? "").Trim();
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			return "0 0 * * 0";
		}
	}

	public string StatusMessageCron => (Environment.GetEnvironmentVariable("STATUS_MESSAGE_CRON") ?? "").Trim();

	public int PollIntervalSeconds
	{
		get
		{
			string value = (Environment.GetEnvironmentVariable("POLL_INTERVAL_SECONDS") ?? "").Trim();
			if (!int.TryParse(value, out var result))
			{
				return 15;
			}
			if (result < 2)
			{
				return 2;
			}
			if (result > 3600)
			{
				return 3600;
			}
			return result;
		}
	}

	public string ScheduleTimeZoneId
	{
		get
		{
			string value = (Environment.GetEnvironmentVariable("SCHEDULE_TIMEZONE") ?? "").Trim();
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			return "UTC";
		}
	}

	public string StorageFilePath
	{
		get
		{
			string value = (Environment.GetEnvironmentVariable("STORAGE_FILE_PATH") ?? "").Trim();
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			return "/app/data/state.json";
		}
	}

	public bool IsWeeklyAttackSummaryEnabled
	{
		get
		{
			string value = _isWeeklyAttackSummaryEnabled.Value;
			if (value != null)
			{
				return value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}
	}

	public bool IsPayoutTrackingEnabled
	{
		get
		{
			string value = _isPayoutTrackingEnabled.Value;
			if (value != null)
			{
				return value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}
	}
}
