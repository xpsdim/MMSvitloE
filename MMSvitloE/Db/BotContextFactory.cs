using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using MMSvitloE.ConfigurationService;
using System;
using System.Configuration;

namespace MMSvitloE.Db
{
	public class BotContextFactory : IDesignTimeDbContextFactory<BotDbContext>
	{
		public static string ConnectionString;
		public BotDbContext CreateDbContext(string[] args)
		{
			var serverVersion = new MySqlServerVersion(new Version(10, 3, 35));
			var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();

			//for migrations generating only
			if (string.IsNullOrEmpty(ConnectionString))
			{
				var configuration = new ConfigurationServiceFactory().CreateInstance();
				ConnectionString = configuration["connectionStrings"];
			}

			optionsBuilder.UseMySql(ConnectionString, serverVersion);
				//TODO for debugging
				//.LogTo(Console.WriteLine, LogLevel.Information)
				//.EnableSensitiveDataLogging()
				//.EnableDetailedErrors();

			return new BotDbContext(optionsBuilder.Options);
		}
	}
}
