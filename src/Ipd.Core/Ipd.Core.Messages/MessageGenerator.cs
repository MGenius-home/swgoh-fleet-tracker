using System.Collections.Generic;
using MoreLinq;

namespace Ipd.Core.Messages;

public static class MessageGenerator
{
	public const string DEFAULT_MESSAGE_STATUS = "%USER_ICON%`%PLAYER_NAME%` is at %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public const string DEFAULT_MESSAGE_CLIMB = "%TAG_ON_CLIMB%%USER_ICON%`%PLAYER_NAME%` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public const string DEFAULT_MESSAGE_DROP = "%TAG_ON_DROP%%USER_ICON%`%PLAYER_NAME%` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";

	public static string GenerateStatusMessage(MessageMap map, string customMessage = null)
	{
		string msg = customMessage ?? "%USER_ICON%`%PLAYER_NAME%` is at %CURRENT_RANK%. payout in `%TIME_TO_PO%`";
		map.Values.ForEach(delegate(KeyValuePair<string, string> kv)
		{
			msg = msg.Replace(kv.Key, kv.Value.Trim());
		});
		return msg;
	}

	public static string GenerateMessageOnClimb(MessageMap map, string customMessage = null)
	{
		string msg = customMessage ?? "%TAG_ON_CLIMB%%USER_ICON%`%PLAYER_NAME%` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";
		map.Values.ForEach(delegate(KeyValuePair<string, string> kv)
		{
			msg = msg.Replace(kv.Key, kv.Value.Trim());
		});
		return msg;
	}

	public static string GenerateMessageOnDrop(MessageMap map, string customMessage = null)
	{
		string msg = customMessage ?? "%TAG_ON_DROP%%USER_ICON%`%PLAYER_NAME%` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`";
		map.Values.ForEach(delegate(KeyValuePair<string, string> kv)
		{
			msg = msg.Replace(kv.Key, kv.Value.Trim());
		});
		return msg;
	}
}
