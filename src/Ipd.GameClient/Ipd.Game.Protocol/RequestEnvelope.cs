using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public sealed class RequestEnvelope : IMessage<RequestEnvelope>, IMessage, IEquatable<RequestEnvelope>, IDeepCloneable<RequestEnvelope>
{
	private static readonly MessageParser<RequestEnvelope> _parser = new MessageParser<RequestEnvelope>(() => new RequestEnvelope());

	private UnknownFieldSet _unknownFields;

	public const int CorrelationIdFieldNumber = 1;

	private int correlationId_;

	public const int ServiceNameFieldNumber = 4;

	private string serviceName_ = "";

	public const int MethodNameFieldNumber = 5;

	private string methodName_ = "";

	public const int PayloadFieldNumber = 6;

	private ByteString payload_ = ByteString.Empty;

	public const int AuthIdFieldNumber = 7;

	private string authId_ = "";

	public const int AuthTokenFieldNumber = 8;

	private string authToken_ = "";

	public const int ClientVersionFieldNumber = 9;

	private int clientVersion_;

	public const int ClientStartupTimestampFieldNumber = 11;

	private long clientStartupTimestamp_;

	public const int PlatformFieldNumber = 12;

	private string platform_ = "";

	public const int RegionFieldNumber = 13;

	private string region_ = "";

	public const int ClientExternalVersionFieldNumber = 14;

	private string clientExternalVersion_ = "";

	public const int ClientInternalVersionFieldNumber = 15;

	private string clientInternalVersion_ = "";

	public const int RequestIdFieldNumber = 16;

	private string requestId_ = "";

	public const int AcceptEncodingFieldNumber = 17;

	private AcceptEncoding acceptEncoding_;

	public const int FlagFieldNumber = 18;

	private static readonly FieldCodec<string> _repeated_flag_codec = FieldCodec.ForString(146u);

	private readonly RepeatedField<string> flag_ = new RepeatedField<string>();

	public const int TelemetryEventFieldNumber = 19;

	private static readonly FieldCodec<string> _repeated_telemetryEvent_codec = FieldCodec.ForString(154u);

	private readonly RepeatedField<string> telemetryEvent_ = new RepeatedField<string>();

	public const int CurrentClientTimeFieldNumber = 20;

	private long currentClientTime_;

	public const int NimbleSessionIdFieldNumber = 21;

	private string nimbleSessionId_ = "";

	public const int TimezoneFieldNumber = 22;

	private string timezone_ = "";

	public const int FirmwareVersionFieldNumber = 23;

	private string firmwareVersion_ = "";

	public const int CarrierFieldNumber = 24;

	private string carrier_ = "";

	public const int NetworkAccessFieldNumber = 25;

	private string networkAccess_ = "";

	public const int HardwareIdFieldNumber = 26;

	private string hardwareId_ = "";

	public const int AdvertiserIdFieldNumber = 27;

	private string advertiserId_ = "";

	public const int VendorIdFieldNumber = 28;

	private string vendorId_ = "";

	public const int AndroidIdFieldNumber = 29;

	private string androidId_ = "";

	public const int JailbrokenFlagFieldNumber = 30;

	private int jailbrokenFlag_;

	public const int PiracyFlagFieldNumber = 31;

	private int piracyFlag_;

	public const int SynergyIdFieldNumber = 32;

	private string synergyId_ = "";

	public const int DeviceModelFieldNumber = 33;

	private string deviceModel_ = "";

	public const int DeviceIdFieldNumber = 34;

	private string deviceId_ = "";

	public const int ApplicationFieldNumber = 37;

	private string application_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<RequestEnvelope> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RequestEnvelopeReflection.Descriptor.MessageTypes[0];

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
	public string ServiceName
	{
		get
		{
			return serviceName_;
		}
		set
		{
			serviceName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string MethodName
	{
		get
		{
			return methodName_;
		}
		set
		{
			methodName_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string AuthId
	{
		get
		{
			return authId_;
		}
		set
		{
			authId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string AuthToken
	{
		get
		{
			return authToken_;
		}
		set
		{
			authToken_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int ClientVersion
	{
		get
		{
			return clientVersion_;
		}
		set
		{
			clientVersion_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long ClientStartupTimestamp
	{
		get
		{
			return clientStartupTimestamp_;
		}
		set
		{
			clientStartupTimestamp_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Platform
	{
		get
		{
			return platform_;
		}
		set
		{
			platform_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Region
	{
		get
		{
			return region_;
		}
		set
		{
			region_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ClientExternalVersion
	{
		get
		{
			return clientExternalVersion_;
		}
		set
		{
			clientExternalVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ClientInternalVersion
	{
		get
		{
			return clientInternalVersion_;
		}
		set
		{
			clientInternalVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string RequestId
	{
		get
		{
			return requestId_;
		}
		set
		{
			requestId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public AcceptEncoding AcceptEncoding
	{
		get
		{
			return acceptEncoding_;
		}
		set
		{
			acceptEncoding_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> Flag => flag_;

	[DebuggerNonUserCode]
	public RepeatedField<string> TelemetryEvent => telemetryEvent_;

	[DebuggerNonUserCode]
	public long CurrentClientTime
	{
		get
		{
			return currentClientTime_;
		}
		set
		{
			currentClientTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string NimbleSessionId
	{
		get
		{
			return nimbleSessionId_;
		}
		set
		{
			nimbleSessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Timezone
	{
		get
		{
			return timezone_;
		}
		set
		{
			timezone_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string FirmwareVersion
	{
		get
		{
			return firmwareVersion_;
		}
		set
		{
			firmwareVersion_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Carrier
	{
		get
		{
			return carrier_;
		}
		set
		{
			carrier_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string NetworkAccess
	{
		get
		{
			return networkAccess_;
		}
		set
		{
			networkAccess_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string HardwareId
	{
		get
		{
			return hardwareId_;
		}
		set
		{
			hardwareId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string AdvertiserId
	{
		get
		{
			return advertiserId_;
		}
		set
		{
			advertiserId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string VendorId
	{
		get
		{
			return vendorId_;
		}
		set
		{
			vendorId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string AndroidId
	{
		get
		{
			return androidId_;
		}
		set
		{
			androidId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int JailbrokenFlag
	{
		get
		{
			return jailbrokenFlag_;
		}
		set
		{
			jailbrokenFlag_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int PiracyFlag
	{
		get
		{
			return piracyFlag_;
		}
		set
		{
			piracyFlag_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string SynergyId
	{
		get
		{
			return synergyId_;
		}
		set
		{
			synergyId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string DeviceModel
	{
		get
		{
			return deviceModel_;
		}
		set
		{
			deviceModel_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string DeviceId
	{
		get
		{
			return deviceId_;
		}
		set
		{
			deviceId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Application
	{
		get
		{
			return application_;
		}
		set
		{
			application_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RequestEnvelope()
	{
	}

	[DebuggerNonUserCode]
	public RequestEnvelope(RequestEnvelope other)
		: this()
	{
		correlationId_ = other.correlationId_;
		serviceName_ = other.serviceName_;
		methodName_ = other.methodName_;
		payload_ = other.payload_;
		authId_ = other.authId_;
		authToken_ = other.authToken_;
		clientVersion_ = other.clientVersion_;
		clientStartupTimestamp_ = other.clientStartupTimestamp_;
		platform_ = other.platform_;
		region_ = other.region_;
		clientExternalVersion_ = other.clientExternalVersion_;
		clientInternalVersion_ = other.clientInternalVersion_;
		requestId_ = other.requestId_;
		acceptEncoding_ = other.acceptEncoding_;
		flag_ = other.flag_.Clone();
		telemetryEvent_ = other.telemetryEvent_.Clone();
		currentClientTime_ = other.currentClientTime_;
		nimbleSessionId_ = other.nimbleSessionId_;
		timezone_ = other.timezone_;
		firmwareVersion_ = other.firmwareVersion_;
		carrier_ = other.carrier_;
		networkAccess_ = other.networkAccess_;
		hardwareId_ = other.hardwareId_;
		advertiserId_ = other.advertiserId_;
		vendorId_ = other.vendorId_;
		androidId_ = other.androidId_;
		jailbrokenFlag_ = other.jailbrokenFlag_;
		piracyFlag_ = other.piracyFlag_;
		synergyId_ = other.synergyId_;
		deviceModel_ = other.deviceModel_;
		deviceId_ = other.deviceId_;
		application_ = other.application_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RequestEnvelope Clone()
	{
		return new RequestEnvelope(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RequestEnvelope);
	}

	[DebuggerNonUserCode]
	public bool Equals(RequestEnvelope other)
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
		if (ServiceName != other.ServiceName)
		{
			return false;
		}
		if (MethodName != other.MethodName)
		{
			return false;
		}
		if (Payload != other.Payload)
		{
			return false;
		}
		if (AuthId != other.AuthId)
		{
			return false;
		}
		if (AuthToken != other.AuthToken)
		{
			return false;
		}
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		if (ClientStartupTimestamp != other.ClientStartupTimestamp)
		{
			return false;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		if (Region != other.Region)
		{
			return false;
		}
		if (ClientExternalVersion != other.ClientExternalVersion)
		{
			return false;
		}
		if (ClientInternalVersion != other.ClientInternalVersion)
		{
			return false;
		}
		if (RequestId != other.RequestId)
		{
			return false;
		}
		if (AcceptEncoding != other.AcceptEncoding)
		{
			return false;
		}
		if (!flag_.Equals(other.flag_))
		{
			return false;
		}
		if (!telemetryEvent_.Equals(other.telemetryEvent_))
		{
			return false;
		}
		if (CurrentClientTime != other.CurrentClientTime)
		{
			return false;
		}
		if (NimbleSessionId != other.NimbleSessionId)
		{
			return false;
		}
		if (Timezone != other.Timezone)
		{
			return false;
		}
		if (FirmwareVersion != other.FirmwareVersion)
		{
			return false;
		}
		if (Carrier != other.Carrier)
		{
			return false;
		}
		if (NetworkAccess != other.NetworkAccess)
		{
			return false;
		}
		if (HardwareId != other.HardwareId)
		{
			return false;
		}
		if (AdvertiserId != other.AdvertiserId)
		{
			return false;
		}
		if (VendorId != other.VendorId)
		{
			return false;
		}
		if (AndroidId != other.AndroidId)
		{
			return false;
		}
		if (JailbrokenFlag != other.JailbrokenFlag)
		{
			return false;
		}
		if (PiracyFlag != other.PiracyFlag)
		{
			return false;
		}
		if (SynergyId != other.SynergyId)
		{
			return false;
		}
		if (DeviceModel != other.DeviceModel)
		{
			return false;
		}
		if (DeviceId != other.DeviceId)
		{
			return false;
		}
		if (Application != other.Application)
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
		if (ServiceName.Length != 0)
		{
			num ^= ServiceName.GetHashCode();
		}
		if (MethodName.Length != 0)
		{
			num ^= MethodName.GetHashCode();
		}
		if (Payload.Length != 0)
		{
			num ^= Payload.GetHashCode();
		}
		if (AuthId.Length != 0)
		{
			num ^= AuthId.GetHashCode();
		}
		if (AuthToken.Length != 0)
		{
			num ^= AuthToken.GetHashCode();
		}
		if (ClientVersion != 0)
		{
			num ^= ClientVersion.GetHashCode();
		}
		if (ClientStartupTimestamp != 0L)
		{
			num ^= ClientStartupTimestamp.GetHashCode();
		}
		if (Platform.Length != 0)
		{
			num ^= Platform.GetHashCode();
		}
		if (Region.Length != 0)
		{
			num ^= Region.GetHashCode();
		}
		if (ClientExternalVersion.Length != 0)
		{
			num ^= ClientExternalVersion.GetHashCode();
		}
		if (ClientInternalVersion.Length != 0)
		{
			num ^= ClientInternalVersion.GetHashCode();
		}
		if (RequestId.Length != 0)
		{
			num ^= RequestId.GetHashCode();
		}
		if (AcceptEncoding != 0)
		{
			num ^= AcceptEncoding.GetHashCode();
		}
		num ^= flag_.GetHashCode();
		num ^= telemetryEvent_.GetHashCode();
		if (CurrentClientTime != 0L)
		{
			num ^= CurrentClientTime.GetHashCode();
		}
		if (NimbleSessionId.Length != 0)
		{
			num ^= NimbleSessionId.GetHashCode();
		}
		if (Timezone.Length != 0)
		{
			num ^= Timezone.GetHashCode();
		}
		if (FirmwareVersion.Length != 0)
		{
			num ^= FirmwareVersion.GetHashCode();
		}
		if (Carrier.Length != 0)
		{
			num ^= Carrier.GetHashCode();
		}
		if (NetworkAccess.Length != 0)
		{
			num ^= NetworkAccess.GetHashCode();
		}
		if (HardwareId.Length != 0)
		{
			num ^= HardwareId.GetHashCode();
		}
		if (AdvertiserId.Length != 0)
		{
			num ^= AdvertiserId.GetHashCode();
		}
		if (VendorId.Length != 0)
		{
			num ^= VendorId.GetHashCode();
		}
		if (AndroidId.Length != 0)
		{
			num ^= AndroidId.GetHashCode();
		}
		if (JailbrokenFlag != 0)
		{
			num ^= JailbrokenFlag.GetHashCode();
		}
		if (PiracyFlag != 0)
		{
			num ^= PiracyFlag.GetHashCode();
		}
		if (SynergyId.Length != 0)
		{
			num ^= SynergyId.GetHashCode();
		}
		if (DeviceModel.Length != 0)
		{
			num ^= DeviceModel.GetHashCode();
		}
		if (DeviceId.Length != 0)
		{
			num ^= DeviceId.GetHashCode();
		}
		if (Application.Length != 0)
		{
			num ^= Application.GetHashCode();
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
		if (ServiceName.Length != 0)
		{
			output.WriteRawTag(34);
			output.WriteString(ServiceName);
		}
		if (MethodName.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteString(MethodName);
		}
		if (Payload.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteBytes(Payload);
		}
		if (AuthId.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(AuthId);
		}
		if (AuthToken.Length != 0)
		{
			output.WriteRawTag(66);
			output.WriteString(AuthToken);
		}
		if (ClientVersion != 0)
		{
			output.WriteRawTag(72);
			output.WriteInt32(ClientVersion);
		}
		if (ClientStartupTimestamp != 0L)
		{
			output.WriteRawTag(88);
			output.WriteInt64(ClientStartupTimestamp);
		}
		if (Platform.Length != 0)
		{
			output.WriteRawTag(98);
			output.WriteString(Platform);
		}
		if (Region.Length != 0)
		{
			output.WriteRawTag(106);
			output.WriteString(Region);
		}
		if (ClientExternalVersion.Length != 0)
		{
			output.WriteRawTag(114);
			output.WriteString(ClientExternalVersion);
		}
		if (ClientInternalVersion.Length != 0)
		{
			output.WriteRawTag(122);
			output.WriteString(ClientInternalVersion);
		}
		if (RequestId.Length != 0)
		{
			output.WriteRawTag(130, 1);
			output.WriteString(RequestId);
		}
		if (AcceptEncoding != 0)
		{
			output.WriteRawTag(136, 1);
			output.WriteEnum((int)AcceptEncoding);
		}
		flag_.WriteTo(output, _repeated_flag_codec);
		telemetryEvent_.WriteTo(output, _repeated_telemetryEvent_codec);
		if (CurrentClientTime != 0L)
		{
			output.WriteRawTag(160, 1);
			output.WriteInt64(CurrentClientTime);
		}
		if (NimbleSessionId.Length != 0)
		{
			output.WriteRawTag(170, 1);
			output.WriteString(NimbleSessionId);
		}
		if (Timezone.Length != 0)
		{
			output.WriteRawTag(178, 1);
			output.WriteString(Timezone);
		}
		if (FirmwareVersion.Length != 0)
		{
			output.WriteRawTag(186, 1);
			output.WriteString(FirmwareVersion);
		}
		if (Carrier.Length != 0)
		{
			output.WriteRawTag(194, 1);
			output.WriteString(Carrier);
		}
		if (NetworkAccess.Length != 0)
		{
			output.WriteRawTag(202, 1);
			output.WriteString(NetworkAccess);
		}
		if (HardwareId.Length != 0)
		{
			output.WriteRawTag(210, 1);
			output.WriteString(HardwareId);
		}
		if (AdvertiserId.Length != 0)
		{
			output.WriteRawTag(218, 1);
			output.WriteString(AdvertiserId);
		}
		if (VendorId.Length != 0)
		{
			output.WriteRawTag(226, 1);
			output.WriteString(VendorId);
		}
		if (AndroidId.Length != 0)
		{
			output.WriteRawTag(234, 1);
			output.WriteString(AndroidId);
		}
		if (JailbrokenFlag != 0)
		{
			output.WriteRawTag(240, 1);
			output.WriteInt32(JailbrokenFlag);
		}
		if (PiracyFlag != 0)
		{
			output.WriteRawTag(248, 1);
			output.WriteInt32(PiracyFlag);
		}
		if (SynergyId.Length != 0)
		{
			output.WriteRawTag(130, 2);
			output.WriteString(SynergyId);
		}
		if (DeviceModel.Length != 0)
		{
			output.WriteRawTag(138, 2);
			output.WriteString(DeviceModel);
		}
		if (DeviceId.Length != 0)
		{
			output.WriteRawTag(146, 2);
			output.WriteString(DeviceId);
		}
		if (Application.Length != 0)
		{
			output.WriteRawTag(170, 2);
			output.WriteString(Application);
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
		if (ServiceName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServiceName);
		}
		if (MethodName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(MethodName);
		}
		if (Payload.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Payload);
		}
		if (AuthId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AuthId);
		}
		if (AuthToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AuthToken);
		}
		if (ClientVersion != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ClientVersion);
		}
		if (ClientStartupTimestamp != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ClientStartupTimestamp);
		}
		if (Platform.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Platform);
		}
		if (Region.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Region);
		}
		if (ClientExternalVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientExternalVersion);
		}
		if (ClientInternalVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientInternalVersion);
		}
		if (RequestId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(RequestId);
		}
		if (AcceptEncoding != 0)
		{
			num += 2 + CodedOutputStream.ComputeEnumSize((int)AcceptEncoding);
		}
		num += flag_.CalculateSize(_repeated_flag_codec);
		num += telemetryEvent_.CalculateSize(_repeated_telemetryEvent_codec);
		if (CurrentClientTime != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(CurrentClientTime);
		}
		if (NimbleSessionId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(NimbleSessionId);
		}
		if (Timezone.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Timezone);
		}
		if (FirmwareVersion.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(FirmwareVersion);
		}
		if (Carrier.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Carrier);
		}
		if (NetworkAccess.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(NetworkAccess);
		}
		if (HardwareId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(HardwareId);
		}
		if (AdvertiserId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(AdvertiserId);
		}
		if (VendorId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(VendorId);
		}
		if (AndroidId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(AndroidId);
		}
		if (JailbrokenFlag != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(JailbrokenFlag);
		}
		if (PiracyFlag != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(PiracyFlag);
		}
		if (SynergyId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(SynergyId);
		}
		if (DeviceModel.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(DeviceModel);
		}
		if (DeviceId.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(DeviceId);
		}
		if (Application.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Application);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RequestEnvelope other)
	{
		if (other != null)
		{
			if (other.CorrelationId != 0)
			{
				CorrelationId = other.CorrelationId;
			}
			if (other.ServiceName.Length != 0)
			{
				ServiceName = other.ServiceName;
			}
			if (other.MethodName.Length != 0)
			{
				MethodName = other.MethodName;
			}
			if (other.Payload.Length != 0)
			{
				Payload = other.Payload;
			}
			if (other.AuthId.Length != 0)
			{
				AuthId = other.AuthId;
			}
			if (other.AuthToken.Length != 0)
			{
				AuthToken = other.AuthToken;
			}
			if (other.ClientVersion != 0)
			{
				ClientVersion = other.ClientVersion;
			}
			if (other.ClientStartupTimestamp != 0L)
			{
				ClientStartupTimestamp = other.ClientStartupTimestamp;
			}
			if (other.Platform.Length != 0)
			{
				Platform = other.Platform;
			}
			if (other.Region.Length != 0)
			{
				Region = other.Region;
			}
			if (other.ClientExternalVersion.Length != 0)
			{
				ClientExternalVersion = other.ClientExternalVersion;
			}
			if (other.ClientInternalVersion.Length != 0)
			{
				ClientInternalVersion = other.ClientInternalVersion;
			}
			if (other.RequestId.Length != 0)
			{
				RequestId = other.RequestId;
			}
			if (other.AcceptEncoding != 0)
			{
				AcceptEncoding = other.AcceptEncoding;
			}
			flag_.Add(other.flag_);
			telemetryEvent_.Add(other.telemetryEvent_);
			if (other.CurrentClientTime != 0L)
			{
				CurrentClientTime = other.CurrentClientTime;
			}
			if (other.NimbleSessionId.Length != 0)
			{
				NimbleSessionId = other.NimbleSessionId;
			}
			if (other.Timezone.Length != 0)
			{
				Timezone = other.Timezone;
			}
			if (other.FirmwareVersion.Length != 0)
			{
				FirmwareVersion = other.FirmwareVersion;
			}
			if (other.Carrier.Length != 0)
			{
				Carrier = other.Carrier;
			}
			if (other.NetworkAccess.Length != 0)
			{
				NetworkAccess = other.NetworkAccess;
			}
			if (other.HardwareId.Length != 0)
			{
				HardwareId = other.HardwareId;
			}
			if (other.AdvertiserId.Length != 0)
			{
				AdvertiserId = other.AdvertiserId;
			}
			if (other.VendorId.Length != 0)
			{
				VendorId = other.VendorId;
			}
			if (other.AndroidId.Length != 0)
			{
				AndroidId = other.AndroidId;
			}
			if (other.JailbrokenFlag != 0)
			{
				JailbrokenFlag = other.JailbrokenFlag;
			}
			if (other.PiracyFlag != 0)
			{
				PiracyFlag = other.PiracyFlag;
			}
			if (other.SynergyId.Length != 0)
			{
				SynergyId = other.SynergyId;
			}
			if (other.DeviceModel.Length != 0)
			{
				DeviceModel = other.DeviceModel;
			}
			if (other.DeviceId.Length != 0)
			{
				DeviceId = other.DeviceId;
			}
			if (other.Application.Length != 0)
			{
				Application = other.Application;
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
			case 34u:
				ServiceName = input.ReadString();
				break;
			case 42u:
				MethodName = input.ReadString();
				break;
			case 50u:
				Payload = input.ReadBytes();
				break;
			case 58u:
				AuthId = input.ReadString();
				break;
			case 66u:
				AuthToken = input.ReadString();
				break;
			case 72u:
				ClientVersion = input.ReadInt32();
				break;
			case 88u:
				ClientStartupTimestamp = input.ReadInt64();
				break;
			case 98u:
				Platform = input.ReadString();
				break;
			case 106u:
				Region = input.ReadString();
				break;
			case 114u:
				ClientExternalVersion = input.ReadString();
				break;
			case 122u:
				ClientInternalVersion = input.ReadString();
				break;
			case 130u:
				RequestId = input.ReadString();
				break;
			case 136u:
				acceptEncoding_ = (AcceptEncoding)input.ReadEnum();
				break;
			case 146u:
				flag_.AddEntriesFrom(input, _repeated_flag_codec);
				break;
			case 154u:
				telemetryEvent_.AddEntriesFrom(input, _repeated_telemetryEvent_codec);
				break;
			case 160u:
				CurrentClientTime = input.ReadInt64();
				break;
			case 170u:
				NimbleSessionId = input.ReadString();
				break;
			case 178u:
				Timezone = input.ReadString();
				break;
			case 186u:
				FirmwareVersion = input.ReadString();
				break;
			case 194u:
				Carrier = input.ReadString();
				break;
			case 202u:
				NetworkAccess = input.ReadString();
				break;
			case 210u:
				HardwareId = input.ReadString();
				break;
			case 218u:
				AdvertiserId = input.ReadString();
				break;
			case 226u:
				VendorId = input.ReadString();
				break;
			case 234u:
				AndroidId = input.ReadString();
				break;
			case 240u:
				JailbrokenFlag = input.ReadInt32();
				break;
			case 248u:
				PiracyFlag = input.ReadInt32();
				break;
			case 258u:
				SynergyId = input.ReadString();
				break;
			case 266u:
				DeviceModel = input.ReadString();
				break;
			case 274u:
				DeviceId = input.ReadString();
				break;
			case 298u:
				Application = input.ReadString();
				break;
			}
		}
	}
}
