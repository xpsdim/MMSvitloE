namespace Statistic
{
	internal class OutageEvent
	{
		public int EventType { get; set; }
		public DateTime Date { get; set; }

		public DateTime KyivDate =>
			TimeZoneInfo.ConvertTimeFromUtc(Date, TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"));
	}
}
