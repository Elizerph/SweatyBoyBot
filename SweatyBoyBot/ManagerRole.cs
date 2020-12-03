using FluentNHibernate.Mapping;

namespace SweatyBoyBot
{
	public class ManagerRole
	{
		public virtual string GuildId { get; set; }
		public virtual string RoleId { get; set; }
	}

	public class ManagerRoleMap : ClassMap<ManagerRole>
	{
		public ManagerRoleMap()
		{
			Table("ManagerRole");
			Id(x => x.RoleId);
			Map(x => x.GuildId);
		}
	}
}
