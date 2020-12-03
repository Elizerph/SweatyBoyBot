using System.Collections.Generic;

namespace SweatyBoyBot
{
	public class RawContentParserTemplate
	{
		public string Regex { get; set; }
		public HashSet<string> RegexKeys { get; set; }
		public string Title { get; set; }
		public string RegexMatchPattern { get; set; }
		public int MatchLimit { get; set; }
		public bool EnumerateItems { get; set; }

		public RawContentParser Get()
		{
			return new RawContentParser(Regex, RegexKeys, Title, RegexMatchPattern, MatchLimit, EnumerateItems);
		}
	}
}
