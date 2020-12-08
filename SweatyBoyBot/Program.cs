using Discord;
using Discord.WebSocket;

using Microsoft.Data.Sqlite;

using SweatyBoyBot.Repositories;
using SweatyBoyBot.Repositories.DbQueryProviders;
using SweatyBoyBot.RepositoryFactories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	class Program
	{
		private BotShell _bot;

		public static void Main(string[] args)
		=> new Program().MainAsync(args).GetAwaiter().GetResult();

		// token = args[0]
		// dbType = args[1]
		// dbConnection = args[2]
		public async Task MainAsync(string[] args)
		{
			string token;
			IFactory<IRepository> repositoryFactory = new MemoryRepositoryFactory();
			if (args != null && args.Length > 0)
			{
				token = args[0];
				if (args.Length == 3)
				{
					var connection = DbConnections[args[1]];
					connection.ConnectionString = args[2];
					repositoryFactory = new DbRepositoryFactory
					{
						Connection = DbConnections[args[1]],
						QueryProvider = DbQueryProviders[args[1]]
					};
				}
			}
			else
			{
				token = Environment.GetEnvironmentVariable("SweatyBoyBotToken");
				foreach (var variable in DbTypes)
				{
					var connectionString = Environment.GetEnvironmentVariable(variable);
					if (!string.IsNullOrEmpty(connectionString))
					{
						var connection = DbConnections[variable];
						connection.ConnectionString = connectionString;
						repositoryFactory = new DbRepositoryFactory
						{ 
							Connection = connection,
							QueryProvider = DbQueryProviders[variable]
						};
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(token))
			{
				Console.WriteLine($"Environment variable SweatyBoyBotToken not found");
				return;
			}

			var discordClient = new DiscordSocketClient();
			discordClient.Log += Log;

			_bot = new BotShell(discordClient, token, new HttpClient(), repositoryFactory.Get());

			await _bot.Start();
			await _bot.Routine();
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

		private static readonly IEnumerable<string> DbTypes = new[] 
		{
			"SweatyBoyBotMSSQL",
			"SweatyBoyBotSQLite"
		};

		private static readonly Dictionary<string, IDbConnection> DbConnections = new Dictionary<string, IDbConnection>
		{
			{ "SweatyBoyBotMSSQL", new SqlConnection() },
			{ "SweatyBoyBotSQLite", new SqliteConnection() }
		};

		private static readonly Dictionary<string, IDbQueryProvider> DbQueryProviders = new Dictionary<string, IDbQueryProvider>
		{
			{ "SweatyBoyBotMSSQL", new SqlQueryProvider() },
			{ "SweatyBoyBotSQLite", new SqliteQueryProvider() }
		};
	}
}
