using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class SlimPlayerArenaProfileRequest : IMessage<SlimPlayerArenaProfileRequest>, IMessage, IEquatable<SlimPlayerArenaProfileRequest>, IDeepCloneable<SlimPlayerArenaProfileRequest>
{
	private static readonly MessageParser<SlimPlayerArenaProfileRequest> _parser = new MessageParser<SlimPlayerArenaProfileRequest>(() => new SlimPlayerArenaProfileRequest());

	private UnknownFieldSet _unknownFields;

	public const int PlayerIdFieldNumber = 1;

	private string playerId_ = "";

	public const int AllyCodeFieldNumber = 2;

	private string allyCode_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<SlimPlayerArenaProfileRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => SlimPlayerProfileReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public string AllyCode
	{
		get
		{
			return allyCode_;
		}
		set
		{
			allyCode_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileRequest()
	{
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileRequest(SlimPlayerArenaProfileRequest other)
		: this()
	{
		playerId_ = other.playerId_;
		allyCode_ = other.allyCode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SlimPlayerArenaProfileRequest Clone()
	{
		return new SlimPlayerArenaProfileRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SlimPlayerArenaProfileRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SlimPlayerArenaProfileRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlayerId != other.PlayerId)
		{
			return false;
		}
		if (AllyCode != other.AllyCode)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (PlayerId.Length != 0)
		{
			num ^= PlayerId.GetHashCode();
		}
		if (AllyCode.Length != 0)
		{
			num ^= AllyCode.GetHashCode();
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
		if (PlayerId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(PlayerId);
		}
		if (AllyCode.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(AllyCode);
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
		if (PlayerId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlayerId);
		}
		if (AllyCode.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AllyCode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SlimPlayerArenaProfileRequest other)
	{
		if (other != null)
		{
			if (other.PlayerId.Length != 0)
			{
				PlayerId = other.PlayerId;
			}
			if (other.AllyCode.Length != 0)
			{
				AllyCode = other.AllyCode;
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
				PlayerId = input.ReadString();
				break;
			case 18u:
				AllyCode = input.ReadString();
				break;
			}
		}
	}
}
