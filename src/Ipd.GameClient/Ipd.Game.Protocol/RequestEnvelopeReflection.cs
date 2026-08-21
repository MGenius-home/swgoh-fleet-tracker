using System;
using Google.Protobuf.Reflection;

namespace Ipd.Game.Protocol;

public static class RequestEnvelopeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static RequestEnvelopeReflection()
	{
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVSZXF1ZXN0RW52ZWxvcGUucHJvdG8SEWlwZC5nYW1lLnByb3RvY29sIv8F" + "Cg9SZXF1ZXN0RW52ZWxvcGUSFgoOY29ycmVsYXRpb25faWQYASABKAUSFAoM" + "c2VydmljZV9uYW1lGAQgASgJEhMKC21ldGhvZF9uYW1lGAUgASgJEg8KB3Bh" + "eWxvYWQYBiABKAwSDwoHYXV0aF9pZBgHIAEoCRISCgphdXRoX3Rva2VuGAgg" + "ASgJEhYKDmNsaWVudF92ZXJzaW9uGAkgASgFEiAKGGNsaWVudF9zdGFydHVw" + "X3RpbWVzdGFtcBgLIAEoAxIQCghwbGF0Zm9ybRgMIAEoCRIOCgZyZWdpb24Y" + "DSABKAkSHwoXY2xpZW50X2V4dGVybmFsX3ZlcnNpb24YDiABKAkSHwoXY2xp" + "ZW50X2ludGVybmFsX3ZlcnNpb24YDyABKAkSEgoKcmVxdWVzdF9pZBgQIAEo" + "CRI6Cg9hY2NlcHRfZW5jb2RpbmcYESABKA4yIS5pcGQuZ2FtZS5wcm90b2Nv" + "bC5BY2NlcHRFbmNvZGluZxIMCgRmbGFnGBIgAygJEhcKD3RlbGVtZXRyeV9l" + "dmVudBgTIAMoCRIbChNjdXJyZW50X2NsaWVudF90aW1lGBQgASgDEhkKEW5p" + "bWJsZV9zZXNzaW9uX2lkGBUgASgJEhAKCHRpbWV6b25lGBYgASgJEhgKEGZp" + "cm13YXJlX3ZlcnNpb24YFyABKAkSDwoHY2FycmllchgYIAEoCRIWCg5uZXR3" + "b3JrX2FjY2VzcxgZIAEoCRITCgtoYXJkd2FyZV9pZBgaIAEoCRIVCg1hZHZl" + "cnRpc2VyX2lkGBsgASgJEhEKCXZlbmRvcl9pZBgcIAEoCRISCgphbmRyb2lk" + "X2lkGB0gASgJEhcKD2phaWxicm9rZW5fZmxhZxgeIAEoBRITCgtwaXJhY3lf" + "ZmxhZxgfIAEoBRISCgpzeW5lcmd5X2lkGCAgASgJEhQKDGRldmljZV9tb2Rl" + "bBghIAEoCRIRCglkZXZpY2VfaWQYIiABKAkSEwoLYXBwbGljYXRpb24YJSAB" + "KAkqQwoOQWNjZXB0RW5jb2RpbmcSGQoVREVGQVVMVEFDQ0VQVEVOQ09ESU5H" + "EAASFgoSR1pJUEFDQ0VQVEVOQ09ESU5HEAFiBnByb3RvMw=="), new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(AcceptEncoding) }, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RequestEnvelope), RequestEnvelope.Parser, new string[32]
			{
				"CorrelationId", "ServiceName", "MethodName", "Payload", "AuthId", "AuthToken", "ClientVersion", "ClientStartupTimestamp", "Platform", "Region",
				"ClientExternalVersion", "ClientInternalVersion", "RequestId", "AcceptEncoding", "Flag", "TelemetryEvent", "CurrentClientTime", "NimbleSessionId", "Timezone", "FirmwareVersion",
				"Carrier", "NetworkAccess", "HardwareId", "AdvertiserId", "VendorId", "AndroidId", "JailbrokenFlag", "PiracyFlag", "SynergyId", "DeviceModel",
				"DeviceId", "Application"
			}, null, null, null)
		}));
	}
}
