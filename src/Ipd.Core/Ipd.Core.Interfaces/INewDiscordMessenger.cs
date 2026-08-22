using System.Threading.Tasks;
using Ipd.Core.Models.Discord;

namespace Ipd.Core.Interfaces;

public interface INewDiscordMessenger
{
	Task<bool> SendTextMessage(string webHookUrl, string textMessage);

	Task<bool> SendEmbedMessage(string webHookUrl, DiscordEmbed embed);
}
