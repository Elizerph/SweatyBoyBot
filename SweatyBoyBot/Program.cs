using Discord;
using Discord.WebSocket;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	class Program
	{
		private BotShell _bot;

		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			IFactory<IRepository> repositoryFactory;
			var connectionString = Environment.GetEnvironmentVariable("SweatyBoyBotConnectionString");
			if (string.IsNullOrEmpty(connectionString))
				repositoryFactory = new MemoryRepositoryFactory();
			else
				repositoryFactory = new NHibernateRepositoryFactory { ConnectionString = connectionString };

			var token = Environment.GetEnvironmentVariable("SweatyBoyBotToken");

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
	}
}
