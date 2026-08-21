using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;

namespace SimpleTracker.Services;

public class StaticArenaRankStorage : IArenaRankStorage
{
	private static readonly IDictionary<string, int> _storage = new ConcurrentDictionary<string, int>();

	public int? GetRank(string allyCode)
	{
		if (_storage.ContainsKey(allyCode))
		{
			return _storage[allyCode];
		}
		return null;
	}

	public IDictionary<string, int> GetRanks()
	{
		return new Dictionary<string, int>(_storage);
	}

	public Task<IDictionary<string, int>> GetRanksAsync()
	{
		throw new NotImplementedException();
	}

	public void SaveRank(string allyCode, int rank)
	{
		_storage[allyCode] = rank;
	}

	public Task SaveRanksAsync(IDictionary<string, int> ranks)
	{
		throw new NotImplementedException();
	}
}
