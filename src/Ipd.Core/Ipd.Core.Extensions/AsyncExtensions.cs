using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ipd.Core.Extensions;

public static class AsyncExtensions
{
	public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		List<T> list = new List<T>();
		await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			list.Add(item);
		}
		return list;
	}

	public static async Task<List<T>> ToListAsync<T>(this ChannelReader<T> channelReader, CancellationToken cancellationToken = default(CancellationToken))
	{
		List<T> list = new List<T>();
		if (await channelReader.WaitToReadAsync(cancellationToken))
		{
			int count = channelReader.Count;
			T item;
			while (channelReader.TryRead(out item) && list.Count < count)
			{
				list.Add(item);
			}
			return list;
		}
		return list;
	}
}
