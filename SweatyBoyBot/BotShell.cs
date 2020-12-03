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
	public class BotShell
	{
		private const int DiscordMessageLimit = 2000;

		private readonly DiscordSocketClient _discordClient;
		private readonly string _token;
		private readonly HttpClient _httpClient;
		private readonly IRepository _repository;

		public BotShell(DiscordSocketClient discordClient, string token, HttpClient httpClient, IRepository repository)
		{
			_discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
			_token = token ?? throw new ArgumentNullException(nameof(token));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));

			_discordClient.MessageReceived += DiscordClient_MessageReceived;
		}

		public async Task Start()
		{
			await _discordClient.LoginAsync(TokenType.Bot, _token);
			await _discordClient.StartAsync();
		}

		public async Task Routine()
		{
			var routineInterval = TimeSpan.FromMinutes(1);
			while (true)
			{
				var now = DateTime.UtcNow;

				Console.WriteLine($"{now}: Post start");
				var sw = new Stopwatch();
				sw.Start();

				var triggeredWorkItems = _repository.GetWorkItems().Where(e => e.NextRun < now).ToList();

				foreach (var triggeredPost in triggeredWorkItems)
				{
					var frequency = TimeSpan.FromMinutes(triggeredPost.Frequency);
					while (triggeredPost.NextRun < now)
						triggeredPost.NextRun += frequency;
				}

				await _repository.SaveOrUpdateWorkItems(triggeredWorkItems);

				await ProcessWorkItems(triggeredWorkItems);

				sw.Stop();
				Console.WriteLine($"{DateTime.UtcNow}: Post end - Elapsed time: {sw.Elapsed}");

				await Task.Delay(routineInterval);
			}
		}

		public static async Task<string> GetRawContent(HttpClient client, string url)
		{
			try
			{
				if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
					return $"Unable to get content from {url}";

				var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

				if (!response.IsSuccessStatusCode)
					return $"Unable to get content from {url}: {(int)response.StatusCode} - {response.StatusCode}";

				return await response.Content.ReadAsStringAsync();
			}
			catch (HttpRequestException e)
			{
				return $"Unable to get content from {url}: {e.Message}";
			}
		}

		private async Task ProcessWorkItems(IEnumerable<WorkItem> triggeredWorkItems)
		{
			var channelPosts = triggeredWorkItems.ToLookup(e => ulong.Parse(e.ChannelId), e => e.GetPost());

			var rawContent = new Dictionary<string, string>();
			foreach (var url in channelPosts.SelectMany(e => e).Select(e => e.Url))
				rawContent[url] = await GetRawContent(_httpClient, url);

			var channelsToDelete = new List<ulong>();
			foreach (var group in channelPosts)
			{
				var channel = _discordClient.GetChannel(group.Key) as SocketTextChannel;
				if (channel == null)
					channelsToDelete.Add(group.Key);
				else
				{
					var channelContentLines = group.Select(e => e.Parse(rawContent[e.Url]))
						.InsertSeparator(new[] { "--------" })
						.SelectMany(e => e);
					var channelMessages = channelContentLines.SplitBy(DiscordMessageLimit, Environment.NewLine.Length);
					foreach (var messageLines in channelMessages)
					{
						var messageText = string.Join(Environment.NewLine, messageLines);
						await channel.SendMessageAsync(messageText);
					}
				}
			}
			await _repository.RemoveWorkItemsByChannel(channelsToDelete);
		}

		private async Task DiscordClient_MessageReceived(SocketMessage message)
		{
			if (!message.MentionedUsers.Any(e => e.Id == _discordClient.CurrentUser.Id))
				return;

			var guildUser = message.Author as IGuildUser;
			if (guildUser == null)
				return;

			var guildId = guildUser.GuildId;
			var managerRoles = _repository.GetManagerRoles(guildId);
			if (!guildUser.GuildPermissions.Administrator && !guildUser.RoleIds.Intersect(managerRoles).Any())
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
						var workItems = _repository.GetWorkItems().Where(e => e.ChannelId == channel.Id.ToString()).ToList();
						if (workItems.Any())
							await ProcessWorkItems(workItems);
						else
							await channel.SendMessageAsync("Nothing to post");
						break;
					case "getposts":
						var channelWorkItems = _repository.GetWorkItems().Where(e => e.ChannelId == channel.Id.ToString()).ToList();
						var postSettings = channelWorkItems.Select(e =>
						{
							var post = e.GetPost();
							return new PostTemplate
							{
								Uri = post.Url,
								Title = post.Title,
								PostTime = e.NextRun,
								PostFrequency = TimeSpan.FromMinutes(e.Frequency),
								Parsers = post.Parsers
							};
						}).ToList();
						if (postSettings.Any())
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
						await _repository.RemoveWorkItemsByChannel(new[] { channel.Id });
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
								await _repository.RemoveWorkItemsByChannel(new[] { channel.Id });
								await _repository.SaveOrUpdateWorkItems(newPosts.Select(e => new WorkItem
								{
									ChannelId = channel.Id.ToString(),
									Frequency = (int)e.PostFrequency.TotalMinutes,
									NextRun = e.PostTime,
									Content = JsonConvert.SerializeObject(new WorkItemPost { Url = e.Uri, Title = e.Title, Parsers = e.Parsers })
								}).ToList());
								await channel.SendMessageAsync("Ok, I will post this");
							}
							catch (JsonException e)
							{
								await channel.SendMessageAsync($"Well that is something I can't read: {e.Message}");
							}
						}
						break;
					case "grantmanager":
						var roles = message.MentionedRoles.Select(e => e.Id).ToList();
						await _repository.SetManagerRoles(guildId, roles);
						if (roles.Any())
							await channel.SendMessageAsync("Managers assigned");
						else
							await channel.SendMessageAsync("Access for admins only");
						break;
					default:
						await channel.SendMessageAsync("What?");
						break;
				}
			}
		}
	}
}
