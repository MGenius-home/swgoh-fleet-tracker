using System.Threading.Tasks;

namespace Ipd.Core.Interfaces;

public interface INewDiscordMessenger
{
	Task SendTextMessage(string webHookUrl, string textMessage);
}
