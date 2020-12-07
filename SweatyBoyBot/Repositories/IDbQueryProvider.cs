using System;
using System.Collections.Generic;

namespace SweatyBoyBot.Repositories
{
	public interface IDbQueryProvider
	{
		Tuple<string, string> GetManagerRolesOptions(ulong guildId);
		string GetWorkItemsQueryText();
		Tuple<string, IEnumerable<Tuple<string, object>>> RemoveWorkItemsByChannelOptions(IEnumerable<ulong> channelIds);
		Tuple<string, IEnumerable<Tuple<string, object>>> SaveOrUpdateWorkItemsOptions(IEnumerable<WorkItem> items);
		Tuple<string, IEnumerable<Tuple<string, object>>> SetManagerRolesOptions(ulong guildId, IReadOnlyCollection<ulong> roleIds);
	}
}
