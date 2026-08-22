using NodaTime;

namespace Ipd.Core.Models.Discord;

public class DiscordMessage
{
	public string DiscrodHookUrl { get; set; }

	public string Message { get; set; }

	public Instant TimeStamp { get; set; }

	public DiscordEmbed Embed { get; set; }
}
