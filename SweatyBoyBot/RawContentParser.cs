using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SweatyBoyBot
{
	public class RawContentParser
	{
		private readonly string _regex;
		private readonly HashSet<string> _regexKeys;
		private readonly string _title;
		private readonly string _regexMatchPattern;
		private readonly int _matchLimit;
		private readonly bool _enumerateItems;

		public RawContentParser(string regex, HashSet<string> regexKeys, string title, string regexMatchPattern, int matchLimit, bool enumerateItems)
		{
			_regex = regex;
			_regexKeys = regexKeys;
			_title = title;
			_regexMatchPattern = regexMatchPattern;
			_matchLimit = matchLimit;
			_enumerateItems = enumerateItems;
		}

		public IEnumerable<string> Parse(string raw)
		{
			var matches = new Regex(_regex).Matches(raw).Cast<Match>();
			if (_matchLimit > 0)
				matches = matches.Take(_matchLimit);

			if (!string.IsNullOrWhiteSpace(_title))
				yield return _title;

			var index = 0;
			foreach (var group in matches.Select(e => e.Groups))
			{
				var resultItem = _regexMatchPattern;
				foreach (var key in _regexKeys)
					resultItem = resultItem.Replace($"<{key}>", group[key].Value);

				if (_enumerateItems)
					yield return $"{++index}. {resultItem}";
				else
					yield return resultItem;
			}
		}
	}
}
