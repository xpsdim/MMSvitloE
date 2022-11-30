using System;

namespace MMSvitloE.Db
{
	public class MessageLogItem
	{
		public int Id { get; set; }

		public virtual long FollowerId {get; set; }

		public virtual DateTime DateUtc { get; set; }

		public virtual string MessageSent { get; set; }
	}
}
