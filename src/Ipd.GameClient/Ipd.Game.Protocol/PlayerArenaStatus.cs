using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class PlayerArenaStatus : IMessage<PlayerArenaStatus>, IMessage, IEquatable<PlayerArenaStatus>, IDeepCloneable<PlayerArenaStatus>
{
	private static readonly MessageParser<PlayerArenaStatus> _parser = new MessageParser<PlayerArenaStatus>(() => new PlayerArenaStatus());

	private UnknownFieldSet _unknownFields;

	public const int ArenaTypeFieldNumber = 1;

	private PlayerArenaType arenaType_;

	public const int PlaceFieldNumber = 2;

	private int place_;

	[DebuggerNonUserCode]
	public static MessageParser<PlayerArenaStatus> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PlayerProfileResponseReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public PlayerArenaType ArenaType
	{
		get
		{
			return arenaType_;
		}
		set
		{
			arenaType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Place
	{
		get
		{
			return place_;
		}
		set
		{
			place_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PlayerArenaStatus()
	{
	}

	[DebuggerNonUserCode]
	public PlayerArenaStatus(PlayerArenaStatus other)
		: this()
	{
		arenaType_ = other.arenaType_;
		place_ = other.place_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PlayerArenaStatus Clone()
	{
		return new PlayerArenaStatus(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerArenaStatus);
	}

	[DebuggerNonUserCode]
	public bool Equals(PlayerArenaStatus other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ArenaType != other.ArenaType)
		{
			return false;
		}
		if (Place != other.Place)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ArenaType != 0)
		{
			num ^= ArenaType.GetHashCode();
		}
		if (Place != 0)
		{
			num ^= Place.GetHashCode();
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
		if (ArenaType != 0)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)ArenaType);
		}
		if (Place != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Place);
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
		if (ArenaType != 0)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ArenaType);
		}
		if (Place != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Place);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PlayerArenaStatus other)
	{
		if (other != null)
		{
			if (other.ArenaType != 0)
			{
				ArenaType = other.ArenaType;
			}
			if (other.Place != 0)
			{
				Place = other.Place;
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
				arenaType_ = (PlayerArenaType)input.ReadEnum();
				break;
			case 16u:
				Place = input.ReadInt32();
				break;
			}
		}
	}
}
