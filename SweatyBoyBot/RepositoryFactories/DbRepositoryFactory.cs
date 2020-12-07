using SweatyBoyBot.Repositories;

using System.Data;

namespace SweatyBoyBot.RepositoryFactories
{
	public class DbRepositoryFactory : IFactory<IRepository>
	{
		public IDbConnection Connection { get; set; }
		public IDbQueryProvider QueryProvider { get; set; }

		public IRepository Get()
		{
			Connection.Open();
			return new DbRepository(Connection, QueryProvider);
		}
	}
}
