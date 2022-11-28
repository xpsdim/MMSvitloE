using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MMSvitloE.ConfigurationService
{
	public class ConfigurationServiceFactory
	{
		public IConfigurationRoot CreateInstance()
		{
			return  new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
				.AddJsonFile("appsettings.json", false)
				.Build();
		}
	}
}
