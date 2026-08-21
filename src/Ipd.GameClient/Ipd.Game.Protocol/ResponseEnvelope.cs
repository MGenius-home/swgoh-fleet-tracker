using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class ResponseEnvelope : IMessage<ResponseEnvelope>, IMessage, IEquatable<ResponseEnvelope>, IDeepCloneable<ResponseEnvelope>
{
	private static readonly MessageParser<ResponseEnvelope> _parser = new MessageParser<ResponseEnvelope>(() => new ResponseEnvelope());

	private UnknownFieldSet _unknownFields;

	public const int CorrelationIdFieldNumber = 1;

	private int correlationId_;

	public const int CurrentServerTimeFieldNumber = 2;

	private long currentServerTime_;

	public const int PayloadFieldNumber = 4;

	private ByteString payload_ = ByteString.Empty;

	public const int CodeFieldNumber = 5;

	private ResponseCode code_;

	public const int MessageFieldNumber = 6;

	private string message_ = "";

	public const int ContentEncodingFieldNumber = 7;

	private ContentEncoding contentEncoding_;

	public const int StackTraceFieldNumber = 8;

	private string stackTrace_ = "";

	public const int DynamicMessageFieldNumber = 9;

	private static readonly FieldCodec<DynamicMessage> _repeated_dynamicMessage_codec = FieldCodec.ForMessage(74u, Ipd.Game.Protocol.DynamicMessage.Parser);

	private readonly RepeatedField<DynamicMessage> dynamicMessage_ = new RepeatedField<DynamicMessage>();

	public const int MaintenanceMessageFieldNumber = 10;

	private string maintenanceMessage_ = "";

	public const int MaintenanceLinkFieldNumber = 11;

	private string maintenanceLink_ = "";

	public const int SubCodeFieldNumber = 12;

	private int subCode_;

	[DebuggerNonUserCode]
	public static MessageParser<ResponseEnvelope> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ResponseEnvelopeReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int CorrelationId
	{
		get
		{
			return correlationId_;
		}
		set
		{
			correlationId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long CurrentServerTime
	{
		get
		{
			return currentServerTime_;
		}
		set
		{
			currentServerTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Payload
	{
		get
		{
			return payload_;
		}
		set
		{
			payload_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ResponseCode Code
	{
		get
		{
			return code_;
		}
		set
		{
			code_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Message
	{
		get
		{
			return message_;
		}
		set
		{
			message_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ContentEncoding ContentEncoding
	{
		get
		{
			return contentEncoding_;
		}
		set
		{
			contentEncoding_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string StackTrace
	{
		get
		{
			return stackTrace_;
		}
		set
		{
			stackTrace_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<DynamicMessage> DynamicMessage => dynamicMessage_;

	[DebuggerNonUserCode]
	public string MaintenanceMessage
	{
		get
		{
			return maintenanceMessage_;
		}
		set
		{
			maintenanceMessage_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string MaintenanceLink
	{
		get
		{
			return maintenanceLink_;
		}
		set
		{
			maintenanceLink_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int SubCode
	{
		get
		{
			return subCode_;
		}
		set
		{
			subCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ResponseEnvelope()
	{
	}

	[DebuggerNonUserCode]
	public ResponseEnvelope(ResponseEnvelope other)
		: this()
	{
		correlationId_ = other.correlationId_;
		currentServerTime_ = other.currentServerTime_;
		payload_ = other.payload_;
		code_ = other.code_;
		message_ = other.message_;
		contentEncoding_ = other.contentEncoding_;
		stackTrace_ = other.stackTrace_;
		dynamicMessage_ = other.dynamicMessage_.Clone();
		maintenanceMessage_ = other.maintenanceMessage_;
		maintenanceLink_ = other.maintenanceLink_;
		subCode_ = other.subCode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ResponseEnvelope Clone()
	{
		return new ResponseEnvelope(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ResponseEnvelope);
	}

	[DebuggerNonUserCode]
	public bool Equals(ResponseEnvelope other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CorrelationId != other.CorrelationId)
		{
			return false;
		}
		if (CurrentServerTime != other.CurrentServerTime)
		{
			return false;
		}
		if (Payload != other.Payload)
		{
			return false;
		}
		if (Code != other.Code)
		{
			return false;
		}
		if (Message != other.Message)
		{
			return false;
		}
		if (ContentEncoding != other.ContentEncoding)
		{
			return false;
		}
		if (StackTrace != other.StackTrace)
		{
			return false;
		}
		if (!dynamicMessage_.Equals(other.dynamicMessage_))
		{
			return false;
		}
		if (MaintenanceMessage != other.MaintenanceMessage)
		{
			return false;
		}
		if (MaintenanceLink != other.MaintenanceLink)
		{
			return false;
		}
		if (SubCode != other.SubCode)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CorrelationId != 0)
		{
			num ^= CorrelationId.GetHashCode();
		}
		if (CurrentServerTime != 0L)
		{
			num ^= CurrentServerTime.GetHashCode();
		}
		if (Payload.Length != 0)
		{
			num ^= Payload.GetHashCode();
		}
		if (Code != 0)
		{
			num ^= Code.GetHashCode();
		}
		if (Message.Length != 0)
		{
			num ^= Message.GetHashCode();
		}
		if (ContentEncoding != 0)
		{
			num ^= ContentEncoding.GetHashCode();
		}
		if (StackTrace.Length != 0)
		{
			num ^= StackTrace.GetHashCode();
		}
		num ^= dynamicMessage_.GetHashCode();
		if (MaintenanceMessage.Length != 0)
		{
			num ^= MaintenanceMessage.GetHashCode();
		}
		if (MaintenanceLink.Length != 0)
		{
			num ^= MaintenanceLink.GetHashCode();
		}
		if (SubCode != 0)
		{
			num ^= SubCode.GetHashCode();
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
		if (CorrelationId != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(CorrelationId);
		}
		if (CurrentServerTime != 0L)
		{
			output.WriteRawTag(16);
			output.WriteInt64(CurrentServerTime);
		}
		if (Payload.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteBytes(Payload);
		}
		if (Code != 0)
		{
			output.WriteRawTag(40);
			output.WriteEnum((int)Code);
		}
		if (Message.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(Message);
		}
		if (ContentEncoding != 0)
		{
			output.WriteRawTag(56);
			output.WriteEnum((int)ContentEncoding);
		}
		if (StackTrace.Length != 0)
		{
			output.WriteRawTag(66);
			output.WriteString(StackTrace);
		}
		dynamicMessage_.WriteTo(output, _repeated_dynamicMessage_codec);
		if (MaintenanceMessage.Length != 0)
		{
			output.WriteRawTag(82);
			output.WriteString(MaintenanceMessage);
		}
		if (MaintenanceLink.Length != 0)
		{
			output.WriteRawTag(90);
			output.WriteString(MaintenanceLink);
		}
		if (SubCode != 0)
		{
			output.WriteRawTag(96);
			output.WriteInt32(SubCode);
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
		if (CorrelationId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CorrelationId);
		}
		if (CurrentServerTime != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CurrentServerTime);
		}
		if (Payload.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Payload);
		}
		if (Code != 0)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Code);
		}
		if (Message.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Message);
		}
		if (ContentEncoding != 0)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ContentEncoding);
		}
		if (StackTrace.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(StackTrace);
		}
		num += dynamicMessage_.CalculateSize(_repeated_dynamicMessage_codec);
		if (MaintenanceMessage.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(MaintenanceMessage);
		}
		if (MaintenanceLink.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(MaintenanceLink);
		}
		if (SubCode != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubCode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ResponseEnvelope other)
	{
		if (other != null)
		{
			if (other.CorrelationId != 0)
			{
				CorrelationId = other.CorrelationId;
			}
			if (other.CurrentServerTime != 0L)
			{
				CurrentServerTime = other.CurrentServerTime;
			}
			if (other.Payload.Length != 0)
			{
				Payload = other.Payload;
			}
			if (other.Code != 0)
			{
				Code = other.Code;
			}
			if (other.Message.Length != 0)
			{
				Message = other.Message;
			}
			if (other.ContentEncoding != 0)
			{
				ContentEncoding = other.ContentEncoding;
			}
			if (other.StackTrace.Length != 0)
			{
				StackTrace = other.StackTrace;
			}
			dynamicMessage_.Add(other.dynamicMessage_);
			if (other.MaintenanceMessage.Length != 0)
			{
				MaintenanceMessage = other.MaintenanceMessage;
			}
			if (other.MaintenanceLink.Length != 0)
			{
				MaintenanceLink = other.MaintenanceLink;
			}
			if (other.SubCode != 0)
			{
				SubCode = other.SubCode;
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
				CorrelationId = input.ReadInt32();
				break;
			case 16u:
				CurrentServerTime = input.ReadInt64();
				break;
			case 34u:
				Payload = input.ReadBytes();
				break;
			case 40u:
				code_ = (ResponseCode)input.ReadEnum();
				break;
			case 50u:
				Message = input.ReadString();
				break;
			case 56u:
				contentEncoding_ = (ContentEncoding)input.ReadEnum();
				break;
			case 66u:
				StackTrace = input.ReadString();
				break;
			case 74u:
				dynamicMessage_.AddEntriesFrom(input, _repeated_dynamicMessage_codec);
				break;
			case 82u:
				MaintenanceMessage = input.ReadString();
				break;
			case 90u:
				MaintenanceLink = input.ReadString();
				break;
			case 96u:
				SubCode = input.ReadInt32();
				break;
			}
		}
	}
}
