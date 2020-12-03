using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	public class Post
	{
		private readonly string _uri;
		private readonly string _title;
		private readonly IReadOnlyCollection<RawContentParser> _parsers;

		public ulong ChannelId { get; }
		public DateTime NextPostTime { get; set; }
		public TimeSpan Freuqency { get; }

		public Post(ulong channelId, TimeSpan postFrequency, DateTime postTime, string uri, string title, IReadOnlyCollection<RawContentParser> parsers)
		{
			ChannelId = channelId;
			NextPostTime = postTime;
			Freuqency = postFrequency;
			_uri = uri;
			_title = title;
			_parsers = parsers;
		}

		public async Task<string> GetContent(HttpClient client)
		{
			try
			{
				if (!Uri.TryCreate(_uri, UriKind.Absolute, out var uri))
					return $"Unable to get content from {_uri}";

				var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
				if (!response.IsSuccessStatusCode)
					return $"Unable to get content from {_uri}: {(int)response.StatusCode} - {response.StatusCode}";
				var rawContent = await response.Content.ReadAsStringAsync();
				return string.Join(Environment.NewLine, GetContent(rawContent));
			}
			catch (HttpRequestException e)
			{
				return $"Unable to get content from {_uri}: {e.Message}";
			}
		}

		public IEnumerable<string> GetContent(string rawContent)
		{
			if (!string.IsNullOrWhiteSpace(_title))
				yield return _title;
			foreach (var parser in _parsers)
				yield return parser.GetContent(rawContent);
		}
	}
}
