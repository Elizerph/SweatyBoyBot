using System;
using System.Collections.Generic;

namespace SweatyBoyBot
{
	public class PostTemplate
	{
		public TimeSpan PostFrequency { get; set; }
		public DateTime PostTime { get; set; }
		public string Uri { get; set; }
		public string Title { get; set; }
		public IReadOnlyCollection<RawContentParserTemplate> Parsers { get; set; } = Array.Empty<RawContentParserTemplate>();
	}
}
