using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class SlimPlayerArenaProfileResponse : IMessage<SlimPlayerArenaProfileResponse>, IMessage, IEquatable<SlimPlayerArenaProfileResponse>, IDeepCloneable<SlimPlayerArenaProfileResponse>
{
	private static readonly MessageParser<SlimPlayerArenaProfileResponse> _parser = new MessageParser<SlimPlayerArenaProfileResponse>(() => new SlimPlayerArenaProfileResponse());

	private UnknownFieldSet _unknownFields;

	public const int NameFieldNumber = 1;

	private string name_ = "";

	public const int LevelFieldNumber = 2;

	private int level_;

	public const int AllyCodeFieldNumber = 3;

	private long allyCode_;

	public const int PlayerIdFieldNumber = 4;

	private string playerId_ = "";

	public const int PvpProfileFieldNumber = 5;

	private static readonly FieldCodec<PlayerPvpProfile> _repeated_pvpProfile_codec = FieldCodec.ForMessage(42u, PlayerPvpProfile.Parser);

	private readonly RepeatedField<PlayerPvpProfile> pvpProfile_ = new RepeatedField<PlayerPvpProfile>();

	public const int LocalTimeZoneOffsetMinutesFieldNumber = 6;

	private int localTimeZoneOffsetMinutes_;

	[DebuggerNonUserCode]
	public static MessageParser<SlimPlayerArenaProfileResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SlimPlayerProfileReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long AllyCode
	{
		get
		{
			return allyCode_;
		}
		set
		{
			allyCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string PlayerId
	{
		get
		{
			return playerId_;
		}
		set
		{
			playerId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<PlayerPvpProfile> PvpProfile => pvpProfile_;

	[DebuggerNonUserCode]
	public int LocalTimeZoneOffsetMinutes
	{
		get
		{
			return localTimeZoneOffsetMinutes_;
		}
		set
		{
			localTimeZoneOffsetMinutes_ = value;
		}
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileResponse()
	{
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileResponse(SlimPlayerArenaProfileResponse other)
		: this()
	{
		name_ = other.name_;
		level_ = other.level_;
		allyCode_ = other.allyCode_;
		playerId_ = other.playerId_;
		pvpProfile_ = other.pvpProfile_.Clone();
		localTimeZoneOffsetMinutes_ = other.localTimeZoneOffsetMinutes_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileResponse Clone()
	{
		return new SlimPlayerArenaProfileResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SlimPlayerArenaProfileResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(SlimPlayerArenaProfileResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (AllyCode != other.AllyCode)
		{
			return false;
		}
		if (PlayerId != other.PlayerId)
		{
			return false;
		}
		if (!pvpProfile_.Equals(other.pvpProfile_))
		{
			return false;
		}
		if (LocalTimeZoneOffsetMinutes != other.LocalTimeZoneOffsetMinutes)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (AllyCode != 0L)
		{
			num ^= AllyCode.GetHashCode();
		}
		if (PlayerId.Length != 0)
		{
			num ^= PlayerId.GetHashCode();
		}
		num ^= pvpProfile_.GetHashCode();
		if (LocalTimeZoneOffsetMinutes != 0)
		{
			num ^= LocalTimeZoneOffsetMinutes.GetHashCode();
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
		if (Name.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Name);
		}
		if (Level != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Level);
		}
		if (AllyCode != 0L)
		{
			output.WriteRawTag(24);
			output.WriteInt64(AllyCode);
		}
		if (PlayerId.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(PlayerId);
		}
		pvpProfile_.WriteTo(output, _repeated_pvpProfile_codec);
		if (LocalTimeZoneOffsetMinutes != 0)
		{
			output.WriteRawTag(48);
			output.WriteSInt32(LocalTimeZoneOffsetMinutes);
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
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (AllyCode != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(AllyCode);
		}
		if (PlayerId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlayerId);
		}
		num += pvpProfile_.CalculateSize(_repeated_pvpProfile_codec);
		if (LocalTimeZoneOffsetMinutes != 0)
		{
			num += 1 + CodedOutputStream.ComputeSInt32Size(LocalTimeZoneOffsetMinutes);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SlimPlayerArenaProfileResponse other)
	{
		if (other != null)
		{
			if (other.Name.Length != 0)
			{
				Name = other.Name;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.AllyCode != 0L)
			{
				AllyCode = other.AllyCode;
			}
			if (other.PlayerId.Length != 0)
			{
				PlayerId = other.PlayerId;
			}
			pvpProfile_.Add(other.pvpProfile_);
			if (other.LocalTimeZoneOffsetMinutes != 0)
			{
				LocalTimeZoneOffsetMinutes = other.LocalTimeZoneOffsetMinutes;
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
			case 10u:
				Name = input.ReadString();
				break;
			case 16u:
				Level = input.ReadInt32();
				break;
			case 24u:
				AllyCode = input.ReadInt64();
				break;
			case 34u:
				PlayerId = input.ReadString();
				break;
			case 42u:
				pvpProfile_.AddEntriesFrom(input, _repeated_pvpProfile_codec);
				break;
			case 48u:
				LocalTimeZoneOffsetMinutes = input.ReadSInt32();
				break;
			}
		}
	}
}
