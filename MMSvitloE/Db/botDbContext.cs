using Microsoft.EntityFrameworkCore;
using System;

namespace MMSvitloE.Db
{
	public class BotDbContext : DbContext
	{
		public BotDbContext(DbContextOptions<BotDbContext> options)
			: base(options)
		{ }

		public DbSet<Event> Events { get; set; }
		public DbSet<Follower> Followers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Event>()
				.Property(f => f.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			modelBuilder.Entity<Event>()
				.HasIndex(p => p.DateUtc)
				.IsUnique(false);
		}
	}
}
