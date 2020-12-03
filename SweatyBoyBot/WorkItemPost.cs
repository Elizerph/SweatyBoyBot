using System;
using System.Collections.Generic;

namespace SweatyBoyBot
{
	public class WorkItemPost
	{
		public string Url { get; set; }
		public string Title { get; set; }
		public IReadOnlyCollection<RawContentParserTemplate> Parsers { get; set; } = Array.Empty<RawContentParserTemplate>();

		public IEnumerable<string> Parse(string rawContent)
		{
			if (!string.IsNullOrWhiteSpace(Title))
				yield return Title;
			foreach (var parserTemplate in Parsers)
				foreach (var line in parserTemplate.Get().Parse(rawContent))
					yield return line;
		}
	}
}
