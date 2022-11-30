using System;

namespace MMSvitloE.Db
{
	public class Follower
	{
		public long Id { get; set; }

		public virtual bool IsBot { get; set; }

		public virtual string FirstName { get; set; } = default!;

		public virtual string LastName { get; set; }

		public virtual string Username { get; set; }

		public virtual string LanguageCode { get; set; }

		public virtual bool? CanJoinGroups { get; set; }
		
		public virtual bool? CanReadAllGroupMessages { get; set; }

		public virtual bool? SupportsInlineQueries { get; set; }

		public virtual bool FollowingSvitloBot { get; set; }

		public virtual DateTime? StartedFollowUtc { get; set; }

		public virtual DateTime? StartedUnfollowUtc { get; set; }
	}
}
