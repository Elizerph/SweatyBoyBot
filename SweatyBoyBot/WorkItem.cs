using FluentNHibernate.Mapping;

using System;

namespace SweatyBoyBot
{
	public class WorkItem
	{
		public virtual int Id { get; set; }
		public virtual int Frequency { get; set; }
		public virtual DateTime NextRun { get; set; }
		public virtual string ChannelId { get; set; }
		public virtual string Content { get; set; }
	}

	public class WorkItemMap : ClassMap<WorkItem>
	{
		public WorkItemMap()
		{
			Table("WorkItem");
			Id(x => x.Id).GeneratedBy.Native();
			Map(x => x.Frequency).Not.Nullable();
			Map(x => x.NextRun).Not.Nullable();
			Map(x => x.ChannelId).Not.Nullable();
			Map(x => x.Content).Not.Nullable();
		}
	}
}
