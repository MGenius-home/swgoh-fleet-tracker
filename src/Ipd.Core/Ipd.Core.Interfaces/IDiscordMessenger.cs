using System.Threading.Tasks;

namespace Ipd.Core.Interfaces;

public interface IDiscordMessenger
{
	string DiscordWebHook { get; }

	Task SendTextMessage(string textMessage);

	Task SendTextTaggedMessage(string userDiscordId, string textMessage);
}
