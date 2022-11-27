using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace MMSvitloE
{
	class Program
	{
		public static ITelegramBotClient bot = null;
		public static IConfigurationRoot configuration;
		public static DateTime? StatusChangedAtUtc = null;
		public static bool Status = true;
		public static TimeZoneInfo KyivTimezone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");

		public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
			{
				var message = update.Message;
				if (message.Text?.ToLower() == "/start")
				{
					var timeMsgPart = string.Empty;
					if (StatusChangedAtUtc.HasValue)
					{
						var period = DateTime.UtcNow - StatusChangedAtUtc.Value;
						var periodStr = period.TimespanToReadableStr();
						timeMsgPart = $"з {TimeZoneInfo.ConvertTimeFromUtc(StatusChangedAtUtc.Value, KyivTimezone):HH:mm dd.MM.yyyy}{periodStr}";
					}
					var msg = $"Нема :( {timeMsgPart}";
					if (Status)
					{
						msg = $"Є! {timeMsgPart}";
					}

					Console.WriteLine($"{message.From.FirstName} {message.From.LastName} - {msg}");
					await botClient.SendTextMessageAsync(message.Chat, msg);
					return;
				}
			}
		}

		public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			// Just write error to console
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
		}

		public static void InitConfiguration()
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			// Build configuration
			configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
				.AddJsonFile("appsettings.json", false)
				.Build();

			// Add access to generic IConfigurationRoot
			serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
			bot = new TelegramBotClient(configuration["botToken"]);
		}

		public static void ReadStatus()
		{
			var newStatus = new Utils().PingHost(configuration["ipToPing"]);
			var now = DateTime.UtcNow;
			if (newStatus != Status)
			{
				Status = newStatus;
				StatusChangedAtUtc = DateTime.UtcNow;
			}
			//TODO comment it after testing
			//Console.WriteLine($"new status: {newStatus}: {TimeZoneInfo.ConvertTimeFromUtc(now, KyivTimezone):HH:mm dd.MM.yyyy}");
		}

		static void Main(string[] args)
		{
			InitConfiguration();

			var timer = new Timer(
				e => ReadStatus(),
				null,
				TimeSpan.Zero,
				TimeSpan.FromMinutes(1));

			var cts = new CancellationTokenSource();
			var cancellationToken = cts.Token;
			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = { },
			};
			bot.StartReceiving(
				HandleUpdateAsync,
				HandleErrorAsync,
				receiverOptions,
				cancellationToken
			);
			Console.WriteLine("Bot Started " + bot.GetMeAsync().Result.FirstName);
			Thread.Sleep(Timeout.Infinite);
		}
	}
}
