using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class PlayerPvpProfile : IMessage<PlayerPvpProfile>, IMessage, IEquatable<PlayerPvpProfile>, IDeepCloneable<PlayerPvpProfile>
{
	private static readonly MessageParser<PlayerPvpProfile> _parser = new MessageParser<PlayerPvpProfile>(() => new PlayerPvpProfile());

	private UnknownFieldSet _unknownFields;

	public const int TabFieldNumber = 1;

	private PlayerProfileTab tab_;

	public const int RankFieldNumber = 2;

	private int rank_;

	public const int EventIdFieldNumber = 4;

	private string eventId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PlayerPvpProfile> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SlimPlayerProfileReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PlayerProfileTab Tab
	{
		get
		{
			return tab_;
		}
		set
		{
			tab_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Rank
	{
		get
		{
			return rank_;
		}
		set
		{
			rank_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string EventId
	{
		get
		{
			return eventId_;
		}
		set
		{
			eventId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PlayerPvpProfile()
	{
	}

	[DebuggerNonUserCode]
	public PlayerPvpProfile(PlayerPvpProfile other)
		: this()
	{
		tab_ = other.tab_;
		rank_ = other.rank_;
		eventId_ = other.eventId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PlayerPvpProfile Clone()
	{
		return new PlayerPvpProfile(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerPvpProfile);
	}

	[DebuggerNonUserCode]
	public bool Equals(PlayerPvpProfile other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Tab != other.Tab)
		{
			return false;
		}
		if (Rank != other.Rank)
		{
			return false;
		}
		if (EventId != other.EventId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Tab != 0)
		{
			num ^= Tab.GetHashCode();
		}
		if (Rank != 0)
		{
			num ^= Rank.GetHashCode();
		}
		if (EventId.Length != 0)
		{
			num ^= EventId.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		if (Tab != 0)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)Tab);
		}
		if (Rank != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Rank);
		}
		if (EventId.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(EventId);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Tab != 0)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Tab);
		}
		if (Rank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Rank);
		}
		if (EventId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EventId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PlayerPvpProfile other)
	{
		if (other != null)
		{
			if (other.Tab != 0)
			{
				Tab = other.Tab;
			}
			if (other.Rank != 0)
			{
				Rank = other.Rank;
			}
			if (other.EventId.Length != 0)
			{
				EventId = other.EventId;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
				break;
			case 8u:
				tab_ = (PlayerProfileTab)input.ReadEnum();
				break;
			case 16u:
				Rank = input.ReadInt32();
				break;
			case 34u:
				EventId = input.ReadString();
				break;
			}
		}
	}
}
