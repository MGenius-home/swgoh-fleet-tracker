using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Ipd.Core.Extensions;
using Ipd.Core.Interfaces;
using Ipd.Core.Messages;
using Ipd.Core.Models;
using Ipd.Core.Models.Discord;
using Ipd.Core.Utils;
using MoreLinq;
using NodaTime;

namespace SimpleTracker;

public class Tracker
{
	private const int THROTTLE_DELAY = 200;

	private ITagsProvider TagProvider;

	private readonly Channel<DiscordMessage> _channel;

	private readonly ISettingsService _settingService;

	private IDiscordMessenger Messenger { get; set; }

	private IPlayerSettingsProvider PlayerSettingsProvider { get; set; }

	private IArenaRankStorage ArenaRankStorage { get; set; }

	private IPlayerRankService PlayerRankService { get; set; }

	private ILog Logger { get; set; }

	private ArenaType ArenaType { get; set; }

	private IStatsService StatService { get; set; }

	public Tracker(IDiscordMessenger messenger, IPlayerSettingsProvider playerSettingsProvider, IArenaRankStorage arenaRankStorage, IPlayerRankService playerRankService, ILog logger, ITagsProvider tagProvider, ArenaType arenaType, IStatsService statService, Channel<DiscordMessage> channel, ISettingsService settingService)
	{
		Messenger = messenger;
		PlayerSettingsProvider = playerSettingsProvider;
		ArenaRankStorage = arenaRankStorage;
		PlayerRankService = playerRankService;
		Logger = logger;
		ArenaType = arenaType;
		TagProvider = tagProvider;
		StatService = statService;
		_channel = channel;
		_settingService = settingService;
	}

	public void PostStats()
	{
		try
		{
			IList<PlayerSettings> result = PlayerSettingsProvider.GetPlayerSettingAsync().Result;
			List<string> allyCodes = result.Select((PlayerSettings ps) => ps.AllyCode.NormalizeAllyCode()).ToList();
			StatService.PostStats(ArenaType.ToString(), result.Count, allyCodes);
		}
		catch (Exception)
		{
		}
	}

	public void Track()
	{
		PlayerSettingsProvider.GetPlayerSettingAsync().Result.ForEach(delegate(PlayerSettings settings)
		{
			if (!settings.Skip)
			{
				ExecutionThrottle.ThrottleSync(200, delegate
				{
					TrackOneAllyCode(settings, new AuthResponse());
				});
			}
		});
	}

	public void TrackOneAllyCode(PlayerSettings setting, AuthResponse auth)
	{
		try
		{
			PlayerArenaRank result = PlayerRankService.GetPlayerRank(setting.AllyCode, auth).Result;
			int num = ((ArenaType == ArenaType.Fleet) ? result.FleetArenaRank : result.SquadArenaRank);
			int? rank = ArenaRankStorage.GetRank(setting.AllyCode);
			ArenaRankStorage.SaveRank(setting.AllyCode, num);
			Duration poTime = PoUtils.GetPoTime(result.PayoutOffsetMinutes, ArenaType, null);
			MessageMap map = PopulateMessageMap(setting, result.PlayerName, rank.GetValueOrDefault(), num, poTime);
			bool isStatusMessageDisabled = _settingService.IsStatusMessageDisabled;
			if (!rank.HasValue)
			{
				if (!isStatusMessageDisabled)
				{
					SendStatusMessage(map);
				}
			}
			else if (rank != num)
			{
				if (rank > num)
				{
					SendClimbMessage(map, setting);
				}
				else
				{
					SendDropMessage(map, setting);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("Error processing allyCode:[" + setting.AllyCode + "]:" + ex.Message);
		}
	}

	private MessageMap PopulateMessageMap(PlayerSettings playerSettings, string playerName, int prevRank, int currentRank, Duration timeToPo)
	{
		MessageMap messageMap = new MessageMap();
		messageMap.PlayerName = playerName;
		messageMap.Name = (playerSettings.Name ?? "").Trim();
		messageMap.CurrentRank = currentRank.ToString();
		messageMap.PreviousRank = prevRank.ToString();
		messageMap.TimeToPo = timeToPo.ToPayoutString();
		messageMap.UserIcon = (playerSettings.UserIcon ?? "").Trim();
		messageMap.AllyCode = (playerSettings.AllyCode ?? "").Replace("-", "").Trim();
		double totalMinutes = timeToPo.TotalMinutes;
		if (currentRank >= _settingService.TagOnDropRankLimit && totalMinutes < (double)_settingService.TagOnDropPayoutLimitMins)
		{
			messageMap.TagOnDrop = (string.IsNullOrEmpty((playerSettings?.TagIdOnDrop?.Trim() ?? "").Trim()) ? "" : ("<@" + playerSettings.TagIdOnDrop.Trim() + ">"));
		}
		if (currentRank <= _settingService.TagOnClimbRankLimit)
		{
			messageMap.TagOnClimb = (string.IsNullOrEmpty((playerSettings?.TagIdOnClimb?.Trim() ?? "").Trim()) ? "" : ("<@" + playerSettings.TagIdOnClimb.Trim() + ">"));
		}
		return messageMap;
	}

	private bool WriteDiscordMessage(string textMessage)
	{
		DiscordMessage item = new DiscordMessage
		{
			DiscrodHookUrl = Messenger.DiscordWebHook,
			Message = textMessage
		};
		bool num = _channel.Writer.TryWrite(item);
		if (!num)
		{
			Console.WriteLine("Error: failed to enqueue discord message");
		}
		return num;
	}

	private void SendStatusMessage(MessageMap map)
	{
		string textMessage = MessageGenerator.GenerateStatusMessage(map, _settingService.MessageFormatOnStatus);
		WriteDiscordMessage(textMessage);
	}

	private void SendClimbMessage(MessageMap map, PlayerSettings setting)
	{
		string textMessage = MessageGenerator.GenerateMessageOnClimb(map, _settingService.MessageFormatOnClimb);
		WriteDiscordMessage(textMessage);
	}

	private void SendDropMessage(MessageMap map, PlayerSettings setting)
	{
		string textMessage = MessageGenerator.GenerateMessageOnDrop(map, _settingService.MessageFormatOnDrop);
		WriteDiscordMessage(textMessage);
	}
}
