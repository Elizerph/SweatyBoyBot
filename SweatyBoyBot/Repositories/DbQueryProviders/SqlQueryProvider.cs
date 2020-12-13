using System;
using System.Collections.Generic;
using System.Linq;

namespace SweatyBoyBot.Repositories.DbQueryProviders
{
	public class SqlQueryProvider : IDbQueryProvider
	{
		public Tuple<string, string> GetManagerRolesOptions(ulong guildId)
		{
			return Tuple.Create("SELECT [RoleId] FROM [ManagerRole] WHERE [GuildId] = @guildId", "@guildId");
		}

		public string GetWorkItemsQueryText()
		{
			return "SELECT [Id], [NextRun], [Frequency], [ChannelId], [Content] FROM [WorkItem]";
		}

		public Tuple<string, IEnumerable<Tuple<string, object>>> RemoveWorkItemsByChannelOptions(IEnumerable<ulong> channelIds)
		{
			var channels = channelIds.ToList();
			return Tuple.Create($"DELETE FROM [WorkItem] WHERE [ChannelId] IN ({string.Join(',', channels.Select((e, i) => $"@chid{i}"))})",
				channels.Select((e, i) => Tuple.Create($"@chid{i}", e.ToString() as object)));
		}

		public Tuple<string, IEnumerable<Tuple<string, object>>> SaveOrUpdateWorkItemsOptions(IEnumerable<WorkItem> items)
		{
			var workItems = items.ToList();

			var queryLines = new[]
			{
				"MERGE WorkItem as T",
				"USING (VALUES",
				string.Join(',', workItems.Select((e, i) => $"(@id{i}, @nr{i}, @fr{i}, @chid{i}, @cnt{i})")),
				") as V ([Id], [NextRun], [Frequency], [ChannelId], [Content])",
				"ON T.[Id] = V.[Id]",
				"WHEN MATCHED THEN",
				"UPDATE SET T.[NextRun] = V.[NextRun], T.[Frequency] = V.[Frequency], T.[ChannelId] = V.[ChannelId], T.[Content] = V.[Content]",
				"WHEN NOT MATCHED THEN",
				"INSERT ([NextRun], [Frequency], [ChannelId], [Content])",
				"VALUES (V.[NextRun], V.[Frequency], V.[ChannelId], V.[Content]);"
			};

			var parameters = workItems.SelectMany((e, i) => new[] 
			{ 
				Tuple.Create($"@id{i}", e.Id as object),
				Tuple.Create($"@nr{i}", e.NextRun.ToString() as object),
				Tuple.Create($"@fr{i}", e.Frequency as object),
				Tuple.Create($"@chid{i}", e.ChannelId as object),
				Tuple.Create($"@cnt{i}", e.Content as object)
			});

			return Tuple.Create(string.Join(Environment.NewLine, queryLines), parameters);
		}

		public Tuple<string, IEnumerable<Tuple<string, object>>> SetManagerRolesOptions(ulong guildId, IReadOnlyCollection<ulong> roleIds)
		{
			var queryLines = new List<string>
			{
				"DELETE FROM [ManagerRole] WHERE [GuildId] = @guildId;"
			};

			if (roleIds.Any())
				queryLines.AddRange(new[] 
				{
					"INSERT INTO ManagerRole ([RoleId], [GuildId]) VALUES",
					string.Join(',', roleIds.Select((e, i) => $"(@roleId{i}, @guildId)"))
				});

			var parameters = new[] { Tuple.Create("@guildId", guildId.ToString() as object) }
			.Concat(roleIds.SelectMany((e, i) => new[]
				{
					Tuple.Create($"@roleId{i}", e.ToString() as object)
				}));

			return Tuple.Create(string.Join(Environment.NewLine, queryLines), parameters);
		}
	}
}
