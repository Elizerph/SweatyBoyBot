using System.Collections.Generic;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	public interface IRepository
	{
		IReadOnlyCollection<WorkItem> GetWorkItems();
		Task RemoveWorkItemsByChannel(IReadOnlyCollection<ulong> channelIds);
		Task SaveOrUpdateWorkItems(IReadOnlyCollection<WorkItem> items);
		Task SetManagerRoles(ulong guildId, IReadOnlyCollection<ulong> roleIds);
		IReadOnlyCollection<ulong> GetManagerRoles(ulong guildId);
	}
}
