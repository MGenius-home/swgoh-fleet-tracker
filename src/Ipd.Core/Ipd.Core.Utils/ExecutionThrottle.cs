using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Ipd.Core.Utils;

public class ExecutionThrottle
{
	public static void ThrottleSync(int timeLimitMs, Action action)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		action();
		stopwatch.Stop();
		long num = timeLimitMs - stopwatch.ElapsedMilliseconds;
		if (num > 0)
		{
			Thread.Sleep((int)num);
		}
	}

	public static T ThrottleSync<T>(int timeLimitMs, Func<T> action)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		T result = action();
		stopwatch.Stop();
		long num = timeLimitMs - stopwatch.ElapsedMilliseconds;
		if (num > 0)
		{
			Thread.Sleep((int)num);
		}
		return result;
	}

	public static async Task ThrottleAsync(int timeLimitMs, Func<Task> task)
	{
		Stopwatch watcher = new Stopwatch();
		watcher.Start();
		await Task.Run(task);
		watcher.Stop();
		long num = timeLimitMs - watcher.ElapsedMilliseconds;
		if (num > 0)
		{
			await Task.Delay((int)num);
		}
	}
}
