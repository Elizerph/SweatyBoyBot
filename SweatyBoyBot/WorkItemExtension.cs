using Newtonsoft.Json;

namespace SweatyBoyBot
{
	public static class WorkItemExtension
	{
		public static WorkItemPost GetPost(this WorkItem instance)
		{
			return JsonConvert.DeserializeObject<WorkItemPost>(instance.Content);
		}

		public static void SetPost(this WorkItem instance, WorkItemPost post)
		{
			instance.Content = JsonConvert.SerializeObject(post);
		}
	}
}
