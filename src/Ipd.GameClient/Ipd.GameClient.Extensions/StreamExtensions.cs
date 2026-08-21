using System.IO;
using System.IO.Compression;

namespace Ipd.GameClient.Extensions;

public static class StreamExtensions
{
	public static byte[] ToByteArray(this Stream stream)
	{
		byte[] array = new byte[16384];
		using MemoryStream memoryStream = new MemoryStream();
		int count;
		while ((count = stream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, count);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Unzip(this byte[] gzip)
	{
		using MemoryStream stream = new MemoryStream(gzip);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		byte[] buffer = new byte[4096];
		using MemoryStream memoryStream = new MemoryStream();
		int num = 0;
		do
		{
			num = gZipStream.Read(buffer, 0, 4096);
			if (num > 0)
			{
				memoryStream.Write(buffer, 0, num);
			}
		}
		while (num > 0);
		return memoryStream.ToArray();
	}
}
