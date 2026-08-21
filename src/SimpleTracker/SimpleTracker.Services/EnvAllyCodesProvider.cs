using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace SimpleTracker.Services;

public class EnvAllyCodesProvider : IAllyCodesProvider
{
	public IList<PlayerSettings> GetAllyCodes()
	{
		List<string> list = new List<string>();
		foreach (object key in Environment.GetEnvironmentVariables().Keys)
		{
			string text = key.ToString();
			if (text.StartsWith("AC_"))
			{
				list.Add(text.Replace("AC_", "").Replace("-", "").Trim());
			}
		}
		return (from ac in list.Distinct().Take(75)
			select new PlayerSettings
			{
				AllyCode = ac
			}).ToList();
	}

	public Task<IList<PlayerSettings>> GetAllyCodesAsync()
	{
		throw new NotImplementedException();
	}
}
