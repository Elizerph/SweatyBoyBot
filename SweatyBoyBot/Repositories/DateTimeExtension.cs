using System;
using System.Globalization;

namespace SweatyBoyBot.Repositories
{
	public static class DateTimeExtension
	{
		private const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

		public static string ToSweatyString(this DateTime instance)
		{
			return instance.ToString(DateTimeFormat);
		}

		public static DateTime ToSweatyDateTime(this string instance)
		{
			return DateTime.ParseExact(instance, DateTimeFormat, CultureInfo.InvariantCulture);
		}
	}
}
