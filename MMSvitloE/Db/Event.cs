using System;

namespace MMSvitloE.Db
{
	public class Event
	{
		public int Id { get; set; }

		public virtual EventTypesEnum EventType { get; set; }

		public virtual DateTime DateUtc { get; set; }
	}
}
