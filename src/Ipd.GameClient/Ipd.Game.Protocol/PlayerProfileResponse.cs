using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class PlayerProfileResponse : IMessage<PlayerProfileResponse>, IMessage, IEquatable<PlayerProfileResponse>, IDeepCloneable<PlayerProfileResponse>
{
	private static readonly MessageParser<PlayerProfileResponse> _parser = new MessageParser<PlayerProfileResponse>(() => new PlayerProfileResponse());

	private UnknownFieldSet _unknownFields;

	public const int NameFieldNumber = 1;

	private string name_ = "";

	public const int ArenaStatusesFieldNumber = 17;

	private static readonly FieldCodec<PlayerArenaStatus> _repeated_arenaStatuses_codec = FieldCodec.ForMessage(138u, PlayerArenaStatus.Parser);

	private readonly RepeatedField<PlayerArenaStatus> arenaStatuses_ = new RepeatedField<PlayerArenaStatus>();

	[DebuggerNonUserCode]
	public static MessageParser<PlayerProfileResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PlayerProfileResponseReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<PlayerArenaStatus> ArenaStatuses => arenaStatuses_;

	[DebuggerNonUserCode]
	public PlayerProfileResponse()
	{
	}

	[DebuggerNonUserCode]
	public PlayerProfileResponse(PlayerProfileResponse other)
		: this()
	{
		name_ = other.name_;
		arenaStatuses_ = other.arenaStatuses_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PlayerProfileResponse Clone()
	{
		return new PlayerProfileResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerProfileResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(PlayerProfileResponse other)
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
		if (!arenaStatuses_.Equals(other.arenaStatuses_))
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
		num ^= arenaStatuses_.GetHashCode();
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
		arenaStatuses_.WriteTo(output, _repeated_arenaStatuses_codec);
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
		num += arenaStatuses_.CalculateSize(_repeated_arenaStatuses_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PlayerProfileResponse other)
	{
		if (other != null)
		{
			if (other.Name.Length != 0)
			{
				Name = other.Name;
			}
			arenaStatuses_.Add(other.arenaStatuses_);
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
			case 138u:
				arenaStatuses_.AddEntriesFrom(input, _repeated_arenaStatuses_codec);
				break;
			}
		}
	}
}
