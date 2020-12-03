using Discord;
using Discord.WebSocket;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	class Program
	{
		private Dictionary<ulong, IReadOnlyCollection<PostTemplate>> _postTemplates;
		private IReadOnlyCollection<Post> _posts;

		private DiscordSocketClient _client;
		private HttpClient _httpClient;

		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			var token = Environment.GetEnvironmentVariable("SweatyBoyBotToken");

			if (string.IsNullOrEmpty(token))
			{
				Console.WriteLine($"Environment variable SweatyBoyBotToken not found");
				return;
			}

			_client = new DiscordSocketClient();

			_client.Log += Log;
			_client.MessageReceived += Client_MessageReceived;

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			_httpClient = new HttpClient();
			_postTemplates = new Dictionary<ulong, IReadOnlyCollection<PostTemplate>>();
			_posts = new List<Post>();

			await PostRoutine();
		}

		private async Task PostRoutine()
		{
			var routineInterval = TimeSpan.FromMinutes(1);
			while (true)
			{
				var now = DateTime.Now;

				Console.WriteLine($"{now}: Post start");
				var sw = new Stopwatch();
				sw.Start();

				var triggeredPosts = _posts.Where(e => e.NextPostTime < now).ToList();
				foreach (var triggeredPost in triggeredPosts)
					while (triggeredPost.NextPostTime < now)
						triggeredPost.NextPostTime += triggeredPost.Freuqency;

				var triggeredPostsContent = await Task.WhenAll(triggeredPosts.Select(GetPostContent));

				var channelsStatus = await Task.WhenAll(triggeredPostsContent.GroupBy(e => e.Item1).Select(e => SendContentToChannel(e.Key, e.Select(t => t.Item2))));

				var channelsToDelete = channelsStatus.Where(e => !e.Item2).Select(e => e.Item1).ToList();
				if (channelsToDelete.Count != 0)
				{
					foreach (var channelId in channelsToDelete)
						_postTemplates.Remove(channelId);
					UpdatePosts();
				}

				sw.Stop();
				Console.WriteLine($"{DateTime.Now}: Post end - Elapsed time: {sw.Elapsed}");

				await Task.Delay(routineInterval);
			}
		}

		private async Task<Tuple<ulong, string>> GetPostContent(Post post)
		{
			var content = await post.GetContent(_httpClient);
			return Tuple.Create(post.ChannelId, content);
		}

		private async Task<Tuple<ulong, bool>> SendContentToChannel(ulong channelId, IEnumerable<string> contents)
		{
			var channel = _client.GetChannel(channelId) as SocketTextChannel;
			if (channel == null)
				return Tuple.Create(channelId, false);
			else
			{
				var resultContent = string.Join($"{Environment.NewLine}--------{Environment.NewLine}", contents);
				await channel.SendMessageAsync(resultContent);
				return Tuple.Create(channelId, true);
			}
		}

		private void UpdatePosts()
		{
			_posts = _postTemplates.SelectMany(p => p.Value.Select(e => e.Get(p.Key))).ToList();
		}

		private async Task Client_MessageReceived(SocketMessage message)
		{
			var guildUser = message.Author as IGuildUser;
			if (guildUser == null || !guildUser.GuildPermissions.Administrator)
				return;

			if (!message.MentionedUsers.Any(e => e.Id == _client.CurrentUser.Id))
				return;

			var allMentions = message.MentionedRoles.Select(e => e.Mention)
				.Concat(message.MentionedUsers.Select(e => e.Mention)).ToArray();

			var filteredMessageContent = message.Content;
			foreach (var mention in allMentions)
				filteredMessageContent = filteredMessageContent.Replace(mention, string.Empty);

			var args = filteredMessageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (args.Length >= 1)
			{
				var channel = message.Channel;
				var command = args[0];
				switch (command)
				{
					case "postnow":
						var channelId = message.Channel.Id;
						var channelPosts = _posts.Where(e => e.ChannelId == channelId).ToList();
						if (channelPosts.Any())
						{
							var postContent = await Task.WhenAll(channelPosts.Select(e => e.GetContent(_httpClient)));
							await SendContentToChannel(channelId, postContent);
						}
						else
							await channel.SendMessageAsync("Nothing to post");
						break;
					case "getposts":
						if (_postTemplates.TryGetValue(message.Channel.Id, out var postSettings))
						{
							var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
							await File.WriteAllTextAsync(tempFile, JsonConvert.SerializeObject(postSettings));
							await channel.SendFileAsync(tempFile);
							File.Delete(tempFile);
						}
						else
							await channel.SendMessageAsync("There are no post settings for this channel");
						break;
					case "clearposts":
						_postTemplates.Remove(message.Channel.Id);
						UpdatePosts();
						await channel.SendMessageAsync("Post settings cleared for this channel");
						break;
					case "setposts":
						var attachment = message.Attachments.FirstOrDefault();
						if (attachment == null)
							await channel.SendMessageAsync("No file attached");
						else
						{
							try
							{
								var serializedPosts = await _httpClient.GetStringAsync(attachment.Url);
								var newPosts = JsonConvert.DeserializeObject<IReadOnlyCollection<PostTemplate>>(serializedPosts);
								_postTemplates[message.Channel.Id] = newPosts;
								UpdatePosts();
								await channel.SendMessageAsync("Ok, I will post this");
							}
							catch (JsonException e)
							{
								await channel.SendMessageAsync($"Well that is something I can't read: {e.Message}");
							}
						}
						break;
					default:
						await channel.SendMessageAsync("What?");
						break;
				}
			}
		}


		private Task Log(LogMessage message)
		{
			Console.WriteLine(message.Message);
			if (message.Exception != null)
			{
				Console.WriteLine(message.Exception.Message);
				Console.WriteLine(message.Exception.StackTrace);
			}
			return Task.CompletedTask;
		}
	}
}
