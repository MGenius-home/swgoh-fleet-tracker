namespace Ipd.Core.Models;

public class PayoutShiftInfo
{
	public string AllyCode { get; set; }

	public string PlayerName { get; set; }

	public string PreviousUtcPayoutTime { get; set; }

	public string NewUtcPayoutTime { get; set; }

	public double ShiftDeltaHours { get; set; }
}
