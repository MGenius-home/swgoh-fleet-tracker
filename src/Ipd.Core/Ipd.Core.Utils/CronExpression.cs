using System;
using System.Collections.Generic;
using System.Linq;

namespace Ipd.Core.Utils;

public class CronExpression
{
	private const int FieldCount = 5;

	private static readonly string[] DayNames = new string[14]
	{
		"SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY",
		"SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
	};

	private readonly HashSet<int>[] _fields = new HashSet<int>[5];

	private readonly bool _dayOfMonthRestricted;

	private readonly bool _dayOfWeekRestricted;

	public CronExpression(string expression)
	{
		string[] array = (expression ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 5)
		{
			throw new FormatException("Cron expression must contain 5 fields: minute hour day-of-month month day-of-week");
		}
		int[][] allowedRanges = new int[5][]
		{
			Range(0, 59),
			Range(0, 23),
			Range(1, 31),
			Range(1, 12),
			Range(0, 7)
		};
		for (int i = 0; i < 5; i++)
		{
			_fields[i] = ParseField(array[i], allowedRanges[i]);
		}
		if (_fields[4].Remove(7))
		{
			_fields[4].Add(0);
		}
		_dayOfMonthRestricted = !(array[2] == "*");
		_dayOfWeekRestricted = !(array[4] == "*");
	}

	public static CronExpression ParseSchedule(string expression)
	{
		string text = (expression ?? "").Trim();
		if (string.IsNullOrEmpty(text))
		{
			text = "0 0 * * 0";
		}
		string[] array = text.ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			switch (array[0])
			{
			case "WEEKLY":
				return new CronExpression("0 0 * * 0");
			case "DAILY":
				return new CronExpression("0 0 * * *");
			case "HOURLY":
				return new CronExpression("0 * * * *");
			}
		}
		if (array.Length == 2)
		{
			int? num = ParseDayOfWeek(array[0]);
			TimeSpan timeSpan = ParseTimeOfDay(array[1]);
			if (num.HasValue)
			{
				return new CronExpression($"{timeSpan.Minutes} {timeSpan.Hours} * * {num.Value}");
			}
			if (array[0] == "DAILY")
			{
				return new CronExpression($"{timeSpan.Minutes} {timeSpan.Hours} * * *");
			}
			throw new FormatException("Unknown schedule day '" + array[0] + "'. Use a day name (e.g. SUNDAY or SUN), DAILY, HOURLY, WEEKLY, or a 5-field cron expression.");
		}
		return new CronExpression(text);
	}

	private static int? ParseDayOfWeek(string value)
	{
		int num = Array.IndexOf(DayNames, value);
		if (num < 0)
		{
			return null;
		}
		return num % 7;
	}

	private static TimeSpan ParseTimeOfDay(string value)
	{
		string[] array = value.Split(':');
		if (array.Length != 2 || !int.TryParse(array[0], out var result) || !int.TryParse(array[1], out var result2) || result < 0 || result > 23 || result2 < 0 || result2 > 59)
		{
			throw new FormatException("Invalid schedule time '" + value + "'. Use HH:mm (e.g. 18:30).");
		}
		return new TimeSpan(result, result2, 0);
	}

	public bool IsMatch(DateTime utcTime)
	{
		return _fields[0].Contains(utcTime.Minute) && _fields[1].Contains(utcTime.Hour) && _fields[3].Contains(utcTime.Month) && DayMatches(utcTime);
	}

	private bool DayMatches(DateTime utcTime)
	{
		bool flag = _fields[2].Contains(utcTime.Day);
		bool flag2 = _fields[4].Contains((int)utcTime.DayOfWeek);
		if (_dayOfMonthRestricted && _dayOfWeekRestricted)
		{
			return flag || flag2;
		}
		return flag && flag2;
	}

	private static HashSet<int> ParseField(string field, int[] allowed)
	{
		HashSet<int> hashSet = new HashSet<int>();
		string[] array = field.Split(',', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string text2 = "1";
			string text3 = text;
			int num = text.IndexOf('/');
			if (num >= 0)
			{
				text3 = text.Substring(0, num);
				text2 = text.Substring(num + 1);
			}
			int num2;
			int num3;
			if (text3 == "*")
			{
				num2 = allowed[0];
				num3 = allowed[^1];
			}
			else if (text3.Contains('-'))
			{
				string[] array2 = text3.Split('-', StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length != 2)
				{
					throw new FormatException("Invalid cron range: " + text);
				}
				num2 = ParseValue(array2[0], allowed);
				num3 = ParseValue(array2[1], allowed);
			}
			else
			{
				num2 = (num3 = ParseValue(text3, allowed));
			}
			if (!int.TryParse(text2, out var result) || result <= 0)
			{
				throw new FormatException("Invalid cron step: " + text);
			}
			if (num2 > num3)
			{
				throw new FormatException("Invalid cron range: " + text);
			}
			for (int j = num2; j <= num3; j += result)
			{
				if (allowed.Contains(j))
				{
					hashSet.Add(j);
				}
			}
		}
		if (hashSet.Count == 0)
		{
			throw new FormatException("Cron field has no valid values: " + field);
		}
		return hashSet;
	}

	private static int ParseValue(string value, int[] allowed)
	{
		if (!int.TryParse(value.Trim(), out var result))
		{
			throw new FormatException("Invalid cron value: " + value);
		}
		return result;
	}

	private static int[] Range(int from, int to)
	{
		return Enumerable.Range(from, to - from + 1).ToArray();
	}
}
