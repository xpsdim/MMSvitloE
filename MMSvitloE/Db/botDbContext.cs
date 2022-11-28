using Microsoft.EntityFrameworkCore;

namespace MMSvitloE.Db
{
	public class BotDbContext : DbContext
	{
		public BotDbContext(DbContextOptions<BotDbContext> options)
			: base(options)
		{

		}

		public DbSet<Event> Events { get; set; }
	}
}
