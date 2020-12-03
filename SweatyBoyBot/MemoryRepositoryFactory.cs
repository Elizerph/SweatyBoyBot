namespace SweatyBoyBot
{
	public class MemoryRepositoryFactory : IFactory<IRepository>
	{
		public IRepository Get()
		{
			return new MemoryRepository();
		}
	}
}
