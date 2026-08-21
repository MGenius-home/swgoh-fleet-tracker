using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ipd.Core.Interfaces;
using Ipd.Core.Models;

namespace SimpleTracker.Services;

public class EnvCsvAllyCodesProvider : IPlayerSettingsProvider
{
	public IList<PlayerSettings> GetAllyCodes()
	{
		return (from ac in (Environment.GetEnvironmentVariable("ALLY_CODES") ?? "").Trim().Split(',')
			select ac.Trim().Replace("-", "") into ac
			select new PlayerSettings
			{
				AllyCode = ac
			}).Distinct().ToList();
	}

	public Task<IList<PlayerSettings>> GetPlayerSettingAsync()
	{
		return Task.FromResult((IList<PlayerSettings>)(from ac in (Environment.GetEnvironmentVariable("ALLY_CODES") ?? "").Trim().Split(',')
			select ac.Trim().Replace("-", "") into ac
			select new PlayerSettings
			{
				AllyCode = ac
			}).Distinct().ToList());
	}
}
