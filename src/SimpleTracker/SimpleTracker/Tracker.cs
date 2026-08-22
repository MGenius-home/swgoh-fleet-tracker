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

	private const int PAYOUT_SHIFT_EMBED_COLOR = 0xE67E22;

	private const int MAX_EMBED_FIELD_LENGTH = 1024;

	private ITagsProvider TagProvider;

	private readonly Channel<DiscordMessage> _channel;

	private readonly ISettingsService _settingService;

	private IDiscordMessenger Messenger { get; set; }

	private IPlayerSettingsProvider PlayerSettingsProvider { get; set; }

	private IPersistentStorageService Storage { get; set; }

	private IPlayerRankService PlayerRankService { get; set; }

	private IPayoutService PayoutService { get; set; }

	private IAttackTrackerService AttackTracker { get; set; }

	private ILog Logger { get; set; }

		private IStatsService StatService { get; set; }

	public Tracker(IDiscordMessenger messenger, IPlayerSettingsProvider playerSettingsProvider, IPersistentStorageService storage, IPlayerRankService playerRankService, ILog logger, ITagsProvider tagProvider,  IStatsService statService, Channel<DiscordMessage> channel, ISettingsService settingService, IPayoutService payoutService, IAttackTrackerService attackTracker)
	{
		Messenger = messenger;
		PlayerSettingsProvider = playerSettingsProvider;
		Storage = storage;
		PlayerRankService = playerRankService;
		Logger = logger;
		TagProvider = tagProvider;
		StatService = statService;
		_channel = channel;
		_settingService = settingService;
		PayoutService = payoutService;
		AttackTracker = attackTracker;
	}

	public void PostStats()
	{
		try
		{
			IList<PlayerSettings> result = PlayerSettingsProvider.GetPlayerSettingAsync().Result;
			List<string> allyCodes = result.Select((PlayerSettings ps) => ps.AllyCode.NormalizeAllyCode()).ToList();
			StatService.PostStats("Fleet", result.Count, allyCodes);
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
			string allyCode = setting.AllyCode.NormalizeAllyCode();
			PlayerArenaRank result = PlayerRankService.GetPlayerRank(setting.AllyCode, auth).Result;
			int num = result.FleetArenaRank;
			string utcPayoutTime = PayoutService.GetUtcPayoutTime(result.PayoutOffsetMinutes);
			TrackerState trackerState = Storage.Load();
			bool flag = !trackerState.Players.TryGetValue(allyCode, out PlayerState playerState);
			if (flag || playerState == null)
			{
				playerState = new PlayerState();
				trackerState.Players[allyCode] = playerState;
			}
			int currentRank = playerState.CurrentRank;
			string utcPayoutTime2 = playerState.UtcPayoutTime;
			if (num <= 0)
			{
				if (flag || currentRank <= 0)
				{
					return;
				}
				Logger.Log("Rank fetch for allyCode:[" + allyCode + "] returned no fleet rank. Keeping last known rank " + currentRank + ".");
				playerState.PlayerName = result.PlayerName;
				playerState.TimezoneOffsetMinutes = result.PayoutOffsetMinutes;
				Storage.Save(trackerState);
				return;
			}
			bool shiftConfirmed = !flag && RegisterPayoutObservation(playerState, utcPayoutTime);
			playerState.PlayerName = result.PlayerName;
			playerState.PreviousRank = currentRank;
			playerState.CurrentRank = num;
			if (shiftConfirmed || string.IsNullOrEmpty(utcPayoutTime2))
			{
				playerState.UtcPayoutTime = utcPayoutTime;
				playerState.PendingUtcPayoutTime = null;
			}
			playerState.TimezoneOffsetMinutes = result.PayoutOffsetMinutes;
			if (flag)
			{
				playerState.PreviousRank = num;
			}
			Duration poTime = PoUtils.GetPoTime(result.PayoutOffsetMinutes, null);
			MessageMap map = PopulateMessageMap(setting, result.PlayerName, currentRank, num, poTime);
			if (!flag && currentRank != num)
			{
				if (currentRank > num)
				{
					if (AttackTracker.ShouldCountAttack(result.PayoutOffsetMinutes))
					{
						playerState.WeeklyAttacks++;
						playerState.LastAttackTimestamp = DateTime.UtcNow;
					}
					SendClimbMessage(map, setting);
				}
				else
				{
					SendDropMessage(map, setting);
				}
			}
			if (_settingService.IsPayoutTrackingEnabled && shiftConfirmed && !string.IsNullOrEmpty(utcPayoutTime2))
			{
				SendPayoutShiftMessage(allyCode, result, utcPayoutTime2, utcPayoutTime, trackerState);
			}
			Storage.Save(trackerState);
		}
		catch (Exception ex)
		{
			Logger.Log("Error processing allyCode:[" + setting.AllyCode + "]:" + ex.Message);
		}
	}

	public static bool RegisterPayoutObservation(PlayerState playerState, string newUtcPayoutTime)
	{
		if (playerState.UtcPayoutTime == newUtcPayoutTime)
		{
			playerState.PendingUtcPayoutTime = null;
			return false;
		}
		if (!string.IsNullOrEmpty(playerState.PendingUtcPayoutTime) && playerState.PendingUtcPayoutTime == newUtcPayoutTime)
		{
			return true;
		}
		playerState.PendingUtcPayoutTime = newUtcPayoutTime;
		return false;
	}

	public static MessageMap PopulateMessageMap(PlayerSettings playerSettings, string playerName, int prevRank, int currentRank, Duration timeToPo, ISettingsService settingService)
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
		if (currentRank >= settingService.TagOnDropRankLimit && totalMinutes < (double)settingService.TagOnDropPayoutLimitMins)
		{
			messageMap.TagOnDrop = TagFor(playerSettings?.TagIdOnDrop, playerSettings?.DiscordId);
		}
		if (currentRank <= settingService.TagOnClimbRankLimit)
		{
			messageMap.TagOnClimb = TagFor(playerSettings?.TagIdOnClimb, playerSettings?.DiscordId);
		}
		return messageMap;
	}

	private static string TagFor(string perPlayerTagId, string discordId)
	{
		string tagId = (!string.IsNullOrWhiteSpace(perPlayerTagId) ? perPlayerTagId : discordId) ?? "";
		tagId = tagId.Trim();
		if (string.IsNullOrEmpty(tagId))
		{
			return "";
		}
		return "<@" + tagId + ">";
	}

	private MessageMap PopulateMessageMap(PlayerSettings playerSettings, string playerName, int prevRank, int currentRank, Duration timeToPo)
	{
		return PopulateMessageMap(playerSettings, playerName, prevRank, currentRank, timeToPo, _settingService);
	}

	private string GetPayoutWebHook()
	{
		string payoutWebHookUrl = _settingService.PayoutWebHookUrl;
		if (!string.IsNullOrWhiteSpace(payoutWebHookUrl))
		{
			return payoutWebHookUrl.Trim();
		}
		return Messenger.DiscordWebHook;
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

	private bool WriteDiscordEmbedMessage(DiscordEmbed embed, string webHookUrl)
	{
		DiscordMessage item = new DiscordMessage
		{
			DiscrodHookUrl = webHookUrl,
			Embed = embed
		};
		bool num = _channel.Writer.TryWrite(item);
		if (!num)
		{
			Console.WriteLine("Error: failed to enqueue discord message");
		}
		return num;
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

	private void SendPayoutShiftMessage(string allyCode, PlayerArenaRank rank, string previousUtcPayoutTime, string newUtcPayoutTime, TrackerState state)
	{
		PayoutShiftInfo payoutShiftInfo = PayoutService.BuildShiftInfo(allyCode, rank.PlayerName, previousUtcPayoutTime, newUtcPayoutTime);
		string value = string.Join("\n", PayoutService.GetSharedPayoutGroup(state, newUtcPayoutTime, allyCode).Select((string p) => "- " + p));
		if (string.IsNullOrEmpty(value))
		{
			value = "No other tracked players at this payout slot.";
		}
		DiscordEmbed discordEmbed = new DiscordEmbed
		{
			Title = "Payout Shift",
			Color = PAYOUT_SHIFT_EMBED_COLOR,
			Timestamp = DateTime.UtcNow
		};
		discordEmbed.Fields.Add(new DiscordEmbedField
		{
			Name = "Player",
			Value = FormatPlayer(payoutShiftInfo.PlayerName, allyCode),
			Inline = true
		});
		discordEmbed.Fields.Add(new DiscordEmbedField
		{
			Name = "Shift Delta",
			Value = FormatShiftDelta(payoutShiftInfo.ShiftDeltaHours),
			Inline = true
		});
		discordEmbed.Fields.Add(new DiscordEmbedField
		{
			Name = "New UTC Payout Time",
			Value = payoutShiftInfo.NewUtcPayoutTime + " UTC",
			Inline = true
		});
		discordEmbed.Fields.Add(new DiscordEmbedField
		{
			Name = "Shared Payout Group",
			Value = Truncate(value),
			Inline = false
		});
		if (_settingService.PostFullPayoutListOnChange)
		{
			string value2 = string.Join("\n", PayoutService.GetFullPayoutRoster(state).Select((PayoutRosterEntry e) => $"`{e.UtcPayoutTime}` {FormatPlayer(e.PlayerName, e.AllyCode)}"));
			if (!string.IsNullOrEmpty(value2))
			{
				discordEmbed.Fields.Add(new DiscordEmbedField
				{
					Name = "Full Payout Order",
					Value = Truncate(value2),
					Inline = false
				});
			}
		}
		WriteDiscordEmbedMessage(discordEmbed, GetPayoutWebHook());
	}

	private static string FormatPlayer(string playerName, string allyCode)
	{
		if (!string.IsNullOrWhiteSpace(playerName))
		{
			return $"{playerName} ({allyCode})";
		}
		return allyCode;
	}

	private static string FormatShiftDelta(double shiftDeltaHours)
	{
		return (shiftDeltaHours >= 0.0 ? "+" : "") + shiftDeltaHours.ToString("0.##") + "h";
	}

	private static string Truncate(string value)
	{
		if (value.Length <= MAX_EMBED_FIELD_LENGTH)
		{
			return value;
		}
		return value.Substring(0, MAX_EMBED_FIELD_LENGTH - 3) + "...";
	}
}
