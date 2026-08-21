using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class DynamicMessage : IMessage<DynamicMessage>, IMessage, IEquatable<DynamicMessage>, IDeepCloneable<DynamicMessage>
{
	private static readonly MessageParser<DynamicMessage> _parser = new MessageParser<DynamicMessage>(() => new DynamicMessage());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private string id_ = "";

	public const int TypeFieldNumber = 2;

	private string type_ = "";

	public const int DataFieldNumber = 3;

	private ByteString data_ = ByteString.Empty;

	public const int MessageIdFieldNumber = 4;

	private int messageId_;

	[DebuggerNonUserCode]
	public static MessageParser<DynamicMessage> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ResponseEnvelopeReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int MessageId
	{
		get
		{
			return messageId_;
		}
		set
		{
			messageId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public DynamicMessage()
	{
	}

	[DebuggerNonUserCode]
	public DynamicMessage(DynamicMessage other)
		: this()
	{
		id_ = other.id_;
		type_ = other.type_;
		data_ = other.data_;
		messageId_ = other.messageId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DynamicMessage Clone()
	{
		return new DynamicMessage(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DynamicMessage);
	}

	[DebuggerNonUserCode]
	public bool Equals(DynamicMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (MessageId != other.MessageId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (Type.Length != 0)
		{
			num ^= Type.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
		}
		if (MessageId != 0)
		{
			num ^= MessageId.GetHashCode();
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
		if (Id.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Id);
		}
		if (Type.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Type);
		}
		if (Data.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteBytes(Data);
		}
		if (MessageId != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(MessageId);
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
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (Type.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Type);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (MessageId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MessageId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(DynamicMessage other)
	{
		if (other != null)
		{
			if (other.Id.Length != 0)
			{
				Id = other.Id;
			}
			if (other.Type.Length != 0)
			{
				Type = other.Type;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
			}
			if (other.MessageId != 0)
			{
				MessageId = other.MessageId;
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
				Id = input.ReadString();
				break;
			case 18u:
				Type = input.ReadString();
				break;
			case 26u:
				Data = input.ReadBytes();
				break;
			case 32u:
				MessageId = input.ReadInt32();
				break;
			}
		}
	}
}
