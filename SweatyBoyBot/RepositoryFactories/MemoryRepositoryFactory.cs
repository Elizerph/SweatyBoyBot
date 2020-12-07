using SweatyBoyBot.Repositories;

namespace SweatyBoyBot.RepositoryFactories
{
	public class MemoryRepositoryFactory : IFactory<IRepository>
	{
		public IRepository Get()
		{
			return new MemoryRepository();
		}
	}
}
