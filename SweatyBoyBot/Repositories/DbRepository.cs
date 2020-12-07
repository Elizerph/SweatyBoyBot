using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SweatyBoyBot.Repositories
{
	public class DbRepository : IRepository
	{
		private readonly IDbConnection _connection;
		private readonly IDbQueryProvider _queryProvider;

		public DbRepository(IDbConnection connection, IDbQueryProvider queryProvider)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
		}

		public void Dispose()
		{
			_connection.Dispose();
		}

		public IReadOnlyCollection<ulong> GetManagerRoles(ulong guildId)
		{
			var options = _queryProvider.GetManagerRolesOptions(guildId);
			return QueryWithParameters(options.Item1, new[] { Tuple.Create(options.Item2, guildId.ToString() as object) })
				.Select(e => ulong.Parse(e[0].ToString()))
				.ToList();
		}

		public IReadOnlyCollection<WorkItem> GetWorkItems()
		{
			return Query(_queryProvider.GetWorkItemsQueryText())
				.Select(e => new WorkItem
				{
					Id = int.Parse(e[0].ToString()),
					NextRun = e[1].ToString().ToSweatyDateTime(),
					Frequency = int.Parse(e[2].ToString()),
					ChannelId = e[3].ToString(),
					Content = e[4].ToString()
				})
				.ToList();
		}

		public Task RemoveWorkItemsByChannel(IReadOnlyCollection<ulong> channelIds)
		{
			if (channelIds.Any())
			{
				var options = _queryProvider.RemoveWorkItemsByChannelOptions(channelIds);
				using var command = CreateCommandWithParameters(options.Item1, options.Item2);
				command.ExecuteScalar();
			}
			return Task.CompletedTask;
		}

		public Task SaveOrUpdateWorkItems(IReadOnlyCollection<WorkItem> items)
		{
			if (items.Any())
			{
				var options = _queryProvider.SaveOrUpdateWorkItemsOptions(items);
				using var command = CreateCommandWithParameters(options.Item1, options.Item2);
				command.ExecuteScalar();
			}
			return Task.CompletedTask;
		}

		public Task SetManagerRoles(ulong guildId, IReadOnlyCollection<ulong> roleIds)
		{
			var options = _queryProvider.SetManagerRolesOptions(guildId, roleIds);
			using var command = CreateCommandWithParameters(options.Item1, options.Item2);
			command.ExecuteScalar();
			return Task.CompletedTask;
		}

		private IEnumerable<object[]> Query(string queryText)
		{
			return QueryWithParameters(queryText, Enumerable.Empty<Tuple<string, object>>());
		}

		private IDbCommand CreateCommandWithParameters(string queryText, IEnumerable<Tuple<string, object>> parameters)
		{
			var command = _connection.CreateCommand();
			command.CommandText = queryText;
			foreach (var parameterOption in parameters)
			{
				var parameter = command.CreateParameter();
				parameter.ParameterName = parameterOption.Item1;
				parameter.Value = parameterOption.Item2;
				command.Parameters.Add(parameter);
			}
			return command;
		}

		private IEnumerable<object[]> QueryWithParameters(string queryText, IEnumerable<Tuple<string, object>> parameters)
		{
			using var reader = CreateCommandWithParameters(queryText, parameters).ExecuteReader();
			while (reader.Read())
			{
				var record = new object[reader.FieldCount];
				for (var i = 0; i < reader.FieldCount; i++)
					record[i] = reader[i];
				yield return record;
			}
		}
	}
}
