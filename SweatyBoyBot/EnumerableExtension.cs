using System.Collections.Generic;

namespace SweatyBoyBot
{
	public static class EnumerableExtension
	{
		public static IEnumerable<T> InsertSeparator<T>(this IEnumerable<T> source, T separator)
		{
			using var sourceEnumerator = source.GetEnumerator();
			if (!sourceEnumerator.MoveNext())
				yield break;

			yield return sourceEnumerator.Current;
			while (sourceEnumerator.MoveNext())
			{
				yield return separator;
				yield return sourceEnumerator.Current;
			}
		}
	}
}
