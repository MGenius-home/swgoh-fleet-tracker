using System.Collections.Generic;

namespace Ipd.Core.Messages;

public class MessageMap
{
	public const string NAME = "%NAME%";

	public const string USER_ICON = "%USER_ICON%";

	public const string PLAYER_NAME = "%PLAYER_NAME%";

	public const string TAG_ON_CLIMB = "%TAG_ON_CLIMB%";

	public const string TAG_ON_DROP = "%TAG_ON_DROP%";

	public const string ALLY_CODE = "%ALLY_CODE%";

	public const string CURRENT_RANK = "%CURRENT_RANK%";

	public const string PREVIOUS_RANK = "%PREVIOUS_RANK%";

	public const string TIME_TO_PO = "%TIME_TO_PO%";

	public Dictionary<string, string> Values { get; private set; }

	public string Name
	{
		set
		{
			Values["%NAME%"] = value;
		}
	}

	public string UserIcon
	{
		set
		{
			Values["%USER_ICON%"] = value;
		}
	}

	public string AllyCode
	{
		set
		{
			Values["%ALLY_CODE%"] = value;
		}
	}

	public string PlayerName
	{
		set
		{
			Values["%PLAYER_NAME%"] = value;
		}
	}

	public string CurrentRank
	{
		set
		{
			Values["%CURRENT_RANK%"] = value;
		}
	}

	public string PreviousRank
	{
		set
		{
			Values["%PREVIOUS_RANK%"] = value;
		}
	}

	public string TimeToPo
	{
		set
		{
			Values["%TIME_TO_PO%"] = value;
		}
	}

	public string TagOnDrop
	{
		set
		{
			Values["%TAG_ON_DROP%"] = value;
		}
	}

	public string TagOnClimb
	{
		set
		{
			Values["%TAG_ON_CLIMB%"] = value;
		}
	}

	public MessageMap()
	{
		Values = new Dictionary<string, string>
		{
			{ "%NAME%", "" },
			{ "%USER_ICON%", "" },
			{ "%PLAYER_NAME%", "" },
			{ "%CURRENT_RANK%", "" },
			{ "%PREVIOUS_RANK%", "" },
			{ "%TIME_TO_PO%", "" },
			{ "%ALLY_CODE%", "" },
			{ "%TAG_ON_DROP%", "" },
			{ "%TAG_ON_CLIMB%", "" }
		};
	}
}
