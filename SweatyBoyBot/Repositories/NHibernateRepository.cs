//using FluentNHibernate.Cfg;
//using FluentNHibernate.Cfg.Db;

//using NHibernate;
//using NHibernate.Dialect;
//using NHibernate.Tool.hbm2ddl;

//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace SweatyBoyBot.Repositories
//{
//	public class NHibernateRepository : IRepository
//	{
//		private readonly ISessionFactory _sessionFactory;

//		public NHibernateRepository(string connectionString)
//		{
//			_sessionFactory = Fluently.Configure()
//				.Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).Dialect<MsSql2012Dialect>())
//				.Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
//				.ExposeConfiguration(c => SchemaMetadataUpdater.QuoteTableAndColumns(c, new MsSql2012Dialect()))
//				.BuildSessionFactory();
//		}

//		public IReadOnlyCollection<WorkItem> GetWorkItems()
//		{
//			using var session = _sessionFactory.OpenSession();
//			return session.Query<WorkItem>().ToList();
//		}

//		public async Task RemoveWorkItemsByChannel(IReadOnlyCollection<ulong> channelIds)
//		{
//			if (!channelIds.Any())
//				return;
//			using var session = _sessionFactory.OpenSession();
//			await session.CreateSQLQuery("DELETE FROM [WorkItem] WHERE [ChannelId] IN (:p)")
//				.SetParameterList("p", channelIds.Select(e => e.ToString()))
//				.ExecuteUpdateAsync();
//			session.Flush();
//		}

//		public async Task SaveOrUpdateWorkItems(IReadOnlyCollection<WorkItem> items)
//		{
//			if (!items.Any())
//				return;
//			using var session = _sessionFactory.OpenSession();
//			foreach (var item in items)
//				await session.SaveOrUpdateAsync(item);
//			await session.FlushAsync();
//		}

//		public IReadOnlyCollection<ulong> GetManagerRoles(ulong guildId)
//		{
//			using var session = _sessionFactory.OpenSession();
//			return session.CreateSQLQuery("SELECT [RoleId] FROM [ManagerRole] WHERE [GuildId] = :p")
//				.SetParameter("p", guildId.ToString())
//				.List<string>()
//				.Select(e => ulong.Parse(e))
//				.ToList();
//		}

//		public async Task SetManagerRoles(ulong guildId, IReadOnlyCollection<ulong> roleIds)
//		{
//			using var session = _sessionFactory.OpenSession();
//			await session.CreateSQLQuery("DELETE FROM [ManagerRole] WHERE [GuildId] = (:p)")
//				.SetParameter("p", guildId.ToString())
//				.ExecuteUpdateAsync();
//			foreach (var roleId in roleIds)
//				await session.SaveOrUpdateAsync(new ManagerRole { RoleId = roleId.ToString(), GuildId = guildId.ToString() });
//			await session.FlushAsync();
//		}
//	}
//}
