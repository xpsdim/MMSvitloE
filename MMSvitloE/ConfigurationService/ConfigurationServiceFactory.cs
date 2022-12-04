using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MMSvitloE.ConfigurationService
{
	public class ConfigurationServiceFactory
	{
		public static string CurrentEnvironment = "Debug";

		static ConfigurationServiceFactory()
		{
#if RELEASE
			CurrentEnvironment = "Release";
#endif
		}

		public IConfigurationRoot CreateInstance()
		{
			return  new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
				.AddJsonFile($"appsettings-{CurrentEnvironment}.json", false)
				.Build();
		}
	}
}
