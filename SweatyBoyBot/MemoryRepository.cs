using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SweatyBoyBot
{
	public class MemoryRepository : IRepository
	{
		private readonly Dictionary<ulong, IReadOnlyCollection<ulong>> _managerRoles = new Dictionary<ulong, IReadOnlyCollection<ulong>>();
		private List<WorkItem> _workItems = new List<WorkItem>();

		public IReadOnlyCollection<ulong> GetManagerRoles(ulong guildId)
		{
			return _managerRoles.TryGetValue(guildId, out var roleIds) ? roleIds : Array.Empty<ulong>();
		}

		public IReadOnlyCollection<WorkItem> GetWorkItems()
		{
			return _workItems.ToList();
		}

		public Task RemoveWorkItemsByChannel(IReadOnlyCollection<ulong> channelIds)
		{
			var hashset = new HashSet<string>(channelIds.Select(e => e.ToString()));
			_workItems = _workItems.Where(e => !hashset.Contains(e.ChannelId)).ToList();
			return Task.CompletedTask;
		}

		public Task SaveOrUpdateWorkItems(IReadOnlyCollection<WorkItem> items)
		{
			var newItems = items.Except(_workItems);
			_workItems.AddRange(newItems);
			return Task.CompletedTask;
		}

		public Task SetManagerRoles(ulong guildId, IReadOnlyCollection<ulong> roleIds)
		{
			_managerRoles[guildId] = roleIds;
			return Task.CompletedTask;
		}
	}
}
