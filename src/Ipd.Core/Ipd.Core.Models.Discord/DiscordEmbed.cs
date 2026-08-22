using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ipd.Core.Models.Discord;

public class DiscordEmbed
{
	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("color")]
	public int Color { get; set; }

	[JsonProperty("timestamp")]
	public DateTime? Timestamp { get; set; }

	[JsonProperty("fields")]
	public IList<DiscordEmbedField> Fields { get; set; } = new List<DiscordEmbedField>();
}
