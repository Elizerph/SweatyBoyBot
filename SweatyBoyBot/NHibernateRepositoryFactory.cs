namespace SweatyBoyBot
{
	public class NHibernateRepositoryFactory : IFactory<IRepository>
	{
		public string ConnectionString { get; set; }

		public IRepository Get()
		{
			return new NHibernateRepository(ConnectionString);
		}
	}
}
