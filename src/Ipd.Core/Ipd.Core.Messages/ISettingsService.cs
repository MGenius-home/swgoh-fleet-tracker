namespace Ipd.Core.Messages;

public interface ISettingsService
{
	string MessageFormatOnStatus { get; }

	string MessageFormatOnClimb { get; }

	string MessageFormatOnDrop { get; }

	int TagOnDropRankLimit { get; }

	int TagOnClimbRankLimit { get; }

	int TagOnDropPayoutLimitMins { get; }

	string PayoutWebHookUrl { get; }

	bool PostFullPayoutListOnChange { get; }

	string WeeklyAttackSummaryCron { get; }

	string StatusMessageCron { get; }

	int PollIntervalSeconds { get; }

	bool IsWeeklyAttackSummaryEnabled { get; }

	bool IsPayoutTrackingEnabled { get; }

	string StorageFilePath { get; }
}
