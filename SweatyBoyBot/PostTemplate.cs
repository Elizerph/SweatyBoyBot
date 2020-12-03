using System;
using System.Collections.Generic;
using System.Linq;

namespace SweatyBoyBot
{
	public class PostTemplate
	{
		public TimeSpan PostFrequency { get; set; }
		public DateTime PostTime { get; set; }
		public string Uri { get; set; }
		public string Title { get; set; }
		public IReadOnlyCollection<RawContentParserTemplate> Parsers { get; set; } = Array.Empty<RawContentParserTemplate>();
		public Post Get(ulong channelId)
		{
			return new Post(channelId, PostFrequency, PostTime, Uri, Title, Parsers.Select(p => p.Get()).ToList());
		}
	}
}
