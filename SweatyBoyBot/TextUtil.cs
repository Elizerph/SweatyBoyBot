using System.Collections.Generic;
using System.Linq;

namespace SweatyBoyBot
{
	public static class TextUtil
	{
		public static IEnumerable<IEnumerable<string>> SplitBy(this IEnumerable<string> content, int charLimit, int separatorLength)
		{
			var totalLength = 0;
			var list = new List<string>();
			foreach (var line in content.Select(e => e.Length > charLimit ? e.Substring(0, charLimit) : e))
			{
				totalLength += separatorLength;
				if (totalLength + line.Length > charLimit)
				{
					list.Add(line.Substring(0, charLimit - totalLength));
					yield return list;
					list = new List<string>();
					totalLength = 0;
				}
				else
				{
					list.Add(line);
					totalLength += line.Length;
				}
			}
			yield return list;
		}
	}
}
