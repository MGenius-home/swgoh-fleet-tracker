namespace Ipd.Core.Messages;

public interface ISettingsService
{
	string MessageFormatOnStatus { get; }

	string MessageFormatOnClimb { get; }

	string MessageFormatOnDrop { get; }

	bool IsStatusMessageDisabled { get; }

	int TagOnDropRankLimit { get; }

	int TagOnClimbRankLimit { get; }

	int TagOnDropPayoutLimitMins { get; }
}
